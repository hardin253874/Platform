// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using EDC.Text;
using NUnit.Framework;

namespace EDC.Test.Text
{
    [TestFixture]
    public class DictionaryFormatterTests
    {
        /// <summary>
        ///     Tests dictionary formatter using a dictionary argument.
        /// </summary>
        [Test]
        [TestCase("Test FF", "Test {0:d1p1,X}", new[] {"d1p1"}, new[] {(object)255})]
        [TestCase("Test   ", "Test {0:d1p1} {0:d1p2} {0}", new[] {"d1p1"}, new[] {(object) null})]
        public void TestDictionaryArg(string result, string format, string[] keys, object[] values)
        {
            IDictionary<string, object> dict1 = new Dictionary<string, object>();

            for (int i = 0; i < keys.Length; i++)
            {
                dict1[keys[i]] = values[i];
            }

            Assert.AreEqual(result, string.Format(new DictionaryFormatter(), format, dict1));
        }


        /// <summary>
        ///     Tests the dictionary formatter without a dictionary arg.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="format">The format.</param>
        /// <param name="arg">The argument.</param>
        [Test]
        [TestCase("Test FF", "Test {0:X}", 255)]
        [TestCase("Test 255", "Test {0}", 255)]
        public void TestNonDictionaryArg(string result, string format, object arg)
        {
            Assert.AreEqual(result, string.Format(new DictionaryFormatter(), format, arg));
        }


        /// <summary>
        ///     Tests dictionary formatter using a dictionary argument without formatting.
        /// </summary>
        [Test]
        public void TestUnformattedData()
        {
            IDictionary<string, string> dict1 = new Dictionary<string, string>
            {
                {"d1p1", "D1P1"},
                {"d1p2", "D1P2"}
            };

            IDictionary<string, string> dict2 = new Dictionary<string, string>
            {
                {"d2p1", "D2P1"},
                {"d2p2", "D2P2"}
            };

            Assert.AreEqual("Test D1P1 D1P2 D2P1 D2P2", string.Format(new DictionaryFormatter(), "Test {0:d1p1} {0:d1p2} {1:d2p1} {1:d2p2}", dict1, dict2));
        }
    }
}