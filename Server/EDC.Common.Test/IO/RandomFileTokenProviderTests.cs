// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.IO;
using EDC.IO;
using NUnit.Framework;
using System;

namespace EDC.Test.IO
{
    [TestFixture]
    public class RandomFileTokenProviderTests
    {
        [Test]
        public void ComputeTokenInvalidStreamTest()
        {
            var provider = new RandomFileTokenProvider();

            Assert.That(() => provider.ComputeToken(null),
                    Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("stream"));            
        }


        [Test]
        public void ComputeTokenIsRandomTest()
        {
            var tokens = new HashSet<string>();

            var provider = new RandomFileTokenProvider();            

            byte[] data = {1, 2, 3, 4, 5, 6};
            using (var ms = new MemoryStream(data))
            {
                tokens.Add(provider.ComputeToken(ms));
            }

            using (var ms = new MemoryStream(data))
            {
                tokens.Add(provider.ComputeToken(ms));
            }

            byte[] data2 = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            using (var ms = new MemoryStream(data2))
            {
                tokens.Add(provider.ComputeToken(ms));
            }

            Assert.AreEqual(3, tokens.Count, "The number of tokens is invalid");
        }
    }
}