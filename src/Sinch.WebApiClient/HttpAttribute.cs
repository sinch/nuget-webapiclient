/* Copyright © 2015 Sinch AB

 This software may be modified and distributed under the terms
 of the MIT license.  See the LICENSE file for details
*/
using System;
using System.Net.Http;

namespace Sinch.WebApiClient
{
    public abstract class HttpAttribute : Attribute
    {
        public abstract HttpMethod Method { get; }
        public string Route { get; set; }
    }
}