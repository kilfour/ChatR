using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatR.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


const string CorsPolicy = "Elm";

var builder = WebApplication.CreateBuilder(args);
const string issuer = "chatr";
const string audience = "chatr-client";
const string jwtKey = "dev-super-secret-key-32b-minimum-length!";
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy
            .WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});
builder.Services.AddSingleton<RoomTracker>();
builder.Services.AddSingleton<Doorman>();

var app = builder.Build();
app.UseRouting();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/register", (LoginRequest loginRequest, Doorman doorman) =>
{
    if (string.IsNullOrWhiteSpace(loginRequest.Username))
        return Results.BadRequest(new { error = "Username can not be empty." });
    if (string.IsNullOrWhiteSpace(loginRequest.Password))
        return Results.BadRequest(new { error = "Password can not be empty." });
    if (doorman.Get(loginRequest.Username) != null)
        return Results.BadRequest(new { error = "Username not available." });
    doorman.Set(loginRequest.Username, new Chatterer(loginRequest.Username, loginRequest.Password));
    var token = GetToken(issuer, audience, jwtKey, GetClaims(loginRequest));
    return Results.Json(new { token });
});

app.MapPost("/api/login", (LoginRequest loginRequest, Doorman doorman) =>
{
    if (string.IsNullOrWhiteSpace(loginRequest.Username))
        return Results.BadRequest(new { error = "Invalid credentials." });
    var chatterer = doorman.Get(loginRequest.Username);
    if (chatterer == null)
        return Results.BadRequest(new { error = "Invalid credentials." });
    if (chatterer.Pass != loginRequest.Password)
        return Results.BadRequest(new { error = "Invalid credentials." });
    var token = GetToken(issuer, audience, jwtKey, GetClaims(loginRequest));
    return Results.Json(new { token });
});

app.MapHub<ChatHub>("/chat").RequireAuthorization();
app.Run();

static string GetToken(string issuer, string audience, string jwtKey, Claim[] claims)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var jwt = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: creds);
    var token = new JwtSecurityTokenHandler().WriteToken(jwt);
    return token;
}

static Claim[] GetClaims(LoginRequest loginRequest)
    => [
        new Claim(ClaimTypes.Name, loginRequest.Username),
        new Claim("uid", Guid.NewGuid().ToString())
    ];