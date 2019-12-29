using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using XPike.Drivers.Http.ClientFactory;
using XPike.IoC;


namespace XPike.Drivers.Http
{
    public class Package : IDependencyPackage
    {
        public void RegisterPackage(IDependencyCollection dependencyCollection)
        {
            if (dependencyCollection == null)
                throw new ArgumentNullException(nameof(dependencyCollection));

            dependencyCollection.RegisterTransient<HttpMessageHandlerBuilder, XpikeHttpMessageHandlerBuilder>();

            dependencyCollection.RegisterSingleton<IHttpClientFactory, XpikeHttpClientFactory>();

            dependencyCollection.AddSingletonToCollection<IHttpMessageHandlerBuilderFilter, NullHttpMessageHandlerBuilderFilter>();

        }
    }
}
