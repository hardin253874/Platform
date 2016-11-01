// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.IO;

namespace EDC.IO
{
	/// <summary>
	///     Exposes a method to compare the creation dates of two files.
	/// </summary>
	public class FileCreationComparer : IComparer
	{
		/// <summary>
		///     Compares two files and returns a value indicating whether one was created before the other, whether
		///     both were created at the same time or whether one was created after the other.
		/// </summary>
		/// <param name="file1">
		///     The first object to compare.
		/// </param>
		/// <param name="file2">
		///     The second object to compare.
		/// </param>
		/// <returns>
		///     Less than zero if file1 is created before file2; Zero if file1 and file2 were created as the same
		///     time; Greater than zero if file1 is created after file2.
		/// </returns>
		public int Compare( object file1, object file2 )
		{
			if ( ( file1 == null ) && ( file2 != null ) )
			{
				return -1;
			}

			if ( ( file1 != null ) && ( file2 == null ) )
			{
				return 1;
			}

			if ( ( file1 == null ) )
			{
				return 0;
			}

			if ( !( file1 is FileInfo ) )
			{
				throw new ArgumentException( "The specified file1 parameter is invalid" );
			}
			var fileInfo1 = ( FileInfo ) file1;

			if ( !( file2 is FileInfo ) )
			{
				throw new ArgumentException( "The specified file2 parameter is invalid" );
			}
			var fileInfo2 = ( FileInfo ) file2;

			// Compare the creation times
			if ( fileInfo1.CreationTime < fileInfo2.CreationTime )
			{
				return -1;
			}

			if ( fileInfo1.CreationTime > fileInfo2.CreationTime )
			{
				return 1;
			}

			return 0;
		}
	}
}