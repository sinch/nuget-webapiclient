using System;

namespace Sinch.WebApiClient
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ToBodyAttribute : Attribute { }
}