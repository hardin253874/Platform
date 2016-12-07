// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.IO;
using EDC.ReadiNow.IO.RemoteFileFetcher;
using EDC.ReadiNow.Model;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.IO.RemoteFileFetcher
{
    [TestFixture]
    public class SftpFileFetcherTest
    {
        string UrlRoot = "sftp://RNFTP01.readinow.net/NunitTestFiles";
        string _username = "Simon";
        string _password = "Password1";


        [Test]
        public void PutAndGet()
        {
            var fetcher = new SftpFileFetcher();

            string url = UrlRoot + "/PutAndGet_" + DateTime.UtcNow.Ticks;

            Common.TestPutGet(fetcher, url, _username, _password);
        }
    }
}
