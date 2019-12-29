using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XPike.Drivers.Http.ClientFactory;
using XPike.Metrics;

namespace XPike.Drivers.Http
{
    /// <summary>
    /// Adds metrics recording to the Http pipeline.
    /// Implements the <see cref="System.Net.Http.DelegatingHandler" />
    /// </summary>
    /// <seealso cref="System.Net.Http.DelegatingHandler" />
    /// <remarks>
    /// ### Http Error Codes
    /// 
    /// - Http 408 and 504 are recorded as timeouts
    /// - Anything >= 500 (except 504) are recorded as failures
    /// - All others are recorded as success
    /// 
    /// All events are recorded with the http status code and requested uri. If needed, you can still report on 4xx codes,
    /// for example, by using the `statusCode` tag.
    /// </remarks>
    public class MetricsDelegatingHandler : DelegatingHandler
    {
        IMetricsService metricsService;
        XpikeHttpClientFactoryOptions factoryOptions;
        string prefix;

        public MetricsDelegatingHandler(IMetricsService metricsService, XpikeHttpClientFactoryOptions factoryOptions)
        {
            this.metricsService = metricsService;
            this.factoryOptions = factoryOptions;

            prefix = $"{factoryOptions.CommandGroup}.{factoryOptions.CommandName}";
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // If metrics aren't enabled, just forward the request and return the response.
            if (!factoryOptions.EnableMetrics)
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Gather metrics around the http call...
            IList<string> tags = new List<string> { 
                { $"requestUri:{request.RequestUri}" },
                { $"commandGroup:{factoryOptions.CommandGroup}" },
                { $"commandName:{factoryOptions.CommandName}" }
            };

            metricsService.Increment($"{prefix}.request", tags: tags);

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                stopwatch.Stop();

                tags.Add($"statusCode:{response.StatusCode}");
                metricsService.Timer($"{prefix}.timing", stopwatch.ElapsedMilliseconds, tags: tags);

                if (response.StatusCode == HttpStatusCode.RequestTimeout ||
                    response.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    metricsService.Increment($"{prefix}.timeout", tags: tags);
                }
                else if ((int)response.StatusCode >= 500)
                {
                    metricsService.Increment($"{prefix}.failure", tags: tags);
                }
                else
                { 
                    metricsService.Increment($"{prefix}.success", tags: tags);
                }

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                metricsService.Timer($"{prefix}.timing", stopwatch.ElapsedMilliseconds, tags: new[] { $"request:{request.RequestUri}", $"exceptionType: { ex.GetType().FullName }" });
                metricsService.Increment($"{prefix}.failure", tags: new[] { $"request:{request.RequestUri}", $"exceptionType: { ex.GetType().FullName }" });
                throw;
            }
        }
    }
}
