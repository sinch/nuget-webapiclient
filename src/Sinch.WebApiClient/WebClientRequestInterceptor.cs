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
                await inerceptor.OnActionExecuting(httpRequestMessage);

            using (var client = CreateHttpClient())
            {
                var response = await client.SendAsync(httpRequestMessage);

                foreach (var inerceptor in _filters)
                    await inerceptor.OnActionExecuted(response);

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
                await inerceptor.OnActionExecuting(httpRequestMessage);

            using (var client = CreateHttpClient())
            {
                var response = await client.SendAsync(httpRequestMessage);

                foreach (var inerceptor in _filters)
                    await inerceptor.OnActionExecuted(response);

                var value = await response.Content.ReadAsStringAsync();

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

            var template = new UriTemplate(httpAttribute.Route);
            var uri = template.BindByName(_baseUri, uriParameters);
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
                if (parameters[i].GetCustomAttribute<ToBodyAttribute>() != null)
                    continue;

                if (IsSimpleType(arguments[i]))
                {
                    result.Add(parameters[i].Name, string.Format(CultureInfo.InvariantCulture, "{0}", arguments[i]));
                    continue;
                }

                if (parameters[i].GetCustomAttribute<ToUriAttribute>() == null)
                    continue;

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
                if (parameters[i].GetCustomAttribute<ToBodyAttribute>() != null)
                    return arguments[i];

                if (IsSimpleType(arguments[i]))
                    continue;

                if (parameters[i].GetCustomAttribute<ToUriAttribute>() == null)
                    return arguments[i];
            }

            return null;
        }

        static bool IsSimpleType(object o)
        {
            return o is string ||
                   o is byte ||
                   o is char ||
                   o is short ||
                   o is ushort ||
                   o is int ||
                   o is uint ||
                   o is long ||
                   o is ulong ||
                   o is decimal ||
                   o is float ||
                   o is Guid;
        }
    }
}