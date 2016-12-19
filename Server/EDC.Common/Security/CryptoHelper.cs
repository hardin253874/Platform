// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EDC.Security
{
	/// <summary>
	///     Provides helper methods for interacting with cryptography APIs.
	/// </summary>
	public static class CryptoHelper
	{
	    private const int Sha256Chunkiness = 256 * 1024;         // 256Kb Chunks
        private const int NullHash = Int32.MaxValue - 1234;      // Used to get a consistent hash for a null when merging hashes. There is a chance of a collision.

	/// <summary>
	///     Returns the MD5 hash of the specified input value.
	/// </summary>
	/// <param name="input">The string value to hash.</param>
	/// <returns>The MD5 hash of the input value.</returns>
		public static string GetMd5Hash( string input )
		{
			if ( string.IsNullOrEmpty( input ) )
			{
				throw new ArgumentNullException( nameof( input ) );
			}

			using ( MD5 md5Hash = MD5.Create( ) )
			{
				// Hash the input value
				byte[] data = md5Hash.ComputeHash( Encoding.UTF8.GetBytes( input ) );

				// Convert the hashed data to a hexadecimal string.
				var hashBuilder = new StringBuilder( );

				for ( int i = 0; i < data.Length; i++ )
				{
					hashBuilder.Append( data[ i ].ToString( "x2" ).ToUpper( ) );
				}

				return hashBuilder.ToString( );
			}
		}

        /// <summary>
        ///     Computes the sha256 hash of the specified file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static string ComputeSha256Hash(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException( nameof( filePath ) );
            }

            using (BufferedStream source = new BufferedStream(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), Sha256Chunkiness))
            {
                return ComputeSha1Hash(source);
            }
        }

        /// <summary>
        ///     Computes the sha256 hash of the specified stream.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <returns></returns>
        public static string ComputeSha256Hash(Stream sourceStream)
        {
            if (sourceStream == null)
            {
                throw new ArgumentNullException( nameof( sourceStream ) );
            }

            byte[] hashBytes;

            // Hash the incoming data
            using (SHA256Managed sha256Hash = new SHA256Managed())
            {
                sourceStream.Position = 0;
                hashBytes = sha256Hash.ComputeHash(sourceStream);
            }

            sourceStream.Position = 0;
            return Convert.ToBase64String(hashBytes);
        }


		/// <summary>
		///     Returns the SHA1 hash of the specified input value.
		/// </summary>
		/// <param name="input">The string value to hash.</param>
		/// <returns>The SHA1 hash of the input value.</returns>
		public static string GetSha1Hash( string input )
		{
			if ( string.IsNullOrEmpty( input ) )
			{
				throw new ArgumentNullException( nameof( input ) );
			}

			using ( SHA1 sha1Hash = new SHA1Managed( ) )
			{
				// Hash the input value
				byte[] data = sha1Hash.ComputeHash( Encoding.UTF8.GetBytes( input ) );

				// Convert the hashed data to a hexadecimal string.
				var hashBuilder = new StringBuilder( );

				for ( int i = 0; i < data.Length; i++ )
				{
					hashBuilder.Append( data[ i ].ToString( "x2" ).ToUpper( ) );
				}

				return hashBuilder.ToString( );
			}
		}


		/// <summary>
		///     Encodes the salted hash.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="salt">The salt.</param>
		/// <param name="hash">The hash.</param>
		/// <returns></returns>
		private static string EncodeSaltedHash( int version, byte[] salt, byte[] hash )
		{
			if ( version <= 0 )
			{
				throw new ArgumentOutOfRangeException( nameof( version ) );
			}

			if ( salt == null )
			{
				throw new ArgumentNullException( nameof( salt ) );
			}

			if ( salt.Length == 0 )
			{
				throw new ArgumentException( @"The salt is empty.", nameof( salt ) );
			}

			if ( hash == null )
			{
				throw new ArgumentNullException( nameof( hash ) );
			}

			if ( hash.Length == 0 )
			{
				throw new ArgumentException( @"The hash is empty.", nameof( hash ) );
			}

			return string.Format( "{0}|{1}|{2}", version, Convert.ToBase64String( salt ), Convert.ToBase64String( hash ) );
		}


		/// <summary>
		///     Tries the decode salted hash.
		/// </summary>
		/// <param name="encodedHash">The encoded hash.</param>
		/// <param name="version">The version.</param>
		/// <param name="salt">The salt.</param>
		/// <param name="hash">The hash.</param>
		/// <returns></returns>
		private static bool TryDecodeSaltedHash( string encodedHash, out int version, out byte[] salt, out byte[] hash )
		{
			bool result = false;

			version = 0;
			salt = null;
			hash = null;

			if ( string.IsNullOrEmpty( encodedHash ) )
			{
				return false;
			}

			string[] hashParts = encodedHash.Split( '|' );
			if ( hashParts.Length != 3 )
			{
				return false;
			}

			string versionPart = hashParts[ 0 ];
			string encodedSaltPart = hashParts[ 1 ];
			string encodedHashPart = hashParts[ 2 ];

			if ( !string.IsNullOrEmpty( versionPart ) )
			{
				int.TryParse( versionPart, out version );
			}

			if ( !string.IsNullOrEmpty( encodedSaltPart ) )
			{
				salt = Convert.FromBase64String( encodedSaltPart );
			}

			if ( !string.IsNullOrEmpty( encodedHashPart ) )
			{
				hash = Convert.FromBase64String( encodedHashPart );
			}

			if ( version > 0 &&
			     salt != null &&
			     salt.Length > 0 &&
			     hash != null &&
			     hash.Length > 0 )
			{
				result = true;
			}

			return result;
		}


		/// <summary>
		///     Creates the salted hash using the default <see cref="HashSettings"/>.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="input"/> cannot be null.
		/// </exception>
		public static string CreateEncodedSaltedHash( string input )
		{
			if ( string.IsNullOrEmpty( input ) )
			{
				throw new ArgumentNullException( nameof( input ) );
			}

			// Get the current hash settings
			HashSettings hashSettings = HashSettings.GetHashSettings( );

			return CreateEncodedSaltedHash(input, hashSettings);
		}

        /// <summary>
        ///     Create the salted hash using the given <see cref="HashSettings"/>.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="hashSettings"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
	    public static string CreateEncodedSaltedHash(string input, HashSettings hashSettings)
	    {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException( nameof( input ) );
            }
            if (hashSettings == null)
            {
                throw new ArgumentNullException( nameof( hashSettings ) );
            }

            // Create the salt data
	        byte[] salt = GetRandomBytes(hashSettings.SaltBytesCount);

	        // Hash the input using the specified salt and settings.
	        byte[] hash = CreateSaltedHash(input, salt, hashSettings.IterationsCount, hashSettings.HashBytesCount);

	        // Encode the hash as a string
	        return EncodeSaltedHash(hashSettings.Version, salt, hash);
	    }

	    /// <summary>
		///     Creates the salted hash.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="salt">The salt bytes.</param>
		/// <param name="iterations">The iterations.</param>
		/// <param name="hashSize">Size of the hash.</param>
		/// <returns></returns>
		private static byte[] CreateSaltedHash( string input, byte[] salt, int iterations, int hashSize )
		{
			var pbkdf2 = new Rfc2898DeriveBytes( input, salt, iterations );
			return pbkdf2.GetBytes( hashSize );
		}


		/// <summary>
		///     Validates the password against the existing hash.
		/// </summary>
		/// <param name="password">The password. This cannot be null.</param>
		/// <param name="encodedHash">The encoded hash. This cannot be empty or null.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="password"/> cannot be null. <paramref name="encodedHash"/> cannot be empty or null.
		/// </exception>
		public static bool ValidatePassword( string password, string encodedHash )
		{
			if ( password == null )
			{
				throw new ArgumentNullException( nameof( password ) );
			}

			if ( string.IsNullOrEmpty( encodedHash ) )
			{
				throw new ArgumentNullException( nameof( encodedHash ) );
			}

            int actualVersion;
            byte[] actualSalt;
            byte[] actualHash;

            bool result = false;

            if (TryDecodeSaltedHash(encodedHash, out actualVersion, out actualSalt, out actualHash))
            {
                HashSettings hashSettings = HashSettings.GetHashSettings(actualVersion);

                byte[] expectedHash = CreateSaltedHash(password, actualSalt, hashSettings.IterationsCount, actualHash.Length);

                result = CompareHashes(expectedHash, actualHash);
            }

			return result;
		}


		/// <summary>
		///     Compares the hashes.
		/// </summary>
		/// <param name="hash1">The hash1.</param>
		/// <param name="hash2">The hash2.</param>
		/// <returns></returns>
		private static bool CompareHashes( byte[] hash1, byte[] hash2 )
		{
			if ( hash1 != null &&
			     hash2 != null )
			{
				if ( hash1.Length != hash2.Length )
				{
					return false;
				}

				return !hash1.Where( ( t, i ) => t != hash2[ i ] ).Any( );
			}

			return false;
		}


		/// <summary>
		///     Gets the random bytes.
		/// </summary>
		/// <param name="size">The size.</param>
		/// <returns></returns>
		public static byte[] GetRandomBytes( int size )
		{
			if ( size <= 0 )
			{
				throw new ArgumentOutOfRangeException( nameof( size ) );
			}

			var bytes = new byte[size];

			var rng = new RNGCryptoServiceProvider( );
			rng.GetBytes( bytes );

			return bytes;
		}


        /// <summary>
        ///     Gets a random string of readable characters a-z|A-Z|0-9.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns>The random string</returns>
        /// <remarks>This can be made considerably faster</remarks>
        public static string GetRandomPrintableString(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException( nameof( size ) );
            }

            StringBuilder coupon = new StringBuilder(size);
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] rnd = new byte[1];
            int n = 0;
            while (n < size)
            {
                rng.GetBytes(rnd);
                char c = (char) ((rnd[0] % (127 - 33)) + 33);
                if (Char.IsDigit(c) || Char.IsLetter(c))
                {
                    ++n;
                    coupon.Append(c);
                }
            }
            return coupon.ToString();
        }
        /// <summary>
        ///     Computes the sha1 hash of the specified file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static string ComputeSha1Hash(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException( nameof( filePath ) );
            }

            using (var source = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ComputeSha1Hash(source);
            }
        }

        /// <summary>
        ///     Computes the sha1 hash of the specified stream.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <returns></returns>
        public static string ComputeSha1Hash(Stream sourceStream)
        {
            if (sourceStream == null)
            {
                throw new ArgumentNullException( nameof( sourceStream ) );
            }

            byte[] hashBytes;

            // Hash the incoming data
            using (SHA1 sha1Hash = new SHA1Managed())
            {
                sourceStream.Position = 0;
                hashBytes = sha1Hash.ComputeHash(sourceStream);
            }

            sourceStream.Position = 0;

            // Convert the hashed data to a hexadecimal string.
            var hashBuilder = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashBuilder.Append(hashBytes[i].ToString("x2").ToUpper());
            }

            return hashBuilder.ToString();
        }


		/// <summary>
		/// Hash the provided enumeration of hashes into a single hash using Berstein cofuzing
		/// </summary>
		/// <param name="objects">The objects.</param>
		/// <returns>
		/// The hashes hashes
		/// </returns>
        public static int HashObjects(IEnumerable<object> objects)
        {
            if (objects == null)
                return NullHash;

            return HashHashes(objects.Select(o => o != null ? o.GetHashCode() : NullHash));
        }

		/// <summary>
		/// Hash the provided enumeration of values a single hash using Berstein cofuzing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="values">The values.</param>
		/// <returns>
		/// The hashes hashes
		/// </returns>
        public static int HashValues<T>(IEnumerable<T> values)
            where T: struct
        {
            if (values == null)
                return NullHash;

            return HashHashes(values.Select(v =>  v.GetHashCode() ));
        }

        /// <summary>
        /// Hash the provided enumeration of hashes into a single hash using Berstein cofuzing
        /// </summary>
        /// <param name="hashList"></param>
        /// <returns>The hashes hashes</returns>
        public static int HashHashes(IEnumerable<int> hashList)
        {
            // cofuzzing with Berstein hash
            unchecked
            {

                int hash = 17;
                foreach (var h in hashList)
                    hash = hash * 31 + h;      

                return hash;
            }
        }


        public class HashSettings
		{
			#region Constants

			/// <summary>
			///     Settings for prerelease
			/// </summary>
            public const int HashVersion10 = 1;
			private const int SaltSizeV10 = 24;
			private const int HashSizeV10 = 24;
			private const int IterationsCountV10 = 1000;

            /// <summary>
            ///     Settings for release
            /// </summary>
            public const int HashVersion11 = 2;
            private const int SaltSizeV11 = 24;
            private const int HashSizeV11 = 24;
            private const int IterationsCountV11 = 20000;

			#endregion

			#region Public Methods

			/// <summary>
			///     Gets the hash settings given a specified version.
			/// </summary>
			/// <param name="version">The version.</param>
			/// <returns></returns>
			/// <exception cref="System.ArgumentOutOfRangeException">version</exception>
			public static HashSettings GetHashSettings( int version = HashVersion10 )
			{
				HashSettings settings;

				switch ( version )
				{
					case HashVersion10:
						settings = new HashSettings
							{
								Version = version,
								SaltBytesCount = SaltSizeV10,
								HashBytesCount = HashSizeV10,
								IterationsCount = IterationsCountV10
							};
						break;

                    case HashVersion11:
                        settings = new HashSettings
                        {
                            Version = version,
                            SaltBytesCount = SaltSizeV11,
                            HashBytesCount = HashSizeV11,
                            IterationsCount = IterationsCountV11
                        };
                        break;

					default:
						settings = null;
						break;
				}

				if ( settings == null )
				{
					throw new ArgumentOutOfRangeException( nameof( version ) );
				}

				return settings;
			}

			#endregion

			#region Properties

			/// <summary>
			///     Gets or sets the hash bytes count.
			/// </summary>
			/// <value>
			///     The hash bytes count.
			/// </value>
			public int HashBytesCount
			{
				get;
				private set;
			}


			/// <summary>
			///     Gets or sets the iterations count.
			/// </summary>
			/// <value>
			///     The iterations count.
			/// </value>
			public int IterationsCount
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets or sets the salt bytes count.
			/// </summary>
			/// <value>
			///     The salt bytes count.
			/// </value>
			public int SaltBytesCount
			{
				get;
				private set;
			}

			/// <summary>
			///     Gets or sets the version.
			/// </summary>
			/// <value>
			///     The version.
			/// </value>
			public int Version
			{
				get;
				private set;
			}

			#endregion Properties
		}
	}
}