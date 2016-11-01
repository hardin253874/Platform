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
    /// Fetches files via FTP
    /// </summary>
    public class FtpFileFetcher : IRemoteFileFetcher
    {
        public bool BypassSslCheck { get; }

        public FtpFileFetcher(bool bypassSslCheck)
        {
            BypassSslCheck = bypassSslCheck;
        }


        /// <summary>
        /// Fetch a file via FTP, returning a token to the temporary file in the file repository
        /// </summary>
        /// <param name="url">The file path, must start with either either ftps:// or sftp:// </param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>A token to the file repository</returns>
        public string FetchToTemporaryFile(string url, string username, string password)
        {
            Action undoBypass = null;

            try
            {
                undoBypass = SslCertificateBypass();


                var uri = new Uri(url);

                switch (uri.Scheme.ToLower())
                {
                    case "ftps": return FtpsFetchToTemporaryFile(uri, username, password);
                    case "sftp": return SftpFetchToTemporaryFile(uri, username, password);
                    default: throw new ConnectionException("Only FTPS or SFTP connections are supported", null);
                }
            }
            catch (AuthenticationException ex)
            {
                throw new ConnectionException(ex.Message, ex);
            }
            finally
            {
                undoBypass();
            }


        }

        private string FtpsFetchToTemporaryFile(Uri uri, string username, string password)
        {

            var transformedUri = new Uri(uri.ToString().Replace("ftps://", "ftp://"));    // Ugly hack - the WebRequest does not handle ftps protocol in the URL 
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(transformedUri);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.EnableSsl = true;                                   // We NEVER want to set this as false. The password will be transmitted plain text.

            if (!String.IsNullOrEmpty(username))
                request.Credentials = new NetworkCredential(username, password ?? string.Empty);

            try
            {
                var response = (FtpWebResponse)request.GetResponse();

                using (var responseStream = response.GetResponseStream())
                {
                    return FileRepositoryHelper.AddTemporaryFile(responseStream);
                }

            }
            catch (WebException ex)
            {
                var errorResponse = ex.Response as FtpWebResponse;

                // This status code is returned is the AUTH command is unavailable so when we see it it indicates the we can't flip into encrypted mode
                string message = (errorResponse.StatusCode == FtpStatusCode.CommandSyntaxError ? "Unable to connect to server securely: " : "") + ex.Message;

                throw new ConnectionException(message, ex);
            }
        }


        private string SftpFetchToTemporaryFile(Uri url, string username, string password)
        {
            string fileToken = null;

            try
            {
                using (SftpClient sftp = new SftpClient(url.Host, username, password))
                {
                    sftp.Connect();

                    using (var ms = new MemoryStream())  
                    {
                        var filePath = url.AbsolutePath;
                        sftp.DownloadFile(filePath, ms);
                        fileToken = FileRepositoryHelper.AddTemporaryFile(ms);
                    }

                    sftp.Disconnect();
                }

                return fileToken;
            }
            catch(System.Net.Sockets.SocketException ex)
            {
                throw new ConnectionException(ex.Message, ex);
            }
            catch (SshException ex)
            {
                throw new ConnectionException(ex.Message, ex);
            }
        }


        /// <summary>
        /// Bypass the certificate
        /// </summary>
        /// <returns>An action that will undo the bypass.</returns>
        private Action SslCertificateBypass()
        {
            if (BypassSslCheck)
            {
                var original = ServicePointManager.ServerCertificateValidationCallback;

                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                return () => ServicePointManager.ServerCertificateValidationCallback = original;
            }
            else
            {
                return () => { };
            }
            
        }
    }
}
