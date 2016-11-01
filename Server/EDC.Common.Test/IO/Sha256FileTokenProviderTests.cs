// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using EDC.IO;
using NUnit.Framework;

namespace EDC.Test.IO
{
    [TestFixture]
    public class Sha256FileTokenProviderTests
    {
        [Test]
        public void ComputeTokenInvalidStreamTest()
        {
            var provider = new Sha256FileTokenProvider();

            Assert.That(() => provider.ComputeToken(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("stream"));
        }


        [Test]
        public void ComputeTokenIsRandomTest()
        {
            var tokens = new HashSet<string>();

            var provider = new Sha256FileTokenProvider();

            byte[] data = {1, 2, 3, 4, 5, 6};
            using (var ms = new MemoryStream(data))
            {
                tokens.Add(provider.ComputeToken(ms));
            }

            using (var ms = new MemoryStream(data))
            {
                tokens.Add(provider.ComputeToken(ms));
            }

            Assert.AreEqual(1, tokens.Count, "The number of tokens is invalid");

            byte[] data2 = {1, 2, 3, 4, 5, 6, 7, 8, 9};
            using (var ms = new MemoryStream(data2))
            {
                tokens.Add(provider.ComputeToken(ms));
            }

            Assert.AreEqual(2, tokens.Count, "The number of tokens is invalid");
        }
    }
}