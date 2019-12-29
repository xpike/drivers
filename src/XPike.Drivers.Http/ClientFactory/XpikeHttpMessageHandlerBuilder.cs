// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// The code here is largly Microsoft's original implementation. Dependencies have been swapped
// out for xPike equivalents. The Build() method has also been altered to build up the xPike pipeline.

using Microsoft.Extensions.Http;
using MilestoneTG.TransientFaultHandling.Http;
using NHystrix;
using NHystrix.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using XPike.Logging;
using XPike.Metrics;
using XPike.Settings;

namespace XPike.Drivers.Http.ClientFactory
{
    /// <summary>
    /// An xpike HttpMessageHandlerBuilder implementation. This class can be used in either
    /// Full Framework or CoreFx. In Asp.Net Core, you can simply replace the registration
    /// of HttpMessageHandlerBuilder to use this one instead of Microsoft's 
    /// DefaultHttpMessageHandlerBuilder.
    /// Implements the <see cref="Microsoft.Extensions.Http.HttpMessageHandlerBuilder" />
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Http.HttpMessageHandlerBuilder" />
    public class XpikeHttpMessageHandlerBuilder : HttpMessageHandlerBuilder
    {
        private string _name;
        private ILogService _logger;
        ISettingsService _settingsService;
        IMetricsService _metrics;

        public XpikeHttpMessageHandlerBuilder(ISettingsService settingsService, ILogService logger, IMetricsService metrics)
        {
            _settingsService = settingsService;
            _logger = logger;
            _metrics = metrics;
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="T:System.Net.Http.HttpClient" /> being created.
        /// </summary>
        /// <value>The name.</value>
        /// <exception cref="ArgumentNullException">value</exception>
        /// <remarks>The <see cref="P:Microsoft.Extensions.Http.HttpMessageHandlerBuilder.Name" /> is set by the <see cref="T:System.Net.Http.IHttpClientFactory" /> instructure
        /// and is public for unit testing purposes only. Setting the <see cref="P:Microsoft.Extensions.Http.HttpMessageHandlerBuilder.Name" /> outside of
        /// testing scenarios may have unpredictable results.</remarks>
        public override string Name
        {
            get => _name;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _name = value;
            }
        }

        public override HttpMessageHandler PrimaryHandler { get; set; } = new HttpClientHandler();

        public override IList<DelegatingHandler> AdditionalHandlers { get; } = new List<DelegatingHandler>();

        public override HttpMessageHandler Build()
        {
            var options = _settingsService.GetSettings<XpikeHttpClientFactoryOptions>(Name)?.Value;

            var retryHandler = new RetryDelegatingHandler(options.RetryPolicyOptions) { 
                InnerHandler = PrimaryHandler
            };

            var hystrixHandler = new HystrixDelegatingHandler(
                new HystrixCommandKey(
                    options.CommandName, 
                    new HystrixCommandGroup(options.CommandGroup)), 
                options.HystrixCommandProperties, 
                retryHandler);

            var loggingHandler = new LoggingDelegatingHandler(_logger, category: Name, factoryOptions: options)
            {
                InnerHandler = hystrixHandler
            }; ;

            var metricsHandler = new MetricsDelegatingHandler(_metrics, options)
            {
                InnerHandler = loggingHandler
            };

            return CreateHandlerPipeline(metricsHandler, AdditionalHandlers);
        }                                                                      
    }
}
