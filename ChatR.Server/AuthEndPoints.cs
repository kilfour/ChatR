using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ChatR.Server;

public class AuthEndPoints
{
    public const string issuer = "chatr";
    public const string audience = "chatr-client";
    public const string jwtKey = "dev-super-secret-key-32b-minimum-length!";

    public static IResult Register(LoginRequest loginRequest, Doorman doorman)
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
    }

    public static IResult Login(LoginRequest loginRequest, Doorman doorman)
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
    }

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
}