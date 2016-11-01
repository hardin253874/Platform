// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
    /// <summary>
    ///     Interface IHttpClient
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>
        ///     Posts the message asynchronously.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="content">The content.</param>
        /// <returns>Task&lt;HttpResponseMessage&gt;.</returns>
        Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content);
    }
}