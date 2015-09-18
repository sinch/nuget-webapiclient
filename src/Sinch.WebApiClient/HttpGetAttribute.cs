using System;
using System.Net.Http;

namespace Sinch.WebApiClient
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpGetAttribute : HttpAttribute
    {
        public HttpGetAttribute(string route)
        {
            Route = route;
        }

        public override HttpMethod Method
        {
            get { return HttpMethod.Get; }
        }
    }
}