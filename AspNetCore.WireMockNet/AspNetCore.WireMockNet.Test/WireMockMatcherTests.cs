using System.Net.Http;
using System.Threading.Tasks;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace AspNetCore.WireMockNet.Test
{
    public class WireMockMatcherTests : TestBase
    {
        [Fact]
        public async Task ExactMatcherTest()
        {
            var server = WireMockServer.Start();
            server
                .Given(Request.Create().WithPath(new ExactMatcher("/exact")))
                .RespondWith(Response.Create().WithBody("ExactMatch"));
            
            var client = new HttpClient();
            var body = await client.GetStringAsync($"{server.Urls[0]}/exact");
            Assert.Equal("ExactMatch", body);
            
            await Assert.ThrowsAsync<HttpRequestException>(async () => 
                await client.GetStringAsync($"{server.Urls[0]}/Exact"));
        }

        [Fact]
        public async Task WildcardMatcherTest()
        {
            var server = WireMockServer.Start();
            server
                .Given(Request.Create()
                    // a ? for a single character and * for any characters.
                    .WithPath( new WildcardMatcher("/some*", true)))
                .RespondWith(Response.Create().WithBody("wildcard match"));
            
            var client = new HttpClient();
            var body = await client.GetStringAsync($"{server.Urls[0]}/something");
            Assert.Equal("wildcard match", body);
            
            body = await client.GetStringAsync($"{server.Urls[0]}/Something");
            Assert.Equal("wildcard match", body);

            await Assert.ThrowsAsync<HttpRequestException>(async () => 
                await client.GetStringAsync($"{server.Urls[0]}/nothing"));
        }
        
        [Fact]
        public async Task RejectMatcherTest()
        {
            var server = WireMockServer.Start();
            server
                .Given(Request.Create()
                    .WithPath( MatchBehaviour.RejectOnMatch, "/api/user/0/"))
                .RespondWith(Response.Create().WithBody("reject matcher"));
            
            var client = new HttpClient();
            var body = await client.GetStringAsync($"{server.Urls[0]}/api/user/10/");
            Assert.Equal("reject matcher", body);

            await Assert.ThrowsAsync<HttpRequestException>(async () => 
                await client.GetStringAsync($"{server.Urls[0]}/api/user/0/"));
        }

        [Fact]
        public async Task LinqMatcherTest()
        {
            var server = WireMockServer.Start();
            server
                .Given(Request.Create()
                    .WithPath("/linq")
                    .WithParam("id", new LinqMatcher("int.Parse(it) > 1")))
                .RespondWith(Response.Create().WithBody("linq match"));
            
            var client = new HttpClient();
            var body = await client.GetStringAsync($"{server.Urls[0]}/linq?id=100");
            Assert.Equal("linq match", body);

            await Assert.ThrowsAsync<HttpRequestException>(async () => 
                await client.GetStringAsync($"{server.Urls[0]}/linq?id=1"));
        }

        [Fact]
        public async Task RegularMatcherTest()
        {
            var server = WireMockServer.Start(9090);
            server
                .Given(Request.Create()
                    .WithPath("/reg")
                    // user must start with word, end with digit
                    .WithParam("user", new RegexMatcher(@"^\w.*\d+$")))
                .RespondWith(Response.Create().WithBody("reg match"));
            
            var client = new HttpClient();
            var body = await client.GetStringAsync("http://localhost:9090/reg?user=Tom1993");
            Assert.Equal("reg match", body);

            await Assert.ThrowsAsync<HttpRequestException>(async () => 
                await client.GetStringAsync("http://localhost:9090/reg?user=Tom1993nice"));
        }
    }
}