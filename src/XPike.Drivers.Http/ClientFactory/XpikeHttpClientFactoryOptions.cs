using Microsoft.Extensions.Http;
using MilestoneTG.TransientFaultHandling.Http;
using NHystrix;

namespace XPike.Drivers.Http.ClientFactory
{
    public class XpikeHttpClientFactoryOptions
    {
        /// <summary>
        /// Gets or sets the command group.
        /// </summary>
        /// <value>The command group.</value>
        public string CommandGroup { get; set; } = "Default";

        /// <summary>
        /// Gets or sets the name of the command.
        /// </summary>
        /// <value>The name of the command.</value>
        public string CommandName { get; set; } = "Default";
        
        public HttpClientFactoryOptions HttpClientFactoryOptions { get; set; } = new HttpClientFactoryOptions();
        
        public HystrixCommandProperties HystrixCommandProperties { get; set; } = new HystrixCommandProperties();
        
        public HttpRetryPolicyOptions RetryPolicyOptions { get; set; } = new HttpRetryPolicyOptions();

        /// <summary>
        /// Gets or sets a value indicating whether to enable metrics. Default: <c>true</c>
        /// </summary>
        /// <value><c>true</c> if [enable metrics]; otherwise, <c>false</c>.</value>
        public bool EnableMetrics { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to enable logging. Default: <c>true</c>
        /// </summary>
        /// <value><c>true</c> if [enable logging]; otherwise, <c>false</c>.</value>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to enable detailed request tracing.
        /// </summary>
        /// <value><c>true</c> if [enable detailed request tracing]; otherwise, <c>false</c>.</value>
        public bool EnableDetailedRequestTracing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable detailed response tracing.
        /// </summary>
        /// <value><c>true</c> if [enable detailed response tracing]; otherwise, <c>false</c>.</value>
        public bool EnableDetailedResponseTracing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to treat errors as warnings when logging.
        /// </summary>
        /// <value><c>true</c> if [treat errors as warnings when logging]; otherwise, <c>false</c>.</value>
        public bool TreatErrorsAsWarningsWhenLogging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to treat non success responses as errors when logging.
        /// </summary>
        /// <value><c>true</c> if [treat non success as errors when logging]; otherwise, <c>false</c>.</value>
        public bool TreatNonSuccessAsErrorsWhenLogging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to treat HTTP 4xx as errors when logging. Default: <c>true</c>
        /// </summary>
        /// <value>When <c>true</c>, treats 4xx as errors when logging; otherwise, only 5xx errors are treated as errors.</value>
        public bool Treat4xxAsErrorsWhenLogging { get; set; } = true;

    }
}
