// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Text;
using EDC.IO;
using NUnit.Framework;

namespace EDC.Test.IO
{
	/// <summary>
	///     Unit tests for the CompressionHelper class.
	/// </summary>
	[TestFixture]
	public class CompressionHelperTests
	{
		/// <summary>
		///     Test compressing and decompressing data works.
		/// </summary>
		[Test]
		public void CompressAndDecompressDataTest( )
		{
			const string originalString = @"Felis catus is your taxonomic nomenclature,
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

			byte[ ] originalData = Encoding.UTF8.GetBytes( originalString );

			// Compress the data
			byte[ ] compressedData = CompressionHelper.Compress( originalData );

			Assert.IsTrue( compressedData.Length != originalData.Length, "The compressed data length should not match." );

			// Decompress the data
			byte[ ] originalDecompressedData = CompressionHelper.Decompress( compressedData );

			string originalDecompressedString = Encoding.UTF8.GetString( originalDecompressedData );

			Assert.AreEqual( originalString, originalDecompressedString, "The string data does not match." );
		}

		/// <summary>
		///     Test that compressing null returns null.
		/// </summary>
		[Test]
		public void CompressNullDataTest( )
		{
			Assert.Throws<ArgumentNullException>( ( ) => CompressionHelper.Compress( null ) );
		}

		/// <summary>
		///     Test that decompressing null returns null.
		/// </summary>
		[Test]
		public void DecompressNullDataTest( )
		{
			Assert.Throws<ArgumentNullException>( ( ) => CompressionHelper.Decompress( null ) );
		}
	}
}