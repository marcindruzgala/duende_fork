using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Duende.Bff.Tests.TestFramework;
using Duende.Bff.Tests.TestHosts;
using Shouldly;
using Xunit;

namespace Duende.Bff.Tests.Headers
{
    public class General : BffIntegrationTestBase
    {
        [Fact]
        public async Task local_endpoint_should_receive_standard_headers()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/local_anon"));
            req.Headers.Add("x-csrf", "1");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.ShouldBeTrue();
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);

            apiResult.RequestHeaders.Count.ShouldBe(2);
            apiResult.RequestHeaders["Host"].Single().ShouldBe("app");
            apiResult.RequestHeaders["x-csrf"].Single().ShouldBe("1");
        }
        
        [Fact]
        public async Task custom_header_should_be_forwarded()
        {
            await BffHost.InitializeAsync();

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
            req.Headers.Add("x-csrf", "1");
            req.Headers.Add("x-custom", "custom");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.ShouldBeTrue();
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);

            apiResult.RequestHeaders["Host"].Single().ShouldBe("api");
            apiResult.RequestHeaders["x-custom"].Single().ShouldBe("custom");
        }
        
        [Fact]
        public async Task custom_header_should_be_forwarded_and_xforwarded_headers_should_be_created()
        {
            await BffHost.InitializeAsync();

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
            req.Headers.Add("x-csrf", "1");
            req.Headers.Add("x-custom", "custom");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.ShouldBeTrue();
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            
            apiResult.RequestHeaders["X-Forwarded-Host"].Single().ShouldBe("app");
            apiResult.RequestHeaders["X-Forwarded-Proto"].Single().ShouldBe("https");
            apiResult.RequestHeaders["Host"].Single().ShouldBe("api");
            apiResult.RequestHeaders["x-custom"].Single().ShouldBe("custom");
        }
    }
}