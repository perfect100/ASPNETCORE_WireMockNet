using System.Net.Http;
using System.Threading.Tasks;
using RestEase;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using Xunit;

namespace AspNetCore.WireMockNet.Test
{
    public class WireMockMappingTests : TestBase
    {
        [Fact]
        public async Task RequestMatchWithCodeOptionTest()
        {
            var server = WireMockServer.Start(9090);
            server
                .Given(Request.Create()
                    .WithPath(MatchBehaviour.AcceptOnMatch,"/exact")
                    .WithParam("from", "abc")
                    .WithParam("to", new ExactMatcher("def")))
                .RespondWith(Response.Create()
                    .WithBody("Exact match"));
            var obj = await new HttpClient().GetStringAsync($"http://localhost:9090/exact?from=abc&to=def");
            Assert.Equal("Exact match", obj);
        }
        
        [Fact]
        public async Task RequestMatchWithCodeJsonTest()
        {
            var settings = new WireMockServerSettings()
            {
                Port = 9090,
                ReadStaticMappings = true
            };
            var server = WireMockServer.Start(settings);
            var obj = await new HttpClient().GetStringAsync($"http://localhost:9090/exact?from=abc&to=def");
            Assert.Equal("Exact match", obj);
        }

        [Fact]
        public void RequestWithFullFunctions()
        {
            var server = WireMockServer.Start(9090);
            server
                .Given(Request.Create()
                    .WithBody("body")
                    .WithUrl("http://localhost:9090/api/user/1")
                    .WithCookie("optimizelyEndUserId", "oeu1606971576280r0.6933875023356342", true)
                    .WithHeader("Connection", new ExactMatcher("keep-live"))
                    .WithClientIP("10.78.90.121")
                    .WithPath(MatchBehaviour.AcceptOnMatch, "/exact")
                    .WithParam("from", "abc")
                    .WithParam("to", new ExactMatcher("def"))

                    .UsingAnyMethod()
                    .UsingGet()

                )
                .RespondWith(Response.Create()
                    .WithBody("Exact match")
                    .WithBodyAsJson(@"{
                                        ""Name"":""Tom"",
                                        ""Age"": 18
                                      }"
                    )
                    .WithHeader("Cache-Control", "public, max-age=86400")
                    .WithStatusCode(201)
                    .WithDelay(2000)
                    .WithSuccess()
                    .WithFault(FaultType.MALFORMED_RESPONSE_CHUNK, 0.5)
                    .WithNotFound()
                );
        }
    }
}