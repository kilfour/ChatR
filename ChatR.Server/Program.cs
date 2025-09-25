using System.Text;
using ChatR.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


const string CorsPolicy = "Elm";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthEndPoints.issuer,
            ValidateAudience = true,
            ValidAudience = AuthEndPoints.audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthEndPoints.jwtKey)),
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

app.MapPost("/api/register", (LoginRequest loginRequest, Doorman doorman)
    => AuthEndPoints.Register(loginRequest, doorman));

app.MapPost("/api/login", (LoginRequest loginRequest, Doorman doorman)
    => AuthEndPoints.Login(loginRequest, doorman));

app.MapHub<ChatHub>("/chat").RequireAuthorization();
app.Run();

