// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Collections.Generic;
using NUnit.Framework;

namespace EDC.Test.Collections.Generic
{
    [TestFixture]
    public class BidirectionalMultidictionaryTests
    {
        [Test]
        public void Test_Ctor()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
        }

        [Test]
        public void Test_AddOrUpdate_Single()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;
            const string testKey = "foo";
            const int testValue = 1;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            bidirectionalMultidictionary.AddOrUpdate(testKey, testValue);

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").EquivalentTo(new[] { testKey }));
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").EquivalentTo(new[] { testValue }));
            Assert.That(bidirectionalMultidictionary.KeysToValues, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.KeysToValues[testKey], Is.EquivalentTo(new[] { testValue }));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys[testValue], Is.EquivalentTo(new[] { testKey }));
            Assert.That(bidirectionalMultidictionary.GetValues(testKey), Is.EquivalentTo(new[] { testValue }));
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue), Is.EquivalentTo(new[] { testKey }));
        }

        [Test]
        public void Test_AddOrUpdate_ExistingKey()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;
            const string testKey = "foo";
            int[] testValues = new[] { 1, 2, 4, 42, 123 };

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            foreach (int i in testValues)
            {
                bidirectionalMultidictionary.AddOrUpdate(testKey, i);
            }

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").EquivalentTo(new[] { testKey }));
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").EquivalentTo(testValues));
            Assert.That(bidirectionalMultidictionary.KeysToValues[testKey], Is.EquivalentTo(testValues));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Has.Count.EqualTo(testValues.Length));
            foreach (int i in testValues)
            {
                Assert.That(bidirectionalMultidictionary.ValuesToKeys[i], Is.EquivalentTo(new[] { testKey }));
                Assert.That(bidirectionalMultidictionary.GetKeys(i), Is.EquivalentTo(new[] { testKey }));
            }
            Assert.That(bidirectionalMultidictionary.GetValues(testKey), Is.EquivalentTo(testValues));
        }

        [Test]
        public void Test_AddOrUpdate_DifferentKeys()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;
            string[] testKeys = new[] { "foo", "bar", "baz" };
            const int testValue = 42;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            foreach (string key in testKeys)
            {
                bidirectionalMultidictionary.AddOrUpdate(key, testValue);
            }

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").EquivalentTo(testKeys));
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").EquivalentTo(new[] { testValue }));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys[testValue], Is.EquivalentTo(testKeys));
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue), Is.EquivalentTo(testKeys));
            Assert.That(bidirectionalMultidictionary.KeysToValues, Has.Count.EqualTo(testKeys.Length));
            foreach (string key in testKeys)
            {
                Assert.That(bidirectionalMultidictionary.KeysToValues[key], Is.EquivalentTo(new[] { testValue }));
                Assert.That(bidirectionalMultidictionary.GetValues(key), Is.EquivalentTo(new[] { testValue }));
            }
        }

        [Test]
        public void Test_AddOrUpdate_Update()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;
            const string testKey = "foo";
            const int testValue = 42;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            bidirectionalMultidictionary.AddOrUpdate(testKey, testValue);
            bidirectionalMultidictionary.AddOrUpdate(testKey, testValue);

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").EquivalentTo(new[] { testKey }));
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").EquivalentTo(new[] { testValue }));
            Assert.That(bidirectionalMultidictionary.KeysToValues, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.KeysToValues[testKey], Is.EquivalentTo(new[] { testValue }));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys[testValue], Is.EquivalentTo(new[] { testKey }));
            Assert.That(bidirectionalMultidictionary.GetValues(testKey), Is.EquivalentTo(new[] { testValue }));
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue), Is.EquivalentTo(new[] { testKey }));
        }

        [Test]
        public void Test_RemoveKey_Empty()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            Assert.That(() => bidirectionalMultidictionary.RemoveKey("foo"), Throws.Nothing);

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").Empty);
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").Empty);
            Assert.That(bidirectionalMultidictionary.KeysToValues, Is.Empty);
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetValues("foo"), Is.Empty);
        }

        [Test]
        public void Test_RemoveKey_Single()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;
            const string testKey = "foo";
            const int testValue = 42;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            bidirectionalMultidictionary.AddOrUpdate(testKey, testValue);
            bidirectionalMultidictionary.RemoveKey(testKey);

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").Empty);
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").Empty);
            Assert.That(bidirectionalMultidictionary.KeysToValues, Is.Empty);
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue), Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetValues(testKey), Is.Empty);
        }

        [Test]
        public void Test_RemoveKey_OneFromTwo()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;
            const string testKey1 = "foo";
            const string testKey2 = "bar";
            const int testValue1 = 42;
            const int testValue2 = 54;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            bidirectionalMultidictionary.AddOrUpdate(testKey1, testValue1);
            bidirectionalMultidictionary.AddOrUpdate(testKey2, testValue2);
            bidirectionalMultidictionary.RemoveKey(testKey1);

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").EquivalentTo(new[] { testKey2 }));
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").EquivalentTo(new[] { testValue2 }));
            Assert.That(bidirectionalMultidictionary.KeysToValues, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.KeysToValues[testKey2], Is.EquivalentTo(new[] { testValue2 }));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys[testValue2], Is.EquivalentTo(new[] { testKey2 }));
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue1), Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetValues(testKey1), Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue2), Is.EquivalentTo(new[] { testKey2 }));
            Assert.That(bidirectionalMultidictionary.GetValues(testKey2), Is.EquivalentTo(new[] { testValue2 }));
        }

        [Test]
        public void Test_RemoveKey_OneFromTwoSameKey()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;
            const string testKey1 = "foo";
            const int testValue1 = 42;
            const int testValue2 = 54;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            bidirectionalMultidictionary.AddOrUpdate(testKey1, testValue1);
            bidirectionalMultidictionary.AddOrUpdate(testKey1, testValue2);
            bidirectionalMultidictionary.RemoveKey(testKey1);

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").Empty);
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").Empty);
            Assert.That(bidirectionalMultidictionary.KeysToValues, Is.Empty);
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue1), Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue2), Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetValues(testKey1), Is.Empty);
        }

        [Test]
        public void Test_RemoveKey_OneFromTwoSameValue()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;
            const string testKey1 = "foo";
            const string testKey2 = "bar";
            const int testValue1 = 42;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            bidirectionalMultidictionary.AddOrUpdate(testKey1, testValue1);
            bidirectionalMultidictionary.AddOrUpdate(testKey2, testValue1);
            bidirectionalMultidictionary.RemoveKey(testKey1);

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").EquivalentTo(new[] { testKey2 }));
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").EquivalentTo(new[] { testValue1 }));
            Assert.That(bidirectionalMultidictionary.KeysToValues, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.KeysToValues[testKey2], Is.EquivalentTo(new[] { testValue1 }));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys[testValue1], Is.EquivalentTo(new[] { testKey2 }));
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue1), Is.EquivalentTo(new[] { testKey2 }));
            Assert.That(bidirectionalMultidictionary.GetValues(testKey1), Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetValues(testKey2), Is.EquivalentTo(new[] { testValue1 }));
        }

        [Test]
        public void Test_RemoveValue_Empty()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            Assert.That(() => bidirectionalMultidictionary.RemoveValue(42), Throws.Nothing);

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").Empty);
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").Empty);
            Assert.That(bidirectionalMultidictionary.KeysToValues, Is.Empty);
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetKeys(42), Is.Empty);
        }

        [Test]
        public void Test_RemoveValue_Single()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;
            const string testKey = "foo";
            const int testValue = 42;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            bidirectionalMultidictionary.AddOrUpdate(testKey, testValue);
            bidirectionalMultidictionary.RemoveValue(testValue);

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").Empty);
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").Empty);
            Assert.That(bidirectionalMultidictionary.KeysToValues, Is.Empty);
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue), Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetValues(testKey), Is.Empty);
        }

        [Test]
        public void Test_RemoveValue_OneFromTwo()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;
            const string testKey1 = "foo";
            const string testKey2 = "bar";
            const int testValue1 = 42;
            const int testValue2 = 54;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            bidirectionalMultidictionary.AddOrUpdate(testKey1, testValue1);
            bidirectionalMultidictionary.AddOrUpdate(testKey2, testValue2);
            bidirectionalMultidictionary.RemoveValue(testValue1);

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").EquivalentTo(new[] { testKey2 }));
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").EquivalentTo(new[] { testValue2 }));
            Assert.That(bidirectionalMultidictionary.KeysToValues, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.KeysToValues[testKey2], Is.EquivalentTo(new[] { testValue2 }));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys[testValue2], Is.EquivalentTo(new[] { testKey2 }));
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue1), Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetValues(testKey1), Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue2), Is.EquivalentTo(new[] { testKey2 }));
            Assert.That(bidirectionalMultidictionary.GetValues(testKey2), Is.EquivalentTo(new[] { testValue2 }));
        }

        [Test]
        public void Test_RemoveValue_OneFromTwoSameKey()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;
            const string testKey1 = "foo";
            const int testValue1 = 42;
            const int testValue2 = 54;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            bidirectionalMultidictionary.AddOrUpdate(testKey1, testValue1);
            bidirectionalMultidictionary.AddOrUpdate(testKey1, testValue2);
            bidirectionalMultidictionary.RemoveValue(testValue1);

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").EquivalentTo(new[] { testKey1 }));
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").EquivalentTo(new[] { testValue2 }));
            Assert.That(bidirectionalMultidictionary.KeysToValues, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.KeysToValues[testKey1], Is.EquivalentTo(new[] { testValue2 }));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Has.Count.EqualTo(1));
            Assert.That(bidirectionalMultidictionary.ValuesToKeys[testValue2], Is.EquivalentTo(new[] { testKey1 }));
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue1), Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue2), Is.EquivalentTo(new[] { testKey1 }));
            Assert.That(bidirectionalMultidictionary.GetValues(testKey1), Is.EquivalentTo(new[] { testValue2 }));
        }

        [Test]
        public void Test_RemoveValue_OneFromTwoSameValue()
        {
            BidirectionalMultidictionary<string, int> bidirectionalMultidictionary;
            const string testKey1 = "foo";
            const string testKey2 = "bar";
            const int testValue1 = 42;

            bidirectionalMultidictionary = new BidirectionalMultidictionary<string, int>();
            bidirectionalMultidictionary.AddOrUpdate(testKey1, testValue1);
            bidirectionalMultidictionary.AddOrUpdate(testKey2, testValue1);
            bidirectionalMultidictionary.RemoveValue(testValue1);

            Assert.That(bidirectionalMultidictionary, Has.Property("Keys").Empty);
            Assert.That(bidirectionalMultidictionary, Has.Property("Values").Empty);
            Assert.That(bidirectionalMultidictionary.KeysToValues, Is.Empty);
            Assert.That(bidirectionalMultidictionary.ValuesToKeys, Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetKeys(testValue1), Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetValues(testKey1), Is.Empty);
            Assert.That(bidirectionalMultidictionary.GetValues(testKey2), Is.Empty);
        }

        [Test]
        public void Test_AddOrUpdateKeys_Null()
        {
            Assert.That(
                () => new BidirectionalMultidictionary<long, long>().AddOrUpdateKeys(null, 0),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("keys"));
        }

        [Test]
        public void Test_AddOrUpdateKeys_Empty()
        {
            BidirectionalMultidictionary<long, string> bidirectionalMultidictionary;
            const string value = "test";
            long[] keys = { 1, 2 };

            bidirectionalMultidictionary = new BidirectionalMultidictionary<long, string>();
            bidirectionalMultidictionary.AddOrUpdateKeys(keys, value);

            Assert.That(bidirectionalMultidictionary.Keys, Is.EquivalentTo(keys));
            Assert.That(bidirectionalMultidictionary.Values, Is.EquivalentTo(new[] { value }));
            Assert.That(bidirectionalMultidictionary.GetKeys(value), Is.EquivalentTo(keys));
            Assert.That(bidirectionalMultidictionary.GetValues(keys[0]), Is.EquivalentTo(new[] { value }));
            Assert.That(bidirectionalMultidictionary.GetValues(keys[1]), Is.EquivalentTo(new[] { value }));
        }

        [Test]
        public void Test_AddOrUpdateKeys_Overlap()
        {
            BidirectionalMultidictionary<long, string> bidirectionalMultidictionary;
            const string value = "test";
            long[] firstKeys = { 1, 2 };
            long[] secondKeys = { 2, 3 };

            bidirectionalMultidictionary = new BidirectionalMultidictionary<long, string>();
            bidirectionalMultidictionary.AddOrUpdateKeys(firstKeys, value);
            bidirectionalMultidictionary.AddOrUpdateKeys(secondKeys, value);

            Assert.That(bidirectionalMultidictionary.Keys, Is.EquivalentTo(firstKeys.Union(secondKeys)));
            Assert.That(bidirectionalMultidictionary.Values, Is.EquivalentTo(new[] { value }));
            Assert.That(bidirectionalMultidictionary.GetKeys(value), Is.EquivalentTo(firstKeys.Union(secondKeys)));
            Assert.That(bidirectionalMultidictionary.GetValues(firstKeys[0]), Is.EquivalentTo(new[] { value }));
            Assert.That(bidirectionalMultidictionary.GetValues(firstKeys[1]), Is.EquivalentTo(new[] { value }));
            Assert.That(bidirectionalMultidictionary.GetValues(secondKeys[0]), Is.EquivalentTo(new[] { value }));
            Assert.That(bidirectionalMultidictionary.GetValues(secondKeys[1]), Is.EquivalentTo(new[] { value }));
        }

        [Test]
        public void Test_RemoveValues_Null()
        {
            Assert.That(
                () => new BidirectionalMultidictionary<long, long>().RemoveValues(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("values"));
        }

        [Test]
        public void Test_RemoveValues_Single()
        {
            BidirectionalMultidictionary<long, string> bidirectionalMultidictionary;
            const long key = 1;
            const string value1 = "foo";
            const string value2 = "bar";

            bidirectionalMultidictionary = new BidirectionalMultidictionary<long, string>();
            bidirectionalMultidictionary.AddOrUpdate(key, value1);
            bidirectionalMultidictionary.AddOrUpdate(key, value2);

            bidirectionalMultidictionary.RemoveValues(new[] { value1 });

            Assert.That(bidirectionalMultidictionary.Keys, Is.EquivalentTo(new[] { key }));
            Assert.That(bidirectionalMultidictionary.Values, Is.EquivalentTo(new[] { value2 }));
            Assert.That(bidirectionalMultidictionary.GetKeys(value2), Is.EquivalentTo(new[] { key }));
            Assert.That(bidirectionalMultidictionary.GetValues(key), Is.EquivalentTo(new[] { value2 }));
        }

        [Test]
        public void Test_RemoveValues_Double()
        {
            BidirectionalMultidictionary<long, string> bidirectionalMultidictionary;
            const long key = 1;
            const string value1 = "foo";
            const string value2 = "bar";

            bidirectionalMultidictionary = new BidirectionalMultidictionary<long, string>();
            bidirectionalMultidictionary.AddOrUpdate(key, value1);
            bidirectionalMultidictionary.AddOrUpdate(key, value2);

            bidirectionalMultidictionary.RemoveValues(new[] { value1, value2 });

            Assert.That(bidirectionalMultidictionary.Keys, Is.Empty);
            Assert.That(bidirectionalMultidictionary.Values, Is.Empty);
        }
    }
}
