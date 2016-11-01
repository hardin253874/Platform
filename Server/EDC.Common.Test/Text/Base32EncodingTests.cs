// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.Text;
using NUnit.Framework;

namespace EDC.Test.Text
{
    [TestFixture]
    public class Base32EncodingTests
    {
        [Test]
        public void FromBase32StringInvalidArgs()
        {
            Assert.That(() => Base32Encoding.FromBase32String("A"),
                Throws.TypeOf<FormatException>().And.Property("Message").EqualTo("The input is not a valid Base32 string."));

            Assert.That(() => Base32Encoding.FromBase32String("??"),
                Throws.TypeOf<FormatException>().And.Property("Message").EqualTo("The input is not a valid Base32 string. The character '?' does not exist in the Base32 alphabet."));
        }


        [Test]
        [TestCase(null, null)]
        [TestCase("", new byte[0])]
        [TestCase("AA", new byte[] {0})]
        [TestCase("4Y", new byte[] {230})]
        [TestCase("74", new byte[] {255})]
        [TestCase("MTEEGVYWFQQYCQY", new byte[] {100, 200, 67, 87, 22, 44, 33, 129, 67})]
        [TestCase("BIKB4KBSHRDFAWTENZ4IFDEWUCVLJPWI2LOON4H2", new byte[] {10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 250})]
        public void FromBase32StringTest(string input, byte[] output)
        {
            var bytes = Base32Encoding.FromBase32String(input);

            if (input == null)
            {
                Assert.IsNull(bytes, "The decoded array should be null");
            }
            else
            {
                Assert.That(bytes,
                    Is.EquivalentTo(output), "The decoded arrays strings do not match");
            }
        }


        [Test]
        public void ToBase32StringInvalidArgs()
        {
            Assert.That(() => Base32Encoding.ToBase32String(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("bytes"));
        }


        [Test]
        [TestCase(new byte[0], "")]
        [TestCase(new byte[] {0}, "AA")]
        [TestCase(new byte[] {230}, "4Y")]
        [TestCase(new byte[] {255}, "74")]
        [TestCase(new byte[] {100, 200, 67, 87, 22, 44, 33, 129, 67}, "MTEEGVYWFQQYCQY")]
        [TestCase(new byte[] {10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 250}, "BIKB4KBSHRDFAWTENZ4IFDEWUCVLJPWI2LOON4H2")]
        public void ToBase32StringTests(byte[] input, string output)
        {
            var encodedString = Base32Encoding.ToBase32String(input);

            Assert.AreEqual(output, encodedString, "The encoded strings do not match");
        }
    }
}