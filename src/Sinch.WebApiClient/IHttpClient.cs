using System.Net.Http;
using System.Threading.Tasks;

namespace Sinch.WebApiClient
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage);
    }
}
