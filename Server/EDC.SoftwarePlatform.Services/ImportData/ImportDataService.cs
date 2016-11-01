// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.IO;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.SoftwarePlatform.Services.FileManager;

namespace EDC.SoftwarePlatform.Services.ImportData
{
	public class ImportDataService
	{
		#region Implementation of IImportData

		/// <summary>
		///     Deletes the file from the database.
		/// </summary>
		/// <param name="fileUploadId">The file upload identifier that was created by the file manager service.</param>
		/// <remarks>
		/// </remarks>
		public void DeleteFileFromRepository( string fileUploadId )
		{
            Factory.TemporaryFileRepository.Delete(fileUploadId);
		}

		/// <summary>
		///     Gets the file from the database.
		/// </summary>
		/// <param name="fileUploadId">The file upload identifier that was created by the file manager service.</param>
		/// <returns>An open file stream that represents the file extracted from the database</returns>
		/// <remarks></remarks>
		public Stream GetFileFromRepository( string fileUploadId )
		{		    
		    return FileRepositoryHelper.GetTemporaryFileDataStream(fileUploadId);
		}

		/// <summary>
		///     Inserts the file to the database.
		/// </summary>
		/// <param name="fileUploadId">The file upload identifier that was created by the file manager service.</param>		
		/// <remarks>The file upload ID is used as the primary key for the table that holds the file for the import.</remarks>
		public string InsertFileToRepository( string fileUploadId )
		{
		    string token;
           
			// Write the data to the stream
			string documentFilename = FileManagerService.GetFilenameFromFileId( fileUploadId );
			using ( var source = new FileStream( documentFilename, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
			    token = FileRepositoryHelper.AddTemporaryFile(source);                
            }			
			File.Delete( documentFilename );

		    return token;
		}

		#endregion
	}
}