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
    public class FtpsFileFetcher : IRemoteFileFetcher
    {
        
        /// <summary>
        /// Fetch a file via FTPS, returning a token to the temporary file in the file repository
        /// </summary>
        /// <param name="url">The file path,
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>A token to the file repository</returns>
        public string GetToTemporaryFile(string uri, string username, string password)
        {
            string fileHash = null;
            ConnectAndRun(uri, username, password, 
                
                // setup
                request =>
                {
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                },

                // handle response
                responseStream =>
                {
                    fileHash = FileRepositoryHelper.AddTemporaryFile(responseStream);
                }
            );

            return fileHash;
        }



        /// <summary>
        /// Send a file to a Ftps address
        /// </summary>
        /// <param name="fileHash"></param>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void PutFromTemporaryFile(string fileHash, string uri, string username, string password)
        {
            ConnectAndRun(uri, username, password,

                // setup
                request =>
                {
                    request.Method = WebRequestMethods.Ftp.UploadFile;

                    using (var fileStream = FileRepositoryHelper.GetTemporaryFileDataStream(fileHash))
                    {
                        using (var requestStream = request.GetRequestStream())
                        {
                            fileStream.CopyTo(requestStream);
                        }
                    }
                },

                // handle response
                responseStream =>
                {
                    // do nothing
                }
            );
        }


        void ConnectAndRun(string uri, string username, string password, Action<FtpWebRequest> setup, Action<Stream> handleResponse )
        {

            var transformedUri = new Uri(uri.Replace("ftps://", "ftp://"));    // Ugly hack - the WebRequest does not handle ftps protocol in the URL 
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(transformedUri);
            request.UseBinary = true;

            request.EnableSsl = true;   // We NEVER want to set this as false. The password will be transmitted plain text.

            if (!String.IsNullOrEmpty(username))
                request.Credentials = new NetworkCredential(username, password ?? string.Empty);

            try
            {
                setup(request);

                var response = (FtpWebResponse)request.GetResponse();

                using (var responseStream = response.GetResponseStream())
                {
                    handleResponse(responseStream);
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
    }
}
