using System;
using System.Net.Http;

namespace Sinch.WebApiClient
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPatchAttribute : HttpAttribute
    {
        public HttpPatchAttribute(string route)
        {
            Route = route;
        }

        public override HttpMethod Method
        {
            get { return new HttpMethod("PATCH"); }
        }
    }
}