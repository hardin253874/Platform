// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Net;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using EDC.ReadiNow.Configuration;
using System.Security.Authentication;

namespace EDC.ReadiNow.IO.RemoteFileFetcher
{
    /// <summary>
    /// Fetches files, optionally bypassing SSL
    /// </summary>
    public class SslBypassFileFetcher : IRemoteFileFetcher
    {
        object _syncRoot = new object();

        IRemoteFileFetcher _fetcher { get; }


        public SslBypassFileFetcher(IRemoteFileFetcher fetcher)
        {
            _fetcher = fetcher;
        }


        /// <summary>
        /// Fetch a file via FTP, returning a token to the temporary file in the file repository
        /// </summary>
        /// <param name="url">The file path, must start with either either ftps:// or sftp:// </param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>A token to the file repository</returns>
        public string GetToTemporaryFile(string url, string username, string password)
        {
            string fileHash = null;

            BypassSsl(() =>
            {
                fileHash = _fetcher.GetToTemporaryFile(url, username, password);
            });

            return fileHash;
        }


        public void PutFromTemporaryFile(string fileHash, string url, string username, string password)
        {
            BypassSsl(() =>
            {
                 _fetcher.PutFromTemporaryFile(fileHash, url, username, password);
            });
        }

        private void BypassSsl(Action act)
        {
            lock (_syncRoot)        // we can't let anyone else fiddle with SSL while we are playing - bad for performance but this will only be on a dev box
            {
                RemoteCertificateValidationCallback original = null;

                try
                {
                    original = ServicePointManager.ServerCertificateValidationCallback;

                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                    act();
                }
                finally
                {
                    ServicePointManager.ServerCertificateValidationCallback = original;
                }
            }
        }
     
    }
}
