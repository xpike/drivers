using Microsoft.Extensions.Http;
using System;

namespace XPike.Drivers.Http.ClientFactory
{
    public class NullHttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return next;
        }
    }
}
