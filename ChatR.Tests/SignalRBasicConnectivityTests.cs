using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chatr.Tests;

public class SignalRBasicConnectivityTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact(Skip = "currently fails coz auth not set up in test")]
    public async Task Hub_Connects_With_LongPolling()
    {
        using var server = factory.Server;
        var baseAddress = factory.Server.BaseAddress?.ToString() ?? "http://localhost";
        var connection = new HubConnectionBuilder()
        .WithUrl(baseAddress + "chat", options =>
        {
            options.Transports = HttpTransportType.LongPolling;
            options.HttpMessageHandlerFactory = _ => server.CreateHandler();
        })
        .Build();
        await connection.StartAsync();
        connection.State.Should().Be(HubConnectionState.Connected);
        await connection.DisposeAsync();
    }
}