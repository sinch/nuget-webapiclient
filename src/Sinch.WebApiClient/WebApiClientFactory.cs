﻿/* Copyright © 2015 Sinch AB

 This software may be modified and distributed under the terms
 of the MIT license.  See the LICENSE file for details
*/
using System.Net.Http;
using Castle.DynamicProxy;

namespace Sinch.WebApiClient
{
    public class WebApiClientFactory
    {
        private static readonly ProxyGenerator Generator = new ProxyGenerator();
        private readonly IHttpClient _httpClient;

        public WebApiClientFactory(HttpMessageHandler httpMessageHandler = null)
        {
            #if (NET40 || NET451 || NET452 || NET46 || NET461)
                        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;//SecurityProtocolType.Tls1.2;
            #endif
            _httpClient = new HttpClientAdapter(
                httpMessageHandler != null
                    ? new HttpClient(httpMessageHandler)
                    : new HttpClient()
            );
            
        }

        public T CreateClient<T>(string baseUri, params IActionFilter[] filters) where T : class
        {
            return Generator.CreateInterfaceProxyWithoutTarget<T>(new WebClientRequestInterceptor<T>(baseUri, _httpClient, filters));
        }
    }
}