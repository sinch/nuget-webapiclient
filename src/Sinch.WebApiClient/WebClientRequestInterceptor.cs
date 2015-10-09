/* Copyright © 2015 Sinch AB

 This software may be modified and distributed under the terms
 of the MIT license.  See the LICENSE file for details
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Newtonsoft.Json;

namespace Sinch.WebApiClient
{
    class WebClientRequestInterceptor<TInterface> : IInterceptor
    {
        private readonly HttpMessageHandler _httpMessageHandler;
        private readonly IActionFilter[] _filters;
        private readonly Uri _baseUri;
        private static readonly MethodInfo ExecuteGenericTaskMethodInfo = typeof(WebClientRequestInterceptor<TInterface>).GetMethod("ExecuteGenericTask", BindingFlags.Instance | BindingFlags.NonPublic);

        public WebClientRequestInterceptor(string baseUri, HttpMessageHandler httpMessageHandler, IActionFilter[] filters)
        {
            _httpMessageHandler = httpMessageHandler;
            _filters = filters;

            _baseUri = new Uri(baseUri);
        }

        public void Intercept(IInvocation invocation)
        {
            var type = invocation.Method.ReturnType;
            if (type == typeof(Task))
            {
                invocation.ReturnValue = ExecuteTask(invocation);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var genericMethod = ExecuteGenericTaskMethodInfo.MakeGenericMethod(type.GetGenericArguments()[0]);
                invocation.ReturnValue = genericMethod.Invoke(this, new object[] { invocation });
            }
            else
                throw new WebApiClientException($"ReturnType: {type} not supported. Must be Task or Task<>.");
        }

        private async Task ExecuteTask(IInvocation invocation)
        {
            var httpRequestMessage = BuildHttpRequestMessage(invocation);

            foreach (var inerceptor in _filters)
                await inerceptor.OnActionExecuting(httpRequestMessage).ConfigureAwait(false);

            using (var client = CreateHttpClient())
            {
                var response = await client.SendAsync(httpRequestMessage).ConfigureAwait(false);

                foreach (var inerceptor in _filters)
                    await inerceptor.OnActionExecuted(response).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NoContent ||
                    response.StatusCode == HttpStatusCode.OK)
                    return;

                throw new WebApiClientException();
            }
        }

        internal async Task<T> ExecuteGenericTask<T>(IInvocation invocation)
        {
            var httpRequestMessage = BuildHttpRequestMessage(invocation);

            foreach (var inerceptor in _filters)
                await inerceptor.OnActionExecuting(httpRequestMessage).ConfigureAwait(false);

            using (var client = CreateHttpClient())
            {
                var response = await client.SendAsync(httpRequestMessage);

                foreach (var inerceptor in _filters)
                    await inerceptor.OnActionExecuted(response).ConfigureAwait(false);

                var value = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.OK)
                    return JsonConvert.DeserializeObject<T>(value);
                
                if (response.StatusCode == HttpStatusCode.NoContent)
                    return default(T);
                
                throw new WebApiClientException();
            }
        }

        private HttpClient CreateHttpClient()
        {
            return _httpMessageHandler != null
                ? new HttpClient(_httpMessageHandler)
                : new HttpClient();
        }

        private HttpRequestMessage BuildHttpRequestMessage(IInvocation invocation)
        {
            var uriParameters = GetUriParameters(invocation.Method, invocation.Arguments);
            var body = GetBodyParameter(invocation.Method, invocation.Arguments);

            var httpAttribute = invocation.Method.GetCustomAttribute<HttpAttribute>();

            var builder = new UriBuilder(_baseUri);
            builder.Path += invocation.Method.DeclaringType.GetCustomAttribute<RouteAttribute>()?.Value;

            var template = new UriTemplate(httpAttribute.Route);
            var uri = template.BindByName(builder.Uri, uriParameters);

            var httpRequestMessage = new HttpRequestMessage(httpAttribute.Method, uri);

            if (body != null)
                httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            return httpRequestMessage;
        }

        private static IDictionary<string, string> GetUriParameters(MethodBase methodBase, params object[] arguments)
        {
            var result = new Dictionary<string, string>();
            var parameters = methodBase.GetParameters();

            for (var i = 0; i < arguments.Length; ++i)
            {
                if (parameters[i].IsDefined(typeof(ToBodyAttribute)))
                    continue;

                if (IsUriParameterType(parameters[i].ParameterType))
                {
                    result.Add(parameters[i].Name, string.Format(CultureInfo.InvariantCulture, "{0}", arguments[i]));
                    continue;
                }

                if (!parameters[i].IsDefined(typeof(ToUriAttribute)))
                    continue;

                if (arguments[i] == null)
                    throw new ArgumentNullException(parameters[i].Name);

                foreach (var property in arguments[i].GetType().GetProperties())
                {
                    result.Add(property.Name,
                        string.Format(CultureInfo.InvariantCulture, "{0}", property.GetValue(arguments[i])));
                }
            }

            return result;
        }

        private static object GetBodyParameter(MethodInfo methodBase, IList<object> arguments)
        {
            var parameters = methodBase.GetParameters();

            for (var i = 0; i < arguments.Count; ++i)
            {
                if (parameters[i].IsDefined(typeof(ToBodyAttribute)))
                    return arguments[i];

                if (IsUriParameterType(parameters[i].ParameterType))
                    continue;

                if (!parameters[i].IsDefined(typeof(ToUriAttribute)))
                    return arguments[i];
            }

            return null;
        }

        static bool IsUriParameterType(Type parameterType)
        {
            return parameterType == typeof (string) ||
                   parameterType == typeof (int) ||
                   parameterType == typeof (int?) ||
                   parameterType == typeof (byte) ||
                   parameterType == typeof (byte?) ||
                   parameterType == typeof (char) ||
                   parameterType == typeof (char?) ||
                   parameterType == typeof (short) ||
                   parameterType == typeof (short?) ||
                   parameterType == typeof (ushort) ||
                   parameterType == typeof (ushort?) ||
                   parameterType == typeof (uint) ||
                   parameterType == typeof (uint?) ||
                   parameterType == typeof (long) ||
                   parameterType == typeof (long?) ||
                   parameterType == typeof (ulong) ||
                   parameterType == typeof (ulong?) ||
                   parameterType == typeof (decimal) ||
                   parameterType == typeof (decimal?) ||
                   parameterType == typeof (float) ||
                   parameterType == typeof (float?) ||
                   parameterType == typeof (double) ||
                   parameterType == typeof (double?) ||
                   parameterType == typeof (Guid) ||
                   parameterType == typeof (Guid?);
        }
    }
}