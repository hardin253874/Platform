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
    public class SftpFileFetcher : IRemoteFileFetcher
    {

        /// <summary>
        /// Fetch a file via FTP, returning a token to the temporary file in the file repository
        /// </summary>
        /// <param name="url">The file path, must start with either either ftps:// or sftp:// </param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>A token to the file repository</returns>
        public string GetToTemporaryFile(string url, string username, string password)
        {
            var uri = new Uri(url);

            string fileToken = null;

            ConnectAndRun(uri.Host, username, password, (sftp) =>
            {
                using (var ms = new MemoryStream())
                {
                    var filePath = uri.AbsolutePath;
                    sftp.DownloadFile(filePath, ms);
                    fileToken = FileRepositoryHelper.AddTemporaryFile(ms);
                }

            });

            return fileToken;
        }

        public void PutFromTemporaryFile(string fileHash, string url, string username, string password)
        {
            var uri = new Uri(url);

            ConnectAndRun(uri.Host, username, password, (sftp) =>
            {
                var filePath = uri.AbsolutePath;

                using (var ms = FileRepositoryHelper.GetTemporaryFileDataStream(fileHash))
                {
                    sftp.UploadFile(ms, filePath);
                }
            });
        }


        void ConnectAndRun(string host, string username, string password, Action<SftpClient> act)
        {
            try
            {
                using (SftpClient sftp = new SftpClient(host, username, password))
                {
                    sftp.Connect();

                    act(sftp);

                    sftp.Disconnect();
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                throw new ConnectionException(ex.Message, ex);
            }
            catch (SshException ex)
            {
                throw new ConnectionException(ex.Message, ex);
            }
        }
     

    }
}
