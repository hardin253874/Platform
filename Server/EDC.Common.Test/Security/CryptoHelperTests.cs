// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;

using EDC.Security;
using NUnit.Framework;

namespace EDC.Test.Security
{
	/// <summary>
	///     Units tests for the CryptoHelper class.
	/// </summary>
	[TestFixture]
	public class CryptoHelperTests
	{
        /// <summary>
        /// The test data
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
		///     Test creating a salted hash.
		/// </summary>
		[Test]
		public void CreateEncodedSaltedHashTest( )
		{
			const string password = "Test Password";

			// Create a hash of the password
			string encodedHash = CryptoHelper.CreateEncodedSaltedHash( password );

			Assert.IsNotNull( encodedHash, "The encoded hash should not be null." );
		}

        /// <summary>
        ///     Test hashing a null password.
        /// </summary>
        [Test]
        public void CreateEncodedSaltedHashNullInputTest()
        {
            Assert.That(() => CryptoHelper.CreateEncodedSaltedHash(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("input"));
        }

		/// <summary>
		///     Test that creating a salted hash for the same password are different.
		/// </summary>
		[Test]
		public void CreateEncodedSaltedHashUniqueHashTest( )
		{
			const string password = "Test Password";

			// Create a hash of the password
			string encodedHash1 = CryptoHelper.CreateEncodedSaltedHash( password );
			Assert.IsNotNull( encodedHash1, "The encoded hash should not be null." );

			string encodedHash2 = CryptoHelper.CreateEncodedSaltedHash( password );
			Assert.IsNotNull( encodedHash2, "The encoded hash should not be null." );

			Assert.AreNotEqual( encodedHash1, encodedHash2, "The hashes should not be equal" );
		}

		/// <summary>
		///     Verify that the GetMD5Hash method generates different hashes for different input values.
		/// </summary>
		[Test]
		public void GetMD5Hash_DifferentValues( )
		{
			var hashToInputDictionary = new Dictionary<string, string>( );

			for ( int i = 0; i < 100; i++ )
			{
				string inputValue = "This is a test " + i.ToString( CultureInfo.InvariantCulture );
				string hash = CryptoHelper.GetMd5Hash( inputValue );

				Assert.IsTrue( !string.IsNullOrEmpty( hash ), "The hash value should not be null or empty." );
				Assert.IsTrue( hash.Length == 32, "The hash value does not appear to be a valid MD5 hash." );

				Assert.IsFalse( hashToInputDictionary.ContainsKey( hash ), "The hash value already exists." );

				hashToInputDictionary[ hash ] = inputValue;
			}
		}


		/// <summary>
		///     Verify that the GetMD5Hash method returns a valid exception when an empty value is passed.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void GetMD5Hash_EmptyInputValue( )
		{
			CryptoHelper.GetMd5Hash( "" );
		}

		/// <summary>
		///     Verify that the GetMD5Hash method retuns a valid exception when a null value is passed.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void GetMD5Hash_NullInputValue( )
		{
			CryptoHelper.GetMd5Hash( null );
		}

		/// <summary>
		///     Verify that the GetMD5Hash method generates the same hash for the same
		///     input value.
		/// </summary>
		[Test]
		public void GetMD5Hash_SameValues( )
		{
			const string inputValue1 = "This is a test 1";
			string hash1 = CryptoHelper.GetMd5Hash( inputValue1 );

			Assert.IsTrue( !string.IsNullOrEmpty( hash1 ), "The hash1 value should not be null or empty." );
			Assert.IsTrue( hash1.Length == 32, "The hash1 value does not appear to be a valid MD5 hash." );

			const string inputValue2 = "This is a test 1";
			string hash2 = CryptoHelper.GetMd5Hash( inputValue2 );

			Assert.IsTrue( !string.IsNullOrEmpty( hash2 ), "The hash2 value should not be null or empty." );
			Assert.IsTrue( hash2.Length == 32, "The hash2 value does not appear to be a valid MD5 hash." );

			Assert.AreEqual( hash1, hash2, "The hash values should be the same" );
		}

		/// <summary>
		///     Verify that the GetMD5Hash method runs.
		/// </summary>
		[Test]
		public void GetMD5Hash_ValidHash( )
		{
			const string inputValue = "This is a test";
			string hash = CryptoHelper.GetMd5Hash( inputValue );

			Assert.IsTrue( !string.IsNullOrEmpty( hash ), "The hash value should not be null or empty." );
			Assert.IsTrue( hash.Length == 32, "The hash value does not appear to be a valid MD5 hash." );
		}

		/// <summary>
		///     Gets the random bytes test.
		/// </summary>
		[Test]
		public void GetRandomBytesTest( )
		{
			byte[] randomBytes = CryptoHelper.GetRandomBytes( 24 );

			Assert.AreEqual( 24, randomBytes.Length );
		}

        [Test]
        public void GetRandomReadableStringTest()
        {
            var s = CryptoHelper.GetRandomPrintableString(100);

            Assert.AreEqual(s.Length, 100);
        }


		/// <summary>
		///     Verify that the GetSHA1Hash method generates different hashes for different input values.
		/// </summary>
		[Test]
		public void GetSHA1Hash_DifferentValues( )
		{
			var hashToInputDictionary = new Dictionary<string, string>( );

			for ( int i = 0; i < 100; i++ )
			{
				string inputValue = "This is a test " + i.ToString( CultureInfo.InvariantCulture );
				string hash = CryptoHelper.GetSha1Hash( inputValue );

				Assert.IsTrue( !string.IsNullOrEmpty( hash ), "The hash value should not be null or empty." );
				Assert.IsTrue( hash.Length == 40, "The hash value does not appear to be a valid SHA1 hash." );

				Assert.IsFalse( hashToInputDictionary.ContainsKey( hash ), "The hash value already exists." );

				hashToInputDictionary[ hash ] = inputValue;
			}
		}


		/// <summary>
		///     Verify that the GetMD5Hash method retuns a valid exception when an empty value is passed.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void GetSHA1Hash_EmptyInputValue( )
		{
			CryptoHelper.GetSha1Hash( "" );
		}

		/// <summary>
		///     Verify that the GetSHA1Hash method retuns a valid exception when a null value is passed.
		/// </summary>
		[Test]
		[ExpectedException( typeof ( ArgumentNullException ) )]
		public void GetSHA1Hash_NullInputValue( )
		{
			CryptoHelper.GetSha1Hash( null );
		}

		/// <summary>
		///     Verify that the GetSHA1Hash method generates the same hash for the same
		///     input value.
		/// </summary>
		[Test]
		public void GetSHA1Hash_SameValues( )
		{
			const string inputValue1 = "This is a test 1";
			string hash1 = CryptoHelper.GetSha1Hash( inputValue1 );

			Assert.IsTrue( !string.IsNullOrEmpty( hash1 ), "The hash1 value should not be null or empty." );
			Assert.IsTrue( hash1.Length == 40, "The hash1 value does not appear to be a valid SHA1 hash." );

			const string inputValue2 = "This is a test 1";
			string hash2 = CryptoHelper.GetSha1Hash( inputValue2 );

			Assert.IsTrue( !string.IsNullOrEmpty( hash2 ), "The hash2 value should not be null or empty." );
			Assert.IsTrue( hash2.Length == 40, "The hash2 value does not appear to be a valid SHA1 hash." );

			Assert.AreEqual( hash1, hash2, "The hash values should be the same" );
		}

		/// <summary>
		///     Verify that the GetSHA1Hash method runs.
		/// </summary>
		[Test]
		public void GetSHA1Hash_ValidHash( )
		{
			const string inputValue = "This is a test";
			string hash = CryptoHelper.GetSha1Hash( inputValue );

			Assert.IsTrue( !string.IsNullOrEmpty( hash ), "The hash value should not be null or empty." );
			Assert.IsTrue( hash.Length == 40, "The hash value does not appear to be a valid SHA1 hash." );
		}


		/// <summary>
		///     Validates the password using an invalid password.
		/// </summary>
		[Test]
		public void ValidatePasswordInvalidPasswordTest( )
		{
			const string password = "Test Password";

			// Create a hash of the password
			string encodedHash = CryptoHelper.CreateEncodedSaltedHash( password );

			// Validate the password against the hash
			bool isvalid = CryptoHelper.ValidatePassword( "New Test Password", encodedHash );

			Assert.IsFalse( isvalid, "The password validated." );
		}

		/// <summary>
		///     Validates the password using a valid password.
		/// </summary>
		[Test]
		public void ValidatePasswordValidPasswordTest( )
		{
			const string password = "Test Password";

			// Create a hash of the password
			string encodedHash = CryptoHelper.CreateEncodedSaltedHash( password );

			// Validate the password against the hash
			bool isvalid = CryptoHelper.ValidatePassword( password, encodedHash );

			Assert.IsTrue( isvalid, "The password failed to validate" );
		}


        /// <summary>
        /// Verify that the ComputeSha1Hash method retuns a valid exception when a null value is passed.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComputeSha1HashNullStringTest()
        {
            CryptoHelper.ComputeSha1Hash((string)null);
        }


        /// <summary>
        /// Verify that the ComputeSha1Hash method retuns a valid exception when a null value is passed.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComputeSha1HashNullStreamTest()
        {
            CryptoHelper.ComputeSha1Hash((Stream)null);
        }


        /// <summary>
        ///     Validates the password using a valid password.
        /// </summary>
        [Test]
        public void ComputeSha1HashTest()
        {
            // Create the first file
            string path = string.Empty;

            try
            {
                path = Path.GetTempFileName();

                File.WriteAllText(path, TestData);

                // Create a hash from a file
                string hashFromFile = CryptoHelper.ComputeSha1Hash(path);
                Assert.IsNotNullOrEmpty(hashFromFile, "The hash from file should not be empty.");

                // Create a hash from a data stream
                using (var ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(ms, Encoding.ASCII, 4096, true))
                    {
                        sw.Write(TestData);
                    }

                    string hashFromStream = CryptoHelper.ComputeSha1Hash(ms);
                    Assert.IsNotNullOrEmpty(hashFromStream, "The hash from stream should not be empty.");

                    Assert.AreEqual(hashFromFile, hashFromStream, "The hashes should match.");
                }                
            }
            finally 
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }                        
        }


        /// <summary>
        ///     Test hashing a null password.
        /// </summary>
        [Test]
        public void CreateEncodedSaltedHashWithHashSettingsNullInputTest()
        {
            Assert.That(() => CryptoHelper.CreateEncodedSaltedHash(null, new CryptoHelper.HashSettings()),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("input"));
        }

        /// <summary>
        ///     Test hashing a password with null hashsettings.
        /// </summary>
        [Test]
        public void CreateEncodedSaltedHashWithHashSettingsNullHashSettingsTest()
        {
            Assert.That(() => CryptoHelper.CreateEncodedSaltedHash("abc", null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("hashSettings"));
        }


        [Test]
        public void HashHashesTest_EmptyEqual()
        {
            var hashEmpty1 = CryptoHelper.HashHashes(new List<int> { });
            var hashEmpty2 = CryptoHelper.HashHashes(new List<int> { });

            Assert.That(hashEmpty1, Is.EqualTo(hashEmpty2));
        }


        [Test]
        public void HashHashesTest_DifferentsDifferent()
        {
            var hash1 = CryptoHelper.HashHashes(new List<int> { 2 });
            var hash2 = CryptoHelper.HashHashes(new List<int> { 1 });

            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }

        [Test]
        public void HashHashesTest_SameSame()
        {
            var hash1 = CryptoHelper.HashHashes(new List<int> { 1, 2 });
            var hash2 = CryptoHelper.HashHashes(new List<int> { 1, 2 });

            Assert.That(hash1, Is.EqualTo(hash2));
        }

        [Test]
        public void HashHashesTest_OrderImportant()
        {
            var hash1 = CryptoHelper.HashHashes(new List<int> { 1, 2 });
            var hash2 = CryptoHelper.HashHashes(new List<int> { 2, 1 });

            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }


        [Test]
        public void HashObjectsTest_EmptyEqual()
        {
            var hashEmpty1 = CryptoHelper.HashObjects(new List<object> { });
            var hashEmpty2 = CryptoHelper.HashObjects(new List<object> { });

            Assert.That(hashEmpty1, Is.EqualTo(hashEmpty2));
        }

        [Test]
        public void HashObjectsTest_EmptyEqual2()
        {
            var hashEmpty1 = CryptoHelper.HashValues(new List<long> { });
            var hashEmpty2 = CryptoHelper.HashValues(new List<long> { });

            Assert.That(hashEmpty1, Is.EqualTo(hashEmpty2));
        }

        [Test]
        public void HashObjectsTest_NullsEqual()
        {
            var hashNull1 = CryptoHelper.HashObjects(new List<object> { null });
            var hashNull2 = CryptoHelper.HashObjects(new List<object> { null });

            Assert.That(hashNull1, Is.EqualTo(hashNull2));
        }

        [Test]
        public void HashObjectsTest_NullsListEqual()
        {
            var hashNull1 = CryptoHelper.HashObjects(null);
            var hashNull2 = CryptoHelper.HashObjects(null);

            Assert.That(hashNull1, Is.EqualTo(hashNull2));
        }

    }
}