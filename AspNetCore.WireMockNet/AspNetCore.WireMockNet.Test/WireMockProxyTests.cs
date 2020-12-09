using System.Net.Http;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using Xunit;

namespace AspNetCore.WireMockNet.Test
{
    public class WireMockProxyTests : TestBase
    {
        
        [Fact]
        public async Task GlobalProxyTest()
        {
            var settings = new WireMockServerSettings
            {
                Urls = new[] { "http://localhost/" },
                StartAdminInterface = true,
                ReadStaticMappings = true,
                ProxyAndRecordSettings = new ProxyAndRecordSettings
                {
                    Url = "http://www.bbc.com",
                    SaveMapping = true,
                    SaveMappingToFile = true,
                    SaveMappingForStatusCodePattern = "2xx"
                }
            };
            var server = WireMockServer.Start(settings);
            // please access: http://localhost:9090/earth/story/20170510-terrifying-20m-tall-rogue-waves-are-actually-real
            // and refresh the __admin/mappings/ directory and see the files added
            this.OpenUrl($"{server.Urls[0]}/earth/story/20170510-terrifying-20m-tall-rogue-waves-are-actually-real");
            await this.WaitingForResponse();
        }

        [Fact]
        public async Task ProxyInterceptionTest()
        {
            var server = WireMockServer.Start();
            server
                .Given(Request.Create().WithPath("/*"))
                .AtPriority(10)
                .RespondWith(Response.Create().WithBody("Pass"));
                    // proxy first
                    // .WithProxy("http://otherhost.com"));
            server
                .Given(Request.Create().WithPath("/api/override/123"))
                .AtPriority(1)
                .RespondWith(Response.Create().WithStatusCode(503).WithBody("Error"));
            this.OpenUrl($"{server.Urls[0]}/some/thing/");
            this.OpenUrl($"{server.Urls[0]}/api/override/123");
            await this.WaitingForResponse();
        }
        
        [Fact]
        public async Task ProxyInterceptionTestWithSameMapping()
        {
            var server = WireMockServer.Start();
            server
                .Given(Request.Create().WithPath("/api/"))
                .RespondWith(Response.Create().WithBody("Error"));
            server
                .Given(Request.Create().WithPath("/api/"))
                .RespondWith(Response.Create().WithBody("Pass"));

            var client = new HttpClient();
            var rest1 = await client.GetStringAsync($"{server.Urls[0]}/api/");
            Assert.Equal("Pass", rest1);
        }
    }
}