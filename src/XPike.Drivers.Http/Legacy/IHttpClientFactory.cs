// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if LEGACY
namespace System.Net.Http
{
    /// <summary>
    /// Interface IHttpClientFactory
    /// In netstandard2.0 and net461 targets, this interface inherts System.Net.Http.IHttpClientFactory.
    /// It is provided in this library as a compatability layer for net452
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Creates and configures an <see cref="T:System.Net.Http.HttpClient" /> instance using the configuration that corresponds
        /// to the logical name specified by <paramref name="name" />.
        /// </summary>
        /// <param name="name">The logical name of the client to create.</param>
        /// <returns>A new <see cref="T:System.Net.Http.HttpClient" /> instance.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <remarks><para>
        /// Each call to <see cref="M:System.Net.Http.IHttpClientFactory.CreateClient(System.String)" /> is guaranteed to return a new <see cref="T:System.Net.Http.HttpClient" />
        /// instance. Callers may cache the returned <see cref="T:System.Net.Http.HttpClient" /> instance indefinitely or surround
        /// its use in a <langword>using</langword> block to dispose it when desired.
        /// </para>
        /// <para>
        /// The default <see cref="T:System.Net.Http.IHttpClientFactory" /> implementation may cache the underlying
        /// <see cref="T:System.Net.Http.HttpMessageHandler" /> instances to improve performance.
        /// </para>
        /// <para>
        /// Callers are also free to mutate the returned <see cref="T:System.Net.Http.HttpClient" /> instance's public properties
        /// as desired.
        /// </para></remarks>
        HttpClient CreateClient(string name);
    }
}
#endif
