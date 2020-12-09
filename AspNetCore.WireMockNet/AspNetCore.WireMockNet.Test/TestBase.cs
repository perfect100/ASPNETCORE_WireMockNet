using System.Diagnostics;
using System.Threading.Tasks;

namespace AspNetCore.WireMockNet.Test
{
    public class TestBase
    {
        protected void OpenUrl(string url)
        {
            var ps = new ProcessStartInfo(url)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

        protected async Task WaitingForResponse()
        {
            await Task.Delay(4000);
        }
    }
}