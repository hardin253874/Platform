// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO.RemoteFileFetcher;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.IO.RemoteFileFetcher
{

    public class FtpFileFetcherTest
    {
        [Test]
        public void IsRegistered()
        {
            Assert.That(Factory.RemoteFileFetcher, Is.Not.Null);
            Assert.That(Factory.RemoteFileFetcher is FtpFileFetcher, Is.True);
        }

    }
}
