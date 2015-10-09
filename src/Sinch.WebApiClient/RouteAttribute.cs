/* Copyright © 2015 Sinch AB

 This software may be modified and distributed under the terms
 of the MIT license.  See the LICENSE file for details
*/
using System;

namespace Sinch.WebApiClient
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class RouteAttribute : Attribute
    {
        public RouteAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
    }
}
