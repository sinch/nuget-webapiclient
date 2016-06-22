/* Copyright © 2015 Sinch AB

 This software may be modified and distributed under the terms
 of the MIT license.  See the LICENSE file for details
*/
using System;

namespace Sinch.WebApiClient
{
    public class WebApiClientException : Exception
    {
        public WebApiClientException() { }
        public WebApiClientException(string message) : base(message) { }
        public WebApiClientException(string message, Exception innerException) : base(message, innerException) { }
    }
}