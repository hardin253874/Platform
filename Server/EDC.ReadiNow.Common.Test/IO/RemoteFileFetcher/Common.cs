// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.IO;
using EDC.ReadiNow.IO.RemoteFileFetcher;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.IO.RemoteFileFetcher
{
    public static class Common
    {
        static Random _rand = new Random();

        public static  void TestPutGet(IRemoteFileFetcher fetcher, string url, string username, string password)
        {
            byte[] buffer;
            string fileHash;
            CreateTempFile(out buffer, out fileHash);

            // Put
            fetcher.PutFromTemporaryFile(fileHash, url, username, password);

            // Get
            var fetchedHash = fetcher.GetToTemporaryFile(url, username, password);

            using (var fetchedMs = FileRepositoryHelper.GetTemporaryFileDataStream(fetchedHash))
            {
                var b = fetchedMs.ReadByte();

                buffer[0].ShouldBeEquivalentTo(b);
            }
            //TODO: Delete the file
        }

        public static void TestPut(IRemoteFileFetcher fetcher, string url, string username, string password)
        {
            byte[] buffer;
            string fileHash;
            CreateTempFile(out buffer, out fileHash);

            fetcher.PutFromTemporaryFile(fileHash, url, username, password);
        }

        public static string TestGet(IRemoteFileFetcher fetcher, string url, string username, string password)
        {
            byte[] buffer;
            string fileHash;
            CreateTempFile(out buffer, out fileHash);

            return fetcher.GetToTemporaryFile(url, username, password);
        }



        private static void CreateTempFile(out byte[] buffer, out string fileHash)
        {
            buffer = new Byte[1];
            _rand.NextBytes(buffer);
            var ms = new MemoryStream(buffer);

            fileHash = FileRepositoryHelper.AddTemporaryFile(ms);
        }

    }
}
