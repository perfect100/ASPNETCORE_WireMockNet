using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace AspNetCore.WireMockNet.Test
{
    public class WireMockScenarioTests : TestBase
    {
        [Fact]
        public async Task ScenarioWithCodeTest()
        {
            // Assign
            var server = WireMockServer.Start();

            server
                .Given(Request.Create()
                    .WithPath("/todo/items")
                    .UsingGet())
                .InScenario("To do list")
                .WillSetStateTo("TodoList State Started")
                .RespondWith(Response.Create()
                    .WithBody("Buy milk"));

            server
                .Given(Request.Create()
                    .WithPath("/todo/items")
                    .UsingPost())
                .InScenario("To do list")
                .WhenStateIs("TodoList State Started")
                .WillSetStateTo("Cancel newspaper item added")
                .RespondWith(Response.Create()
                    .WithStatusCode(201));

            server
                .Given(Request.Create()
                    .WithPath("/todo/items")
                    .UsingGet())
                .InScenario("To do list")
                .WhenStateIs("Cancel newspaper item added")
                .RespondWith(Response.Create()
                    .WithBody("Buy milk;Cancel newspaper subscription"));

            // Act and Assert
            var client = new HttpClient();
            string url = server.Urls[0];
            string getResponse1 = await client.GetStringAsync(url + "/todo/items");
            Assert.Equal("Buy milk", getResponse1);

            var postResponse = await client.PostAsync(url + "/todo/items", new StringContent("Cancel newspaper subscription"));
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

            string getResponse2 = await client.GetStringAsync(url + "/todo/items");
            Assert.Equal("Buy milk;Cancel newspaper subscription", getResponse2);
        }
        
        [Fact]
        public async Task ScenarioWithJsonTest()
        {
            // Assign
            var server = WireMockServer.StartWithAdminInterfaceAndReadStaticMappings("http://localhost");

            // Act and Assert
            var client = new HttpClient();
            string url = server.Urls[0];
            string getResponse1 = await client.GetStringAsync(url + "/todo/items");
            Assert.Equal("Buy milk", getResponse1);

            var postResponse = await client.PostAsync(url + "/todo/items", new StringContent("Cancel newspaper subscription"));
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

            string getResponse2 = await client.GetStringAsync(url + "/todo/items");
            Assert.Equal("Buy milk;Cancel newspaper subscription", getResponse2);
        }
    }
}