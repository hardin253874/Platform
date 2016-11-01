// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Security;
using NUnit.Framework;

namespace EDC.Test.Security
{
    /// <summary>
    ///     Units tests for the EncodingCryptoProvider class.
    /// </summary>
    [TestFixture]
    public class EncodingCryptoProviderTests
    {
        /// <summary>
        ///     The test data
        /// </summary>
        private const string TestData = @"Felis catus is your taxonomic nomenclature,
                                      An endothermic quadruped, carnivorous by nature;
                                      Your visual, olfactory, and auditory senses
                                      Contribute to your hunting skills and natural defenses.

                                      I find myself intrigued by your subvocal oscillations,
                                      A singular development of cat communications
                                      That obviates your basic hedonistic predilection
                                      For a rhythmic stroking of your fur to demonstrate affection.

                                      A tail is quite essential for your acrobatic talents;
                                      You would not be so agile if you lacked its counterbalance.
                                      And when not being utilized to aid in locomotion,
                                      It often serves to illustrate the state of your emotion.

                                      O Spot, the complex levels of behavior you display
                                      Connote a fairly well-developed cognitive array.
                                      And though you are not sentient, Spot, and do not comprehend,
                                      I nonetheless consider you a true and valued friend.";


        /// <summary>
        ///     Encrypts and encodes and then decodes and decrypts the data.
        /// </summary>
        [Test]
        public void EncodeDecodeData()
        {
            var cryptoProvider = new EncodingCryptoProvider();            
            
            string encodedCipherText = cryptoProvider.EncryptAndEncode(TestData);
            Assert.AreEqual(TestData, cryptoProvider.DecodeAndDecrypt(encodedCipherText));
        }


        /// <summary>
        ///     Encrypts the same data and verifies that it generates different cipher text.
        /// </summary>
        [Test]
        public void EncryptEncodeSameDataGeneratesDifferentCipherText()
        {
            var cryptoProvider = new EncodingCryptoProvider();

            string encodedCipherText1 = cryptoProvider.EncryptAndEncode(TestData);
            string encodedCipherText2 = cryptoProvider.EncryptAndEncode(TestData);

            Assert.AreNotEqual(encodedCipherText1, encodedCipherText2, "The encoded cipher texts should not be equal");
        }
    }
}