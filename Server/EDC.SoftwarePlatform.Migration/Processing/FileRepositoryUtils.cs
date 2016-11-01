// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using EDC.IO;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     File repository utils
	/// </summary>
	internal static class FileRepositoryUtils
	{
		/// <summary>
		///     Load file data with the given hash from the given repository.
		/// </summary>
		/// <param name="fileRepository"></param>
		/// <param name="token"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		internal static byte[ ] LoadFileData( IFileRepository fileRepository, string token, IProcessingContext context )
		{
			byte[ ] data;

			if ( string.IsNullOrWhiteSpace( token ) )
			{
				return null;
			}

			using ( var stream = new MemoryStream( ) )
			{
				try
				{
					using ( var sourceStream = fileRepository.Get( token ) )
					{
						sourceStream.CopyTo( stream );
					}
					data = stream.ToArray( );
				}
				catch ( FileNotFoundException )
				{
					context.WriteWarning( $"File not found. (Hash: {token})" );
					data = null;
				}
				catch ( Exception ex )
				{
					context.WriteWarning( $"An error occurred getting the file from file repository. DataHash: {token}. Error {ex}." );
					data = null;
				}
			}

			return data;
		}
	}
}