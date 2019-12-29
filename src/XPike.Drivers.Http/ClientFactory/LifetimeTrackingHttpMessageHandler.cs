// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Unfortunately, because this class in internal, we cannot use the implementation from the
// Microsoft.Extensions.Http package. The original implementation is provided here so it can
// be accessed by our XpikeHttpClientFactory.

using System.Net.Http;

namespace XPike.Drivers.Http.ClientFactory
{
    // This a marker used to check if the underlying handler should be disposed. HttpClients
    // share a reference to an instance of this class, and when it goes out of scope the inner handler
    // is eligible to be disposed.
    internal class LifetimeTrackingHttpMessageHandler : DelegatingHandler
    {
        public LifetimeTrackingHttpMessageHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }
    }
}
