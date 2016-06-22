/* Copyright © 2015 Sinch AB

 This software may be modified and distributed under the terms
 of the MIT license.  See the LICENSE file for details
*/

using System.Collections.Concurrent;
using System.Net.Http;
using Castle.DynamicProxy;

namespace Sinch.WebApiClient
{
    public class WebApiClientFactory
    {
        private static readonly ProxyGenerator Generator = new ProxyGenerator();
        private static readonly ConcurrentDictionary<int, HttpClient> HttpClients = new ConcurrentDictionary<int, HttpClient>();

        public T CreateClient<T>(string baseUri, params IActionFilter[] filters) where T : class
        {
            return Generator.CreateInterfaceProxyWithoutTarget<T>(new WebClientRequestInterceptor<T>(baseUri, GetHttpClient(null), filters));
        }

        public T CreateClient<T>(string baseUri, HttpMessageHandler httpMessageHandler, params IActionFilter[] filters) where T : class
        {
            return Generator.CreateInterfaceProxyWithoutTarget<T>(new WebClientRequestInterceptor<T>(baseUri, GetHttpClient(httpMessageHandler), filters));
        }

        private static HttpClient GetHttpClient(HttpMessageHandler httpMessageHandler)
        {
            return HttpClients.GetOrAdd(httpMessageHandler?.GetHashCode() ?? 0, h => httpMessageHandler != null ? new HttpClient(httpMessageHandler) : new HttpClient());
        }
    }
}