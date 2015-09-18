using Castle.DynamicProxy;

namespace Sinch.WebApiClient
{
    public class WebApiClientFactory
    {
        private static readonly ProxyGenerator Generator = new ProxyGenerator();

        public T CreateClient<T>(string baseUri, params IActionFilter[] filters) where T : class
        {
            return Generator.CreateInterfaceProxyWithoutTarget<T>(new WebClientRequestInterceptor<T>(baseUri, filters));
        }
    }
}