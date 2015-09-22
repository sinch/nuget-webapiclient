/* Copyright © 2015 Sinch AB

 This software may be modified and distributed under the terms
 of the MIT license.  See the LICENSE file for details
*/
using System;

namespace Sinch.WebApiClient
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ToUriAttribute : Attribute { }
}