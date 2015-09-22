/* Copyright © 2015 Sinch AB

 This software may be modified and distributed under the terms
 of the MIT license.  See the LICENSE file for details
*/
using System.Net.Http;
using System.Threading.Tasks;

namespace Sinch.WebApiClient
{
    public interface IActionFilter
    {
        Task OnActionExecuting(HttpRequestMessage requestMessage);
        Task OnActionExecuted(HttpResponseMessage responseMessage);
    }
}