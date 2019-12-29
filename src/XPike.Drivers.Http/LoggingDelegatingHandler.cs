using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XPike.Drivers.Http.ClientFactory;
using XPike.Logging;

namespace XPike.Drivers.Http
{
    /// <summary>
    /// Adds configurable opinionated Logging to the HttpClient pipeline.
    /// Implements the <see cref="System.Net.Http.DelegatingHandler" />
    /// </summary>
    /// <seealso cref="System.Net.Http.DelegatingHandler" />
    /// <remarks>
    /// When log level `Trace` or higher is enabled, logs are written at the start and end of the request. Any exceptions
    /// are logged when log level is 'Error` or higher. If you enable `TreatNonSuccessAsErrorsWhenLogging`, responses 
    /// with status codes >=400 will also be logged as long as the error level is `Error` or higher by default. 
    /// You can have only 500 and above be considered errors by setting `Treat4xxAsErrorsWhenLogging` to `false`.
    /// 
    /// If you don't want error responses or exceptions being written as errors, you can record them as `Warning` by enabling
    /// `TreatErrorsAsWarningsWhenLogging`.
    /// 
    /// Detailed tracing can be enabled to include the query string and request/response bodies and headers.
    /// 
    /// You can disable the logging completely, allowing you to use your own, by settinging `EnableLogging` to `false`.
    /// 
    /// > [!IMPORTANT]
    /// > No attempt is made to scrub any sensitive data other than to no include the `Authorization` header in logs.
    /// > Enabling `EnableDetailedRequestTracing` or `EnableDetailedResponseTracing` could cause sensitive data to be logged.
    /// 
    /// ### Metadata
    /// 
    /// #### Always included
    /// 
    /// - commandGroup
    /// - commandName
    /// - httpVerb
    /// - requestUri (without query string)
    /// - host
    /// - statusCode
    /// - Exception details
    /// 
    /// #### Detailed tracing
    /// 
    /// - Request headers
    /// - Response headers
    /// - Query String
    /// 
    /// </remarks>
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        readonly ILogService logger;
        readonly XpikeHttpClientFactoryOptions factoryOptions;
        readonly string category;
        readonly HttpStatusCode lowestError;

        public LoggingDelegatingHandler(ILogService logger, XpikeHttpClientFactoryOptions factoryOptions, string category = LogServiceDefaults.DEFAULT_CATEGORY)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(category))
                this.category = LogServiceDefaults.DEFAULT_CATEGORY;
            else
                this.category = category;
            
            this.factoryOptions = factoryOptions ?? throw new ArgumentNullException(nameof(factoryOptions));

            if (factoryOptions.Treat4xxAsErrorsWhenLogging)
                this.lowestError = HttpStatusCode.BadRequest;
            else
                this.lowestError = HttpStatusCode.InternalServerError;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!factoryOptions.EnableLogging)
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false); 

            var metadata = new Dictionary<string, string> {
                    { "commandGroup", factoryOptions.CommandGroup },
                    { "commandName", factoryOptions.CommandName }
                };

            await EnrichMetadata(metadata, request);

            string messagePrefix = $"Http {request.Method} Request to {metadata["requestUri"]}";

            logger.Trace($"[BEGIN] {messagePrefix}", metadata, category);

            try
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                await EnrichMetadata(metadata, response);

                string message = $"[RESPONSE] {messagePrefix} responded with HTTP status code {(int)response.StatusCode}";
                
                if (factoryOptions.TreatNonSuccessAsErrorsWhenLogging && response.StatusCode >= lowestError)
                {
                    if (factoryOptions.TreatErrorsAsWarningsWhenLogging)
                        logger.Warn(message, null, metadata, category);
                    else
                        logger.Error(message, null, metadata, category);
                }
                else
                {
                    logger.Trace(message, metadata, category);
                }

                return response;
            }
            catch(Exception ex)
            {
                EnrichMetadata(metadata, ex);

                string message = $"[ERROR] {messagePrefix} failed with an exception: '{ex.Message}'. See the exception property for details";

                if (factoryOptions.TreatErrorsAsWarningsWhenLogging)
                    logger.Warn(message, ex, metadata, category);
                else
                    logger.Error(message, ex, metadata, category);
                
                throw;
            }
        }
        
        private async Task EnrichMetadata(Dictionary<string, string> metadata, HttpRequestMessage request)
        {
            string requestUri;

            try
            {
                // scheme://host:port/path
                requestUri = request.RequestUri.GetLeftPart(UriPartial.Path); //<==no query string
            }
            catch
            {
                requestUri = "Unable-to-parse";
            }

            try
            {
                metadata["httpVerb"] = request.Method.ToString();
                metadata["requestUri"] = requestUri;
                metadata["host"] = request.RequestUri.Host;

                if (factoryOptions.EnableDetailedRequestTracing)
                {
                    metadata.Add("queryString", request.RequestUri.Query);
                    metadata.Add("requestBody", await GetBody(request));

                    // Request headers
                    foreach (var header in request.Headers)
                    {
                        // skip the Authorization header for security reasons
                        if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                            continue;

                        metadata[$"requestHeaders.{header.Key}"] = header.Value.ToString();
                    }

                    // Content headers
                    foreach (var header in request?.Content?.Headers)
                    {
                        // skip the Authorization header for security reasons
                        if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                            continue;

                        metadata[$"requestHeaders.{header.Key}"] = header.Value.ToString();
                    }
                }
            }
            catch 
            {
                // Don't blowup logging. If we can't get some of the metadata, that's ok, log what we can.
            }
        }

        private async Task EnrichMetadata(Dictionary<string, string> metadata, HttpResponseMessage response)
        {
            try
            {
                metadata["statusCode"] = ((int)response.StatusCode).ToString();

                if (!response.IsSuccessStatusCode || factoryOptions.EnableDetailedResponseTracing)
                    metadata["responseBody"] = await GetBody(response);

                if (factoryOptions.EnableDetailedResponseTracing)
                {
                    foreach (var header in response.Headers)
                        metadata[$"responseHeaders.{header.Key}"] = header.Value.ToString();
                }
            }
            catch
            {
                // Don't blowup logging. If we can't get some of the metadata, that's ok, log what we can.
            }
        }

        private void EnrichMetadata(Dictionary<string, string> metadata, Exception ex)
        {
            try
            {
                if (ex is WebException webEx)
                {
                    metadata["webExceptionStatus"] = webEx.Status.ToString();
                    foreach (var header in webEx?.Response?.Headers.AllKeys)
                        metadata[$"responseHeaders.{header}"] = webEx.Response.Headers[header];
                }
            }
            catch
            {
                // Don't blowup logging. If we can't get some of the metadata, that's ok, log what we can.
            }
        }

        private Task<string> GetBody(HttpRequestMessage request)
        {
            try
            {
                if (request.Content != null && (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put))
                    return request.Content.ReadAsStringAsync();

                return Task.FromResult("No-Content");
            }
            catch (Exception ex)
            {
                // Don't blowup logging. If we can't get some of the metadata, that's ok, log what we can.
                return Task.FromResult($"Error deserializing content for logging: {ex}");
            }
        }

        private Task<string> GetBody(HttpResponseMessage response)
        {
            try
            {
                if (response.Content == null)
                    return Task.FromResult("No-Content");

                return response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                // Don't blowup logging. If we can't get some of the metadata, that's ok, log what we can.
                return Task.FromResult($"Error deserializing content for logging: {ex}");
            }
        }
    }
}
