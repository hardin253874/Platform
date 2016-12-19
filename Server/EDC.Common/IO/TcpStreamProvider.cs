// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace EDC.IO
{
    /// <summary>
    ///     Respresents a tcp stream provider.
    /// </summary>
    public class TcpStreamProvider : IStreamProvider
    {
        /// <summary>
        ///     The host name
        /// </summary>
        private readonly string _hostName;


        /// <summary>
        ///     The is secure
        /// </summary>
        private readonly bool _isSecure;


        /// <summary>
        ///     The no delay
        /// </summary>
        private readonly bool _noDelay;


        /// <summary>
        ///     The port
        /// </summary>
        private readonly int _port;


        /// <summary>
        ///     The disposed flag.
        /// </summary>
        private bool _disposed;


        /// <summary>
        ///     The initialized flag.
        /// </summary>
        private bool _isInitialized;


        /// <summary>
        ///     The tcp client.
        /// </summary>
        private TcpClient _tcpClient;


        /// <summary>
        ///     The tcp stream.
        /// </summary>
        private Stream _tcpStream;


        /// <summary>
        /// The ignore SSL errors flag.
        /// </summary>
        private readonly bool _ignoreSslErrors;


        /// <summary>
        /// Initializes a new instance of the <see cref="TcpStreamProvider" /> class.
        /// </summary>
        /// <param name="hostName">Name of the host.</param>
        /// <param name="port">The port.</param>
        /// <param name="noDelay">if set to <c>true</c> [no delay].</param>
        /// <param name="isSecure">if set to <c>true</c> [is secure].</param>
        /// <param name="ignoreSslErrors">if set to <c>true</c> [ignore SSL errors].</param>
        /// <exception cref="System.ArgumentNullException">hostName</exception>
        /// <exception cref="System.ArgumentException">port</exception>
        public TcpStreamProvider(string hostName, int port, bool noDelay, bool isSecure, bool ignoreSslErrors = false)
        {
            if (string.IsNullOrWhiteSpace(hostName))
            {
                throw new ArgumentNullException( nameof( hostName ) );
            }

            if (port <= 0)
            {
                throw new ArgumentException("port");
            }

            _hostName = hostName;
            _port = port;
            _noDelay = noDelay;
            _isSecure = isSecure;
            _ignoreSslErrors = ignoreSslErrors;
        }


        #region IStreamProvider Members


        /// <summary>
        ///     Gets the stream.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            if (_isInitialized)
            {
                return _tcpStream;
            }

            _tcpClient = new TcpClient(_hostName, _port) {NoDelay = _noDelay};

            if (_isSecure)
            {
                var sslStream = new SslStream(_tcpClient.GetStream(), false, UserCertificateValidationCallback);
                _tcpStream = sslStream;
                sslStream.AuthenticateAsClient(_hostName);

                if (!sslStream.IsEncrypted)
                {
                    throw new SecurityException(string.Format("The tcp stream provider could not establish a secure connection to {0}.", _hostName));
                }
            }
            else
            {
                _tcpStream = _tcpClient.GetStream();
            }

            _isInitialized = true;

            return _tcpStream;
        }


        /// <summary>
        ///     Closes the stream.
        /// </summary>
        public void CloseStream()
        {
            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient = null;
            }

            if (_tcpStream != null)
            {
                _tcpStream.Close();
                _tcpStream = null;
            }

            _isInitialized = false;
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


        /// <summary>
        ///     Users the certificate validation callback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="chain">The chain.</param>
        /// <param name="sslPolicyErrors">The SSL policy errors.</param>
        /// <returns></returns>
        private bool UserCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return _ignoreSslErrors || sslPolicyErrors == SslPolicyErrors.None;
        }


        /// <summary>
        ///     Disposes of any resources.
        /// </summary>
        /// <param name="disposing">if set to <c>true</c> [disposing].</param>
        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (!disposing) return;
            CloseStream();
        }
    }
}