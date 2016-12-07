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
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.IO.RemoteFileFetcher
{
    /// <summary>
    /// Fetches files via FTP
    /// </summary>
    public class FtpFileFetcher : IRemoteFileFetcher
    {
        IRemoteFileFetcher _sftpFetcher { get; }
        IRemoteFileFetcher _ftpsFetcher { get; }


        public FtpFileFetcher(bool bypassSslCheck, IRemoteFileFetcher sftpFileFetcher, IRemoteFileFetcher ftpsFileFetcher)
        {
            if (bypassSslCheck)
            {
                _sftpFetcher = new SslBypassFileFetcher(sftpFileFetcher);
                _ftpsFetcher = new SslBypassFileFetcher(ftpsFileFetcher);
            }
            else
            {
                _sftpFetcher = sftpFileFetcher;
                _ftpsFetcher = ftpsFileFetcher;
            }
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
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException(nameof(url));

            if (string.IsNullOrEmpty(username))
                throw new ArgumentException(nameof(username));

            return GetFetchForUrl(url).GetToTemporaryFile(url, username, password);
        }

        /// <summary>
        /// Put a temporary file into a Url
        /// </summary>
        /// <param name="fileHash"></param>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void PutFromTemporaryFile(string fileHash, string url, string username, string password)
        {
            if (!Factory.FeatureSwitch.Get("ftpExport"))
                throw new NotImplementedException("This feature has not been turned on.");

            GetFetchForUrl(url).PutFromTemporaryFile(fileHash, url, username, password);
        }

        IRemoteFileFetcher GetFetchForUrl(string url)
        {
            var uri = new Uri(url);

            switch (uri.Scheme.ToLower())
            {
                case "ftps": return _ftpsFetcher;
                case "sftp": return _sftpFetcher;
                default: throw new ConnectionException("Only FTPS or SFTP connections are supported", null);
            }
        }
    }
}
