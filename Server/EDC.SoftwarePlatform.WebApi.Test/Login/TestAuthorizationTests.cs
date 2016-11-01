// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Threading;
using EDC.SoftwarePlatform.WebApi.Controllers.Login;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.Login
{
    [TestFixture]
    public class TestAuthorizationTests
    {
        [Test]
        public void TestNoToken()
        {
            var ta = new TestAuthorization(new TimeSpan(1, 0, 0));
            Assert.IsNull(ta.GetTestToken(5, 6, "AAAA"));
        }

        [Test]
        public void TestGenerateToken( )
        {
            var ta = new TestAuthorization(new TimeSpan(1, 0, 0));

            ta.SetTokenIdentifier(10, 20, "AAAA");

            var token = ta.GetTestToken(10, 20, "AAAA");
            long tenant;
            long provider;
            string id; 

            Assert.IsTrue(ta.TryGetIdentifier(token, out tenant, out provider, out id), "IsValidToken");
            Assert.AreEqual("AAAA", id);
            Assert.AreEqual(10, tenant);
            Assert.AreEqual(20, provider);
        }

        [Test]
        public void TestClearToken()
        {
            var ta = new TestAuthorization(new TimeSpan(1, 0, 0));

            ta.SetTokenIdentifier(20, 21, "BBBB");
            var token = ta.GetTestToken(20, 21, "BBBB");
            string id;
            long tenant;
            long provider;
            ta.ClearToken(20, 21, "BBBB");

            Assert.IsFalse(ta.TryGetIdentifier(token, out tenant, out provider, out id), "TokenCleared");
            Assert.IsNull(id);
            Assert.AreEqual(-1, tenant);
            Assert.AreEqual(-1, provider);
        }

        [Test]
        public void TestTokenReplace()
        {
            var ta = new TestAuthorization(new TimeSpan(1, 0, 0));

            ta.SetTokenIdentifier(30, 31, "CCCC");
            var tokenC = ta.GetTestToken(30, 31, "CCCC");

            ta.SetTokenIdentifier(40, 41, "DDDD");
            var tokenD = ta.GetTestToken(40, 41, "DDDD");

            Assert.AreNotEqual(tokenC, tokenD, "Token replaced");
        }


        [Test]
        public void TestTokenTimeout()
        {
            var ta = new TestAuthorization(new TimeSpan(0, 0, 1));  // one second timeout

            ta.SetTokenIdentifier(50, 51, "EEEE");
            var token = ta.GetTestToken(50, 51, "EEEE");
            string id;
            long tenant;
            long providerId;

            Assert.IsTrue(ta.TryGetIdentifier(token, out tenant, out providerId, out id));

            Thread.Sleep(1500);

            Assert.IsFalse(ta.TryGetIdentifier(token, out tenant, out providerId, out id));
        }

        [Test]
        public void TestTwoTokensIndependent()
        {
            var ta = new TestAuthorization(new TimeSpan(0, 0, 1));  // one second timeout

            ta.SetTokenIdentifier(60, 61, "FFFF");
            var token1 = ta.GetTestToken(60, 61, "FFFF");

            ta.SetTokenIdentifier(70, 71, "GGGG");
            var token2 = ta.GetTestToken(70, 71, "GGGG");

            var token3 = ta.GetTestToken(60, 61, "FFFF");

            Assert.AreSame(token1, token3);
            Assert.AreNotSame(token1, token2);
        }
    }
}
