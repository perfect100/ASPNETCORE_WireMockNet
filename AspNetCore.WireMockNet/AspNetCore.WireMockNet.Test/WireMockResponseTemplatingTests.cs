using System.Net.Http;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace AspNetCore.WireMockNet.Test
{
    public class WireMockResponseTemplatingTests : TestBase
    {
        [Fact]
        public async Task RequestTemplatingTest()
        {
            var server = WireMockServer.Start();
            server
                .Given(Request.Create()
                    .WithPath("/some/thing").UsingGet())
                .RespondWith(Response.Create()
                    .WithBody("Hello world! Your path is {{request.path}}.")
                    // do not forget!
                    .WithTransformer());
            
            var client = new HttpClient();
            var body = await client.GetStringAsync($"{server.Urls[0]}/some/thing");
            Assert.Equal("Hello world! Your path is /some/thing.", body);

            await Assert.ThrowsAsync<HttpRequestException>(async () => 
                await client.GetStringAsync($"{server.Urls[0]}/some/thing/1"));
        }
    }
}