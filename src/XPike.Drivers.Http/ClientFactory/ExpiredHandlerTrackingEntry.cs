// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Unfortunately, because this class in internal, we cannot use the implementation from the
// Microsoft.Extensions.Http package. The original implementation is provided here so it can
// be accessed by our XpikeHttpClientFactory.

using System;
using System.Net.Http;

namespace XPike.Drivers.Http.ClientFactory
{
    // Thread-safety: This class is immutable
    internal class ExpiredHandlerTrackingEntry
    {
        private readonly WeakReference _livenessTracker;

        public ExpiredHandlerTrackingEntry(ActiveHandlerTrackingEntry other)
        {
            Name = other.Name;

            _livenessTracker = new WeakReference(other.Handler);
            InnerHandler = other.Handler.InnerHandler;
        }

        public bool CanDispose => !_livenessTracker.IsAlive;

        public HttpMessageHandler InnerHandler { get; }

        public string Name { get; }
    }
}
