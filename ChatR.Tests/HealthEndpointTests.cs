using System.Net;
namespace Chatr.Tests;

public class HealthEndpointTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Root_Returns_Ok_Text()
    {
        var client = factory.CreateClient();
        var resp = await client.GetAsync("/");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var text = await resp.Content.ReadAsStringAsync();
        text.Trim('"').Should().Be("And it's up.");
    }
}
