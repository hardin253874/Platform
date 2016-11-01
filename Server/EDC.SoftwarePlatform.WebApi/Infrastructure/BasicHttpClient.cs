// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
    /// <summary>
    ///     Class BasicHttpClient.
    /// </summary>
    /// <seealso cref="EDC.SoftwarePlatform.WebApi.Infrastructure.IHttpClient" />
    /// <seealso cref="System.IDisposable" />
    public class BasicHttpClient : IHttpClient, IDisposable
    {
        /// <summary>
        ///     The _HTTP client
        /// </summary>
        private HttpClient _httpClient;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BasicHttpClient" /> class.
        /// </summary>
        public BasicHttpClient()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        ///     Posts the message asynchronously.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="content">The content.</param>
        /// <returns>Task&lt;HttpResponseMessage&gt;.</returns>
        public Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content)
        {
            return _httpClient.PostAsync(requestUri, content);
        }

        #region IDisposable

        private bool _disposed; // To detect redundant calls

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_httpClient != null)
                    {
                        _httpClient.Dispose();
                        _httpClient = null;
                    }
                }

                _disposed = true;
            }
        }


        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}