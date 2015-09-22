/* Copyright © 2015 Sinch AB

 This software may be modified and distributed under the terms
 of the MIT license.  See the LICENSE file for details
*/
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

        public override HttpMethod Method => new HttpMethod("PATCH");
    }
}