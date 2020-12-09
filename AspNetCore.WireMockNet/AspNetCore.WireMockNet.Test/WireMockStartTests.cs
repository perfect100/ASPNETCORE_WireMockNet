using System.Diagnostics;
using System.Threading.Tasks;
using RestEase;
using WireMock.Client;
using WireMock.Exceptions;
using WireMock.Server;
using WireMock.Settings;
using Xunit;

namespace AspNetCore.WireMockNet.Test
{
    public class WireMockStartTests : TestBase
    {
        [Fact]
        public void StartByDefaultWithRandomPort()
        {
            var server = WireMockServer.Start();
            int usedPort = server.Ports[0];
            string usedUrl = server.Urls[0];
            
            Assert.True(server.IsStarted);
            Assert.StartsWith("http://", usedUrl);
            Assert.Empty(server.Scenarios);
            Assert.Empty(server.MappingModels);
            Assert.Empty(server.Mappings);
            Assert.Empty(server.LogEntries);
        }
        
        [Fact]
        public void StartByDefaultWithSpecificPort()
        {
            var server = WireMockServer.Start(10086);
            int usedPort = server.Ports[0];
            string usedUrl = server.Urls[0];
            
            Assert.Equal(10086, usedPort);
            Assert.StartsWith("http://", usedUrl);
            Assert.True(server.IsStarted);
            Assert.Empty(server.Scenarios);
            Assert.Empty(server.MappingModels);
            Assert.Empty(server.Mappings);
            Assert.Empty(server.LogEntries);
        }
        
        [Fact]
        public void StartByDefaultWithExistsPort()
        {
            var server = WireMockServer.Start(10086);
            var server2 = WireMockServer.Start(10086);
            Assert.Throws<WireMockException>(() => WireMockServer.Start(10086));
        }
        
        [Fact]
        public void StartByDefaultWithSpecificPortAndSsl()
        {
            var server = WireMockServer.Start(10086, true);
            int usedPort = server.Ports[0];
            string usedUrl = server.Urls[0];
            
            Assert.Equal(10086, usedPort);
            Assert.StartsWith("https://", usedUrl);
            Assert.True(server.IsStarted);
            Assert.Empty(server.Scenarios);
            Assert.Empty(server.MappingModels);
            Assert.Empty(server.Mappings);
            Assert.Empty(server.LogEntries);
        }
        
        [Fact]
        public void StartByDefaultWithUrls()
        {
            var server = WireMockServer.Start(
                "http://localhost:10086", 
                "https://localhost:10087");
            int usedPort = server.Ports[0];
            string usedUrl = server.Urls[0];
            
            Assert.Equal(10086, server.Ports[0]);
            Assert.Equal(10087, server.Ports[1]);
            Assert.Equal("http://localhost:10086", server.Urls[0]);
            Assert.Equal("https://localhost:10087", server.Urls[1]);
            Assert.True(server.IsStarted);
            Assert.Empty(server.Scenarios);
            Assert.Empty(server.MappingModels);
            Assert.Empty(server.Mappings);
            Assert.Empty(server.LogEntries);
        }
        
        [Fact]
        public void StartByDefaultWithSettings()
        {
            var server = WireMockServer.Start(new WireMockServerSettings()
            {
                AdminPassword = "123",
                AdminUsername = "admin",
                AllowBodyForAllHttpMethods = true,
                AllowCSharpCodeMatcher = true,
                AllowOnlyDefinedHttpStatusCodeInResponse = true,
                AllowPartialMapping = true,
                CertificateSettings = null,
                DisableJsonBodyParsing = true,
                DisableRequestBodyDecompressing = true,
                FileSystemHandler = null,
                UseSSL = true,
                StartAdminInterface = true,
                ReadStaticMappings = true
            });
            int usedPort = server.Ports[0];
            string usedUrl = server.Urls[0];
            
            Assert.StartsWith("https://", usedUrl);
            Assert.True(server.IsStarted);
            Assert.Empty(server.Scenarios);
            Assert.Empty(server.MappingModels);
            // Assert.Empty(server.Mappings); // default size : 24
            Assert.Empty(server.LogEntries);
        }

        [Fact]
        public async Task StartWithAdminInterfaceAndAccessWithInnerApi()
        {
            var server = WireMockServer.StartWithAdminInterface();
            var api = RestClient.For<IWireMockAdminApi>(server.Urls[0]);
            var settings = await api.GetSettingsAsync();
            var mappings = await api.GetMappingsAsync();
            Assert.Empty(mappings);
        }
        
        [Fact]
        public async Task StartWithAdminInterfaceAndAccessWithBrowser()
        {
            WireMockServer.StartWithAdminInterface(9090);
            this.OpenUrl("http://localhost:9090/__admin/mappings");
            await this.WaitingForResponse();
        }

        [Fact]
        public async Task StartWithAdminInterfaceAndReadStaticMappingsAndAccessWithInnerApi()
        {
            // The collection argument 'urls' must contain at least one element.
            var server = WireMockServer.StartWithAdminInterfaceAndReadStaticMappings("http://localhost");
            var api = RestClient.For<IWireMockAdminApi>(server.Urls[0]);
            var settings = await api.GetSettingsAsync();
            var mappings = await api.GetMappingsAsync();
            Assert.NotEmpty(mappings);
        }
        
        [Fact]
        public async Task StartWithAdminInterfaceAndReadStaticMappingsAndAccessWithBrowser()
        {
            // The collection argument 'urls' must contain at least one element.
            WireMockServer.StartWithAdminInterfaceAndReadStaticMappings("http://localhost:9090");
            // please access http://localhost:9090/__admin/mappings
            this.OpenUrl("http://localhost:9090/__admin/mappings");
            await this.WaitingForResponse();
        }
    }
}