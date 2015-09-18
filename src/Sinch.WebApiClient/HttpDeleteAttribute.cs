using System;
using System.Net.Http;

namespace Sinch.WebApiClient
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpDeleteAttribute : HttpAttribute
    {
        public HttpDeleteAttribute(string route)
        {
            Route = route;
        }

        public override HttpMethod Method
        {
            get { return HttpMethod.Delete; }
        }
    }
}