using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.IdentityModel.Tokens;
using ChatR.Server;

namespace Chatr.Tests;

public class SignalRAuthAndMessagingTests
{
    private static string CreateJwt(string user = "alice")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthEndPoints.jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user),
            new Claim("uid", Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            issuer: AuthEndPoints.issuer,
            audience: AuthEndPoints.audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task Connect_JoinRoom_SendToRoom_Should_Broadcast_To_Group()
    {
        using var factory = new CustomWebApplicationFactory(); // fresh server per test
        using var server = factory.Server;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var baseAddress = server.BaseAddress!.ToString();
        var handler = server.CreateHandler();
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        var jwt = CreateJwt("alice");

        var connection = new HubConnectionBuilder()
            .WithUrl(baseAddress + "chat", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(jwt)!;
                options.Transports = HttpTransportType.LongPolling; // works over TestServer
                options.HttpMessageHandlerFactory = _ => handler;   // bind to this server instance
            })
            .Build();

        connection.On<string, string, string>("ReceiveRoomMessage", (room, user, message) =>
        {
            if (room == "room1")
                tcs.TrySetResult($"{user}:{message}");
        });

        await connection.StartAsync(cts.Token);
        connection.State.Should().Be(HubConnectionState.Connected);

        await connection.InvokeAsync("JoinRoom", "room1", cancellationToken: cts.Token);
        await connection.InvokeAsync("SendToRoom", "room1", "alice", "hello", cts.Token);

        var received = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        received.Should().Be("alice:hello");

        // Stop connection before disposing server to avoid TestServer disposed exceptions
        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task Connecting_Without_Token_Should_Fail()
    {
        using var factory = new CustomWebApplicationFactory();
        using var server = factory.Server;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var baseAddress = server.BaseAddress!.ToString();
        var handler = server.CreateHandler();

        var connection = new HubConnectionBuilder()
            .WithUrl(baseAddress + "chat", options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => handler;
            })
            .Build();

        var ex = await Assert.ThrowsAsync<HttpRequestException>(async () => await connection.StartAsync(cts.Token));
        ex.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        await connection.DisposeAsync();
    }
}