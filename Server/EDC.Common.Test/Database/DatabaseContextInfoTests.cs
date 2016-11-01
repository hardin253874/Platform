// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Database;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.Test.Database
{
	/// <summary>
	///     The database context information tests class.
	/// </summary>
	[ReadiNowTestFixture]
	public class DatabaseContextInfoTests
	{
		/// <summary>
		///     The test string 1
		/// </summary>
		public static readonly string TestString1 = "The quick brown fox jumps over the lazy dog"; // 43 characters

		/// <summary>
		///     The test string 2
		/// </summary>
		public static readonly string TestString2 = "The rain in spain stays mainly on the plain"; // 43 characters

		/// <summary>
		///     The maximum length
		/// </summary>
		public static readonly int MaxLength = 128;

		/// <summary>
		///     Tests the empty scenario.
		/// </summary>
		[Test]
		public void TestEmpty( )
		{
			using ( DatabaseContextInfo.SetContextInfo( string.Empty ) )
			{
				/////
				// Use CallerMemberName
				/////
				string message = DatabaseContextInfo.GetMessageChain( 0 );

				Assert.AreEqual( "u:0,TestEmpty", message );
			}
		}


		/// <summary>
		///     Tests the message contains multiple values with multiple center values dropped.
		/// </summary>
		[Test]
		public void TestMessageContainsMultipleValuesWithMultipleCenterValuesDropped( )
		{
			using ( DatabaseContextInfo.SetContextInfo( "aa" ) )
			{
				using ( DatabaseContextInfo.SetContextInfo( "bb" ) )
				{
					using ( DatabaseContextInfo.SetContextInfo( "cc" ) )
					{
						using ( DatabaseContextInfo.SetContextInfo( "dd" ) )
						{
							using ( DatabaseContextInfo.SetContextInfo( "ee" ) )
							{
								using ( DatabaseContextInfo.SetContextInfo( "ff" ) )
								{
									string message = DatabaseContextInfo.GetMessageChain( 0, "->", 20 );

									Assert.AreEqual( "u:0,aa->bb->...->ff", message );
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Tests the message contains multiple values with no values dropped.
		/// </summary>
		[Test]
		public void TestMessageContainsMultipleValuesWithNoValuesDropped( )
		{
			using ( DatabaseContextInfo.SetContextInfo( "One" ) )
			{
				using ( DatabaseContextInfo.SetContextInfo( "Two" ) )
				{
					using ( DatabaseContextInfo.SetContextInfo( "Three" ) )
					{
						using ( DatabaseContextInfo.SetContextInfo( "Four" ) )
						{
							using ( DatabaseContextInfo.SetContextInfo( "Five" ) )
							{
								string message = DatabaseContextInfo.GetMessageChain( 0 );

								Assert.AreEqual( "u:0,One->Two->Three->Four->Five", message );
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Tests the message contains multiple values with the center value dropped and both the first and last values
		///     truncated.
		/// </summary>
		[Test]
		public void TestMessageContainsMultipleValuesWithTheCenterValueDroppedAndBothTheFirstAndLastValuesTruncated( )
		{
			using ( DatabaseContextInfo.SetContextInfo( "Hello" ) )
			{
				using ( DatabaseContextInfo.SetContextInfo( "There" ) )
				{
					using ( DatabaseContextInfo.SetContextInfo( "World" ) )
					{
						string message = DatabaseContextInfo.GetMessageChain( 0, "->", 20 );

						Assert.AreEqual( "u:0,H...->...->W...", message );
					}
				}
			}
		}

		/// <summary>
		///     Tests the message contains multiple values with the center value dropped and the first value truncated.
		/// </summary>
		[Test]
		public void TestMessageContainsMultipleValuesWithTheCenterValueDroppedAndTheFirstValueTruncated( )
		{
			using ( DatabaseContextInfo.SetContextInfo( "HelloWorld" ) )
			{
				using ( DatabaseContextInfo.SetContextInfo( "There" ) )
				{
					using ( DatabaseContextInfo.SetContextInfo( "Hi" ) )
					{
						string message = DatabaseContextInfo.GetMessageChain( 0, "->", 20 );

						Assert.AreEqual( "u:0,Hell...->...->Hi", message );
					}
				}
			}
		}

		/// <summary>
		///     Tests the message contains multiple values with the center value dropped and the last value truncated.
		/// </summary>
		[Test]
		public void TestMessageContainsMultipleValuesWithTheCenterValueDroppedAndTheLastValueTruncated( )
		{
			using ( DatabaseContextInfo.SetContextInfo( "Hi" ) )
			{
				using ( DatabaseContextInfo.SetContextInfo( "There" ) )
				{
					using ( DatabaseContextInfo.SetContextInfo( "HelloWorld" ) )
					{
						string message = DatabaseContextInfo.GetMessageChain( 0, "->", 20 );

						Assert.AreEqual( "u:0,Hi->...->Hell...", message );
					}
				}
			}
		}

		/// <summary>
		///     Tests the message contains two values with the first value truncated.
		/// </summary>
		[Test]
		public void TestMessageContainsTwoValuesWithTheFirstValueTruncated( )
		{
			using ( DatabaseContextInfo.SetContextInfo( "HelloThereWorld" ) )
			{
				using ( DatabaseContextInfo.SetContextInfo( "Hi" ) )
				{
					string message = DatabaseContextInfo.GetMessageChain( 0, "->", 20 );

					Assert.AreEqual( "u:0,HelloTher...->Hi", message );
				}
			}
		}

		/// <summary>
		///     Tests the message contains two values with the last value truncated.
		/// </summary>
		[Test]
		public void TestMessageContainsTwoValuesWithTheLastValueTruncated( )
		{
			using ( DatabaseContextInfo.SetContextInfo( "Hi" ) )
			{
				using ( DatabaseContextInfo.SetContextInfo( "HelloThereWorld" ) )
				{
					string message = DatabaseContextInfo.GetMessageChain( 0, "->", 20 );

					Assert.AreEqual( "u:0,Hi->HelloTher...", message );
				}
			}
		}

		/// <summary>
		///     Tests the message with multiple values that equal the maximum length.
		/// </summary>
		[Test]
		public void TestMessageWithMultipleValuesThatEqualTheMaximumLength( )
		{
			using ( DatabaseContextInfo.SetContextInfo( TestString1.PadRight( MaxLength / 2, '!' ) ) )
			{
				using ( DatabaseContextInfo.SetContextInfo( TestString2.PadRight( MaxLength / 2, '@' ) ) )
				{
					string message = DatabaseContextInfo.GetMessageChain( 0 );

					Assert.AreEqual( "u:0,The quick brown fox jumps over the lazy dog!!!!!!!!!!!!!!...->The rain in spain stays mainly on the plain@@@@@@@@@@@@@@...", message );
				}
			}
		}

		/// <summary>
		///     Tests the message with multiple values that exceed the maximum length.
		/// </summary>
		[Test]
		public void TestMessageWithMultipleValuesThatExceedTheMaximumLength( )
		{
			using ( DatabaseContextInfo.SetContextInfo( TestString1.PadRight( MaxLength / 2 + 1, '!' ) ) )
			{
				using ( DatabaseContextInfo.SetContextInfo( TestString2.PadRight( MaxLength / 2 + 1, '@' ) ) )
				{
					string message = DatabaseContextInfo.GetMessageChain( 0 );

					Assert.AreEqual( "u:0,The quick brown fox jumps over the lazy dog!!!!!!!!!!!!!!...->The rain in spain stays mainly on the plain@@@@@@@@@@@@@@...", message );
				}
			}
		}

		/// <summary>
		///     Tests the message with multiple values that fall below the maximum length.
		/// </summary>
		[Test]
		public void TestMessageWithMultipleValuesThatFallBelowTheMaximumLength( )
		{
			using ( DatabaseContextInfo.SetContextInfo( TestString1 ) )
			{
				using ( DatabaseContextInfo.SetContextInfo( TestString2 ) )
				{
					string message = DatabaseContextInfo.GetMessageChain( 0 );

					Assert.AreEqual( $"u:0,{TestString1}->{TestString2}", message );
				}
			}
		}

		/// <summary>
		///     Tests the message with single value that equals the maximum length.
		/// </summary>
		[Test]
		public void TestMessageWithSingleValueThatEqualsTheMaximumLength( )
		{
			using ( DatabaseContextInfo.SetContextInfo( TestString1.PadRight( MaxLength - 4, '!' ) ) )
			{
				string message = DatabaseContextInfo.GetMessageChain( 0 );

				Assert.AreEqual( $"u:0,{TestString1.PadRight( MaxLength - 4, '!' )}", message );
			}
		}

		/// <summary>
		///     Tests the message with a single value that exceed the maximum length.
		/// </summary>
		[Test]
		public void TestMessageWithSingleValueThatExceedTheMaximumLength( )
		{
			using ( DatabaseContextInfo.SetContextInfo( TestString1.PadRight( MaxLength -3, '!' ) ) )
			{
				string message = DatabaseContextInfo.GetMessageChain( 0 );

				Assert.AreEqual( "u:0,The quick brown fox jumps over the lazy dog!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!...", message );
			}
		}

		/// <summary>
		///     Tests the message with single value that falls below the maximum length.
		/// </summary>
		[Test]
		public void TestMessageWithSingleValueThatFallsBelowTheMaximumLength( )
		{
			using ( DatabaseContextInfo.SetContextInfo( TestString1 ) )
			{
				string message = DatabaseContextInfo.GetMessageChain( 0 );

				Assert.AreEqual( $"u:0,{TestString1}", message );
			}
		}

		/// <summary>
		///     Tests the null scenario.
		/// </summary>
		[Test]
		public void TestNull( )
		{
			/////
			// Use stack trace
			/////
			string message = DatabaseContextInfo.GetMessageChain( 0 );

			Assert.AreEqual( "u:0,DatabaseContextInfoTests.TestNull", message );
		}
	}
}