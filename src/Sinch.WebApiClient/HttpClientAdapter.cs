using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Sinch.WebApiClient
{
    internal class HttpClientAdapter : IHttpClient
    {
        private readonly HttpClient _httpClient;

        public HttpClientAdapter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) => _httpClient.SendAsync(request);
    }
}