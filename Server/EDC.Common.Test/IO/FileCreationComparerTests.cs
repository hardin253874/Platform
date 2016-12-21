// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Threading;
using NUnit.Framework;

// ReSharper disable CheckNamespace
namespace EDC.IO.Test
// ReSharper restore CheckNamespace
{
	/// <summary>
	///     This is a test class for the FileCreationComparer type
	/// </summary>
	[TestFixture]
	public class FileCreationComparerTests
	{
		/// <summary>
		///     Tests that the Compare method returns greater than zero if file1 is created after file2.
		/// </summary>
		[Test]
		public void Compare_File1CreatedAfterFile2_ReturnsGreaterThanZero( )
		{
			// Create the first file
			string path2 = Path.GetTempFileName( );

			// Testing the comparer here, not the file system.
			File.SetCreationTime( path2, DateTime.Now.AddMinutes( -1 ) );

			// Create the second file
			string path1 = Path.GetTempFileName( );

			// Initialize the file data
			var file2 = new FileInfo( path2 );
			var file1 = new FileInfo( path1 );

			// Compare the file
			var comparer = new FileCreationComparer( );
			int result = comparer.Compare( file1, file2 );

			// Assert that the file1 was created after file2
			Assert.IsTrue( result > 0 );

			File.Delete( path1 );
			File.Delete( path2 );
		}

		/// <summary>
		///     Tests that the Compare method return less than zero if file1 is created before file2.
		/// </summary>
		[Test]
		public void Compare_File1CreatedBeforeFile2_ReturnsLessThanZero( )
		{
			// Create the first file
			string path1 = Path.GetTempFileName( );

			// Testing the comparer here, not the file system.
			File.SetCreationTime( path1, DateTime.Now.AddMinutes( -1 ) );

			// Create the second file
			string path2 = Path.GetTempFileName( );

			// Initialize the file data
			var file1 = new FileInfo( path1 );
			var file2 = new FileInfo( path2 );

			// Compare the file
			var comparer = new FileCreationComparer( );
			int result = comparer.Compare( file1, file2 );

			// Assert that the file1 was created before file2
			Assert.IsTrue( result < 0 );

			File.Delete( path1 );
			File.Delete( path2 );
		}

		/// <summary>
		///     Tests that the Compare method return zero if file1 is created at the same time as file2.
		/// </summary>
		[Test]
		public void Compare_File1File2CreatedSameTime_ReturnsZero( )
		{
			// Create the file
			string path = Path.GetTempFileName( );

			// Initialize the file data
			var file1 = new FileInfo( path );
			var file2 = new FileInfo( path );

			// Compare the file
			var comparer = new FileCreationComparer( );
			int result = comparer.Compare( file1, file2 );

			// Assert that the file1 was created at the same time as file2
			Assert.IsTrue( result == 0 );

			File.Delete( path );
		}
	}
}