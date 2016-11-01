// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using EDC.IO;
using EDC.ReadiNow.Core;
using EDC.Security;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;

namespace EDC.SoftwarePlatform.Install.Common
{
	/// <summary>
	///     Database class.
	/// </summary>
	public class Database
	{
		private const string FullTextCatalog = "Data_NVarChar_Catalog";
		private const string CheckDbConnectionString = @"Server=tcp:{0};Integrated security=SSPI;database=master";
		private const string MasterConnectionString = @"Server=tcp:localhost;Integrated security=SSPI;database=master";
		private const string PlatformConnectionString = @"Server=tcp:{0};Integrated security=SSPI;database={1}";

	    private const string DropTableSql =
@"IF (EXISTS (SELECT 1 FROM 
              INFORMATION_SCHEMA.TABLES 
              WHERE TABLE_SCHEMA = 'dbo' AND 
                    TABLE_NAME = '{0}'))
BEGIN
    DROP TABLE [dbo].[{0}]
END";

        private const string DropProcSql =
@"IF (EXISTS (SELECT 1 FROM 
              INFORMATION_SCHEMA.ROUTINES 
              WHERE ROUTINE_SCHEMA = 'dbo' AND 
                    ROUTINE_TYPE = 'PROCEDURE' AND
                    ROUTINE_NAME = '{0}'))
BEGIN
    DROP PROC [dbo].[{0}]
END";

        private const string DropFullTextCatalogSql =
@"IF (EXISTS (SELECT 1 FROM 
              sys.fulltext_catalogs
              WHERE name = '{0}'))
BEGIN
    DROP FULLTEXT CATALOG [{0}]
END";

        private const string RemoveFileSql =
@"IF (EXISTS (SELECT 1 FROM 
        sys.database_files
        WHERE name = '{1}'))
BEGIN
    ALTER DATABASE [{0}] REMOVE FILE [{1}]
END";

        private const string RemoveFileGroupSql =
@"IF (EXISTS (SELECT 1 FROM 
        sys.filegroups
        WHERE name = '{1}'))
BEGIN
    ALTER DATABASE [{0}] REMOVE FILEGROUP [{1}]
END";

        private const string SelectSqlFileStreamFileGuids =
@"IF (EXISTS (SELECT 1 FROM 
              INFORMATION_SCHEMA.TABLES 
              WHERE TABLE_SCHEMA = 'dbo' AND 
                    TABLE_NAME = '{0}'))
BEGIN        
    SELECT FileGuid FROM [dbo].[{0}]
END";

        private const string SelectSqlFileStreamFile =
@"SELECT 
    Data.PathName( 2 ),
    GET_FILESTREAM_TRANSACTION_CONTEXT( ), 
    DataHash, 
    FileExtension 
FROM 
    [dbo].[{0}]
WHERE
    FileGuid = @fileGuid";

        private const string DatabaseFileExistsSql =
@"SELECT 1 FROM 
        sys.database_files
        WHERE name = '{0}'";

        private const string AppLibraryUpgradeFileDataHashesAndFileExtensions = @"
DECLARE @fileDataHashFieldId UNIQUEIDENTIFIER
DECLARE @fileExtensionFieldId UNIQUEIDENTIFIER

SET @fileDataHashFieldId = '256D76BC-B5E4-4AD7-A765-5DDCABE1121F'
SET @fileExtensionFieldId = '2EA3CC77-2BDC-479E-B55B-81F5A7B565BE'

UPDATE
	d
SET
	d.Data = b.NewDataHash
FROM
	AppData_NVarChar d
    JOIN #AppLibaryMigratedFiles b ON b.OldDataHash = d.Data
WHERE	
	d.FieldUid = @fileDataHashFieldId AND
	d.Data <> b.NewDataHash AND
    b.NewDataHash IS NOT NULL

INSERT INTO AppData_NVarChar (AppVerUid, EntityUid, FieldUid, Data)
SELECT 
    DISTINCT dhash.AppVerUid, dhash.EntityUid, @fileExtensionFieldId, b.FileExtension
FROM 
    #AppLibaryMigratedFiles b
    JOIN AppData_NVarChar dhash ON dhash.Data = b.NewDataHash
    LEFT JOIN AppData_NVarChar dext ON dext.AppVerUid = dhash.AppVerUid AND dext.EntityUid = dhash.EntityUid 
WHERE    
    dhash.FieldUid = @fileDataHashFieldId AND    
    dext.FieldUid = @fileExtensionFieldId AND
    dext.EntityUid IS NULL AND
    b.FileExtension IS NOT NULL

UPDATE	
    dext
SET
	dext.Data = b.FileExtension
FROM
	#AppLibaryMigratedFiles b
    JOIN AppData_NVarChar dhash ON dhash.Data = b.NewDataHash
    JOIN AppData_NVarChar dext ON dext.EntityUid = dhash.EntityUid AND dext.AppVerUid = dhash.AppVerUid
WHERE    
    dhash.FieldUid = @fileDataHashFieldId AND    
    dext.FieldUid = @fileExtensionFieldId AND
    dext.Data <> b.FileExtension AND
    b.FileExtension IS NOT NULL
";

	    private const string TenantUpgradeFileDataHashesAndFileExtensions = @"
UPDATE
	d
SET
	d.Data = b.NewDataHash
FROM
	Data_NVarChar d
	JOIN Data_Alias fdha ON fdha.TenantId = d.TenantId AND fdha.EntityId = d.FieldId
    JOIN #MigratedFiles b ON b.OldDataHash = d.Data
WHERE	
	fdha.Namespace = 'core' AND 
	fdha.Data = 'fileDataHash' AND  
	fdha.AliasMarkerId = 0 AND	
	d.Data <> b.NewDataHash AND
    b.NewDataHash IS NOT NULL

INSERT INTO Data_NVarChar (TenantId, EntityId, FieldId, Data)
SELECT 
    DISTINCT dhash.TenantId, dhash.EntityId, fea.EntityId, b.FileExtension
FROM 
    #MigratedFiles b
    JOIN Data_NVarChar dhash ON dhash.Data = b.NewDataHash
	JOIN Data_Alias fdha ON fdha.TenantId = dhash.TenantId AND fdha.EntityId = dhash.FieldId
    LEFT JOIN Data_NVarChar dext ON dext.TenantId = dhash.TenantId AND dext.EntityId = dhash.EntityId 
	JOIN Data_Alias fea ON fea.TenantId = dext.TenantId AND fea.EntityId = dext.FieldId
WHERE    
	fdha.Namespace = 'core' AND 
	fdha.Data = 'fileDataHash' AND  
	fdha.AliasMarkerId = 0 AND	
	fea.Namespace = 'core' AND 
	fea.Data = 'fileExtension' AND  
	fea.AliasMarkerId = 0 AND	    
    dext.EntityId IS NULL AND
    b.FileExtension IS NOT NULL

UPDATE	
    dext
SET
	dext.Data = b.FileExtension
FROM
	#MigratedFiles b
    JOIN Data_NVarChar dhash ON dhash.Data = b.NewDataHash
	JOIN Data_Alias fdha ON fdha.TenantId = dhash.TenantId AND fdha.EntityId = dhash.FieldId
    JOIN Data_NVarChar dext ON dext.TenantId = dhash.TenantId AND dext.EntityId = dhash.EntityId
	JOIN Data_Alias fea ON fea.TenantId = dext.TenantId AND fea.EntityId = dext.FieldId
WHERE    
    fdha.Namespace = 'core' AND 
	fdha.Data = 'fileDataHash' AND  
	fdha.AliasMarkerId = 0 AND	
	fea.Namespace = 'core' AND 
	fea.Data = 'fileExtension' AND  
	fea.AliasMarkerId = 0 AND	 
    dext.Data <> b.FileExtension AND
    b.FileExtension IS NOT NULL
";        

        private const string WaitForSqlFileStreamGCSql = @"CHECKPOINT
EXEC sp_filestream_force_garbage_collection";

		/// <summary>
		///     Adds the database user.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <param name="role">The role.</param>
		/// <param name="server">The server.</param>
		/// <param name="catalog">The catalog.</param>
		/// <param name="dbUser">The database user.</param>
		/// <param name="dbPassword">The database password.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void AddDatabaseUser( string username, string role, string server, string catalog, string dbUser, string dbPassword )
		{
			if ( string.IsNullOrEmpty( username ) || string.IsNullOrEmpty( server ) || string.IsNullOrEmpty( catalog ) )
			{
				throw new ArgumentNullException( );
			}

			bool impersonate = false;

			var credential = new NetworkCredential( );

			if ( !string.IsNullOrEmpty( dbUser ) )
			{
				credential = CredentialHelper.ConvertToNetworkCredential( dbUser, dbPassword );

				/////
				// Check if the context identity matches the current windows identity
				/////
				WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent( );

				var principal = new WindowsPrincipal( windowsIdentity );

				string account = ( ( WindowsIdentity ) principal.Identity ).Name;

				if ( String.Compare( CredentialHelper.GetFullyQualifiedName( credential ), account, StringComparison.OrdinalIgnoreCase ) != 0 )
				{
					impersonate = true;
				}
			}

			ImpersonationContext impersonationContext = null;

			/////
			// Format up the connection string
			/////
			string connectionString = GetDatabaseConnectionString( server );

			try
			{
				if ( impersonate )
				{
					impersonationContext = ImpersonationContext.GetContext( credential );
				}

				using ( var platformDbConnection = new SqlConnection( connectionString ) )
				{
					platformDbConnection.Open( );

					using ( SqlCommand sqlCommand = platformDbConnection.CreateCommand( ) )
					{
						/////
						// If specific user exists then delete it
						/////
						sqlCommand.CommandText = string.Format( @"DECLARE @login NVARCHAR(MAX) = NULL; SELECT @login = name FROM sys.server_principals WHERE LOWER(name) = LOWER(N'{0}'); IF (@login IS NULL) CREATE LOGIN [{0}] FROM WINDOWS; GRANT VIEW SERVER STATE TO [{0}]", username );
						sqlCommand.ExecuteNonQuery( );
					}

					platformDbConnection.Close( );
				}

				/////
				// Connect to the platform database and add in the new user to the database role.
				/////
				connectionString = $@"Server=tcp:{server};Integrated security=SSPI;database={catalog}";

				using ( var platformDbConnection = new SqlConnection( connectionString ) )
				{
					platformDbConnection.Open( );

					using ( SqlCommand sqlCommand = platformDbConnection.CreateCommand( ) )
					{
						// If specific user exists then delete it
						sqlCommand.CommandText = string.Format( @"DECLARE @user NVARCHAR(MAX) = NULL; SELECT @user = name FROM sys.database_principals WHERE LOWER(name) = LOWER(N'{0}'); IF (@user IS NOT NULL) EXEC ('ALTER USER [' + @user + '] WITH LOGIN = [{0}]') ELSE CREATE USER [{0}] FOR LOGIN [{0}]", username );
						sqlCommand.ExecuteNonQuery( );

						/////
						// Assign the role for the user
						/////
						sqlCommand.CommandText = string.Format( @"exec sp_addrolemember N'{1}', N'{0}'", username, role );
						sqlCommand.ExecuteNonQuery( );
					}
					platformDbConnection.Close( );
				}
			}
			finally
			{
				impersonationContext?.Dispose( );
			}
		}


		public static bool DatabaseExists( string databaseName, string serverName = "" )
		{
			int databaseCount = 0;
			using ( var checkDbConnection = new SqlConnection( string.Format( CheckDbConnectionString, string.IsNullOrEmpty( serverName ) ? @"localhost" : serverName ) ) )
			{
				checkDbConnection.Open( );
				using ( SqlCommand sqlCommand = checkDbConnection.CreateCommand( ) )
				{
					sqlCommand.CommandText = string.Format( "SELECT database_id FROM sys.databases WHERE Name = '{0}'", databaseName );
					object result = sqlCommand.ExecuteScalar( );
					if ( result != null )
					{
						databaseCount = ( int ) result;
					}
				}
				checkDbConnection.Close( );
			}
			return databaseCount > 0;
		}

		/// <summary>
		///     Rebuilds the full text catalog.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="catalog">The catalog.</param>
		/// <param name="waitForCompletion">if set to <c>true</c> [wait for completion].</param>
		public static void RebuildFullTextCatalog( string server, string catalog, bool waitForCompletion = true )
		{
			using ( var platformDbConnection = new SqlConnection( GetDatabaseConnectionString( server, catalog ) ) )
			{
				platformDbConnection.Open( );

				using ( SqlCommand sqlCommand = platformDbConnection.CreateCommand( ) )
				{
					string command = @"
					ALTER FULLTEXT CATALOG {0}
					REBUILD WITH ACCENT_SENSITIVITY = ON";

					if ( waitForCompletion )
					{
						command += @"
						DECLARE @status int
						DECLARE @maxCount int = 0

						SELECT @status = FULLTEXTCATALOGPROPERTY('{0}', 'PopulateStatus')
						WHILE ( @status <> 0 )
						BEGIN
							IF @maxCount > 20
							BEGIN
								RETURN
							END

							SET @maxCount = @maxCount + 1

							WAITFOR DELAY '00:00:01'
							SELECT @status = FULLTEXTCATALOGPROPERTY('{0}', 'PopulateStatus')
						END";
					}

					sqlCommand.CommandText = string.Format( command, "Data_NVarChar_Catalog" );
					sqlCommand.ExecuteNonQuery( );
				}
			}
        }

        /// <summary>
        ///     Previously, installation used the config xml bootstrap mechanism, which resulted in an incorrect packageId for core solutions
        ///     being stored in the Global tenant. This means that subsequent upgrades don't run correctly.
        ///     This probably only needs to be here for a few versions.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="catalog">The database.</param>
        public static void RepairPackageIdInGlobal( string server, string catalog )
        {
            string query = @"
declare @coreSolution uniqueidentifier = '7062aade-2e72-4a71-a7fa-a412d20d6f01'
declare @packageId uniqueidentifier = '1670eb43-6568-4e41-97c4-71b2bd1fba61'
declare @consoleSolution uniqueidentifier = '34ff4d95-70c6-4ae8-8f6f-38d88546d4c4'
declare @coreSolutionIncorrect uniqueidentifier = 'd14fbd50-52b8-43b8-9388-dba8574c0280'
declare @consoleSolutionIncorrect uniqueidentifier = '8aeb3b0b-21ce-49f3-96ca-90103e62eea6'

update gp
set Data = 
(
	select top 1 Data from Data_Guid tp
	join Entity tf on tf.Id = tp.FieldId and tf.UpgradeId = @packageId and tp.TenantId = tf.TenantId
	join Entity ts on ts.Id = tp.EntityId and ts.UpgradeId = @coreSolution and tp.TenantId = ts.TenantId
	where tp.TenantId <> 0
)
from Data_Guid gp
join Entity gf on gf.Id = gp.FieldId and gf.UpgradeId = @packageId and gf.TenantId = 0
join Entity gs on gs.Id = gp.EntityId and gs.UpgradeId = @coreSolution and gs.TenantId = 0
where gp.TenantId = 0 and gp.Data = @coreSolutionIncorrect
;
update gp
set Data = 
(
	select top 1 Data from Data_Guid tp
	join Entity tf on tf.Id = tp.FieldId and tf.UpgradeId = @packageId and tp.TenantId = tf.TenantId
	join Entity ts on ts.Id = tp.EntityId and ts.UpgradeId = @consoleSolution and tp.TenantId = ts.TenantId
	where tp.TenantId <> 0
)
from Data_Guid gp
join Entity gf on gf.Id = gp.FieldId and gf.UpgradeId = @packageId and gf.TenantId = 0
join Entity gs on gs.Id = gp.EntityId and gs.UpgradeId = @consoleSolution and gs.TenantId = 0
where gp.TenantId = 0 and gp.Data = @consoleSolutionIncorrect";

            using ( var conn = new SqlConnection( GetDatabaseConnectionString( server, catalog ) ) )
            {
                conn.Open( );
                using ( SqlCommand sqlCommand = conn.CreateCommand( ) )
                {
                    sqlCommand.CommandText = query;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.ExecuteScalar( );
                }
            }
        }

        /// <summary>
        ///     There are some bad duplicate resource key entries in the global tenant.
        ///     Remove them.
        ///     This probably only needs to be here for a few versions.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="catalog">The database.</param>
        public static void RepairResourceKeyHashesInGlobal( string server, string catalog )
        {
            string query = @"
declare @tenantId bigint = 0
declare @dataHash bigint = dbo.fnAliasNsId('dataHash','core', @tenantId)
declare @relId bigint = dbo.fnAliasNsId('resourceKeyDataHashAppliesToResourceKey','core', @tenantId)
declare @keyId bigint = dbo.fnAliasNsId('definitionUniqueNameKey','core', @tenantId)

create table #tmp (
  Id bigint
)
insert into #tmp (Id)
select e.Id
from
	Data_NVarChar n
join Entity e on
	-- Skip the guids for the keys that we want to keep (namely the ones in coreData)
	e.TenantId = @tenantId and e.Id = n.EntityId and e.UpgradeId not in ('715a37be-7aa5-4afd-bb1e-0d071f0d75d9', '2fc32cd0-af5b-4761-bd42-0bf33a94c008')
join Relationship r on
	r.TenantId = @tenantId and r.FromId = e.Id and r.TypeId = @relId and r.ToId = @keyId
where
	n.TenantId = @tenantId and n.FieldId = @dataHash
	-- Match based on the hashes we want to nuke
	and n.Data in ('F40E710BDABCDDE48927029B4DE274E0DDCBF7A8','F63A4849805C121F443BC8E8EE7D21D65BAF5625')

delete from Data_NVarChar where TenantId = @tenantId and EntityId in ( select Id from #tmp )
delete from Data_DateTime where TenantId = @tenantId and EntityId in ( select Id from #tmp )
delete from Relationship where TenantId = @tenantId and FromId in ( select Id from #tmp )
delete from Relationship where TenantId = @tenantId and ToId in ( select Id from #tmp )
delete from Entity where TenantId = @tenantId and Id in ( select Id from #tmp )

drop table #tmp";

            using ( var conn = new SqlConnection( GetDatabaseConnectionString( server, catalog ) ) )
            {
                conn.Open( );
                using ( SqlCommand sqlCommand = conn.CreateCommand( ) )
                {
                    sqlCommand.CommandText = query;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.ExecuteScalar( );
                }
            }
        }

        /// <summary>
        ///     Creates the database role.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="catalog">The database.</param>
        /// <param name="roleName">Name of the role.</param>
        public static void CreateDatabaseRole( string server, string catalog, string roleName )
		{
			using ( var platformDbConnection = new SqlConnection( GetDatabaseConnectionString( server, catalog ) ) )
			{
				platformDbConnection.Open( );

				using ( SqlCommand sqlCommand = platformDbConnection.CreateCommand( ) )
				{
					/////
					// Create the role
					/////
					sqlCommand.CommandText = string.Format( @"DECLARE @role NVARCHAR(MAX) = NULL; SELECT @role = name FROM sys.database_principals WHERE LOWER(name) = LOWER(N'{0}'); IF (@role IS NULL) CREATE ROLE [{0}]", roleName );
					sqlCommand.ExecuteNonQuery( );

					/////
					// Assign permissions for this role
					/////
					sqlCommand.CommandText = $@"grant execute,select,insert,update,delete to {roleName}";
					sqlCommand.ExecuteNonQuery( );

					/////
					// Assign permissions for crypto
					/////
					sqlCommand.CommandText = $@"GRANT CONTROL ON CERTIFICATE::cert_keyProtection TO {roleName}";
					sqlCommand.ExecuteNonQuery( );

					/////
					// Assign permissions for crypto
					/////
					sqlCommand.CommandText = $@"GRANT VIEW DEFINITION ON SYMMETRIC KEY::key_Secured TO {roleName}";
					sqlCommand.ExecuteNonQuery( );
				}
				platformDbConnection.Close( );
			}
		}

		/// <summary>
		/// FileInfo Class.
		/// </summary>
		private class FileInfo
	    {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="newDataHash"></param>
            /// <param name="extension"></param>
	        public FileInfo(string newDataHash, string extension)
	        {
	            NewDataHash = newDataHash;
	            Extension = extension;
	        }

            public string NewDataHash { get; private set; }
            public string Extension { get; private set; }
        }

        /// <summary>
        /// Copies files from the specified source filestream table to the specified file repository.
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="sourceTable"></param>
        /// <param name="targetRepository"></param>
        /// <param name="migratedFiles"></param>
	    private static void CopySqlFileStreamFilesToRepository(SqlConnection dbConnection, string sourceTable, IFileRepository targetRepository, Dictionary<string, FileInfo> migratedFiles)
	    {
            ISet<Guid> fileGuids = new HashSet<Guid>();

            // Get all the file guids first
            using (SqlCommand sqlCommand = dbConnection.CreateCommand())
            {
                sqlCommand.CommandText = string.Format(SelectSqlFileStreamFileGuids, sourceTable);
                sqlCommand.CommandType = CommandType.Text;

                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        fileGuids.Add(reader.GetGuid(0));
                    }
                }
            }

            // Get each file in a transaction.            
	        foreach (Guid fileGuid in fileGuids)
	        {
	            using (SqlTransaction transaction = dbConnection.BeginTransaction())
	            {
                    using (SqlCommand sqlCommand = dbConnection.CreateCommand())
                    {
                        sqlCommand.Transaction = transaction;
                        sqlCommand.CommandText = string.Format(SelectSqlFileStreamFile, sourceTable);
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.Parameters.AddWithValue("@fileGuid", fileGuid);

                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string path = reader.GetString(0);
                                byte[] fsContext = reader.GetSqlBinary(1).Value;
                                string dataHash = reader.GetString(2);
                                string extension = reader.GetString(3);

                                using (var sqlFileStream = new ImpersonationSqlFileStream(path, fsContext, FileAccess.Read))
                                {                                    
                                    string newToken = targetRepository.Put(sqlFileStream.Stream);

                                    migratedFiles[dataHash] = new FileInfo(newToken, extension);
                                }
                            }
                        }
                    }

                    transaction.Commit();
                }
	        }	        
	    }


        /// <summary>
        /// Bulk copy migrated files info into the specified table.
        /// </summary>
        /// <param name="dbConnection"></param>        
        /// <param name="files"></param>
        /// <param name="targetTable"></param>
        /// <param name="logger"></param>
	    private static void BulkCopyMigratedFileInfo(SqlConnection dbConnection, Dictionary<string, FileInfo> files, string targetTable, Action<string> logger)
	    {
	        if (files.Count == 0)
	        {
	            return;
	        }

            using (var bulkCopy = new SqlBulkCopy(dbConnection))
            {
                bulkCopy.BulkCopyTimeout = 600;                             

                DataTable table = new DataTable(targetTable);
                table.Columns.Add("OldDataHash", typeof(string));
                table.Columns.Add("NewDataHash", typeof(string));
                table.Columns.Add("FileExtension", typeof(string));

                foreach (var kvp in files)
                {
                    DataRow row = table.NewRow();

                    row[0] = kvp.Key;
                    row[1] = kvp.Value.NewDataHash;
                    row[2] = kvp.Value.Extension;

                    table.Rows.Add(row);
                }

                bulkCopy.DestinationTableName = targetTable;
                bulkCopy.WriteToServer(table);                

                logger(string.Format("Loaded files into table {0}.", targetTable));
            }
        }

        /// <summary>
        /// Upgrades the file stream files.
        /// </summary>        
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <param name="logger"></param>
	    internal static void UpgradeSqlFileStream(string server, string database, Action<string> logger)
	    {
	        if (!DatabaseExists(database, server))
	        {
                logger("The database does not exist. Skipping file stream file upgrade.");
                // The database does not exist, no files to upgrade.
                return;
	        }

	        var migratedFiles = new Dictionary<string, FileInfo>();
            var migratedAppLibraryFiles = new Dictionary<string, FileInfo>();

            string connection = string.Format(PlatformConnectionString, server, database);

            using (var dbConnection = new SqlConnection(connection))
            {
                dbConnection.Open();                

                // Create temp tables
                using (SqlCommand sqlCommand = dbConnection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.Text;

                    sqlCommand.CommandText = string.Format(DatabaseFileExistsSql, "AppFile_Indexed_Data");
                    var result = sqlCommand.ExecuteScalar();
                    if (result == null)
                    {
                        return;
                    }

                    sqlCommand.CommandText = "CREATE TABLE #MigratedFiles ( OldDataHash NVARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL, NewDataHash NVARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL, FileExtension NVARCHAR(20) COLLATE Latin1_General_CI_AI NULL )";
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = "CREATE TABLE #AppLibaryMigratedFiles ( OldDataHash NVARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL, NewDataHash NVARCHAR(MAX) COLLATE Latin1_General_CI_AI NULL, FileExtension NVARCHAR(20) COLLATE Latin1_General_CI_AI NULL )";
                    sqlCommand.ExecuteNonQuery();
                }

                // Copy all the files to their appropriate file repository
                logger("Copying File_NonIndexed files...");
                CopySqlFileStreamFilesToRepository(dbConnection, "File_NonIndexed", Factory.BinaryFileRepository, migratedFiles);

                logger("Copying File_Indexed files...");
                CopySqlFileStreamFilesToRepository(dbConnection, "File_Indexed", Factory.DocumentFileRepository, migratedFiles);

                logger("Copying AppFile_Indexed files...");
                CopySqlFileStreamFilesToRepository(dbConnection, "AppFile_Indexed", Factory.AppLibraryFileRepository, migratedAppLibraryFiles);

                logger("Copying AppFile_NonIndexed files...");
                CopySqlFileStreamFilesToRepository(dbConnection, "AppFile_NonIndexed", Factory.AppLibraryFileRepository, migratedAppLibraryFiles);                

                logger("Updating entity datahashes and file extensions...");

                // Bulk load migrated file info                    
                BulkCopyMigratedFileInfo(dbConnection, migratedFiles, "#MigratedFiles", logger);
                BulkCopyMigratedFileInfo(dbConnection, migratedAppLibraryFiles, "#AppLibaryMigratedFiles", logger);

                // Do as much as we can in the transaction
                using (var transaction = dbConnection.BeginTransaction())
                {
                    // Update App library and Tenant data hashes and file extensions
                    using (SqlCommand sqlCommand = dbConnection.CreateCommand())
                    {
                        sqlCommand.Transaction = transaction;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandTimeout = 600;

                        sqlCommand.CommandText = AppLibraryUpgradeFileDataHashesAndFileExtensions;
                        sqlCommand.ExecuteNonQuery();

                        sqlCommand.CommandText = TenantUpgradeFileDataHashesAndFileExtensions;
                        sqlCommand.ExecuteNonQuery();                        
                    }

                    // Drop objects
                    DropSqlFileStreamTables(dbConnection, transaction, logger);
                    DropSqlFileStreamProcs(dbConnection, transaction, logger);                    

                    transaction.Commit();
                }

                // Drop temp tables
                using (SqlCommand sqlCommand = dbConnection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.Text;

                    sqlCommand.CommandText = @"DROP TABLE #MigratedFiles";
                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.CommandText = @"DROP TABLE #AppLibaryMigratedFiles";
                    sqlCommand.ExecuteNonQuery();                    
                }                

                DropSqlFileStreamCatalog(dbConnection, logger);
                DropSqlFileStreamFilesAndFileGroups(dbConnection, database, logger);
                // TODO - C.C Uncomment this bit
                // Do not disable filestream for now. Is causing issues on build boxes.
                //DisableSqlFileStream(dbConnection, logger);                
            }	    
	    }       

	    /// <summary>
        /// Drop tables that referenced filestream.
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="transaction"></param>
        /// <param name="logger"></param>
	    private static void DropSqlFileStreamTables(SqlConnection dbConnection, SqlTransaction transaction, Action<string> logger)
	    {
            string[] tablesToDelete = { "AppFile_Indexed", "AppFile_NonIndexed", "File_Indexed", "File_NonIndexed", "File_Temporary" };

            foreach (string tableToDelete in tablesToDelete)
            {
                logger(string.Format("Dropping table {0}", tableToDelete));

                using (SqlCommand sqlCommand = dbConnection.CreateCommand())
                {
                    sqlCommand.Transaction = transaction;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = string.Format(DropTableSql, tableToDelete);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Drop procs that referenced filestream.
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="transaction"></param>
        /// <param name="logger"></param>
        private static void DropSqlFileStreamProcs(SqlConnection dbConnection, SqlTransaction transaction, Action<string> logger)
        {
            string[] procsToDelete = { "spDeleteImportFile", "spIndexedFileDelete", "spIndexedFileSelect", "spMoveIndexedFile", "spMoveNonIndexedFile", "spNonIndexedFileDelete", "spNonIndexedFileSelect", "spTemporaryFileDelete", "spTemporaryFileInsert", "spTemporaryFileSelect", "spTemporaryFileSelectExtension" };

            foreach (string procToDelete in procsToDelete)
            {
                logger(string.Format("Dropping proc {0}", procToDelete));

                using (SqlCommand sqlCommand = dbConnection.CreateCommand())
                {
                    sqlCommand.Transaction = transaction;
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = string.Format(DropProcSql, procToDelete);
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Drop catalogs that referenced filestream
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="logger"></param>
	    private static void DropSqlFileStreamCatalog(SqlConnection dbConnection, Action<string> logger)
	    {
            using (SqlCommand sqlCommand = dbConnection.CreateCommand())
            {
                logger("Dropping catalog File_Indexed_Catalog");
                
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = string.Format(DropFullTextCatalogSql, "File_Indexed_Catalog");
                sqlCommand.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Waits for the sql filestream GC to complete.
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="logger"></param>
	    private static void WaitForSqlFileStreamGC(SqlConnection dbConnection, Action<string> logger)
	    {
	        DateTime start = DateTime.Now;

	        logger("Waiting for SqlFileStream GC");

            while (true)
	        {
                using (SqlCommand sqlCommand = dbConnection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = WaitForSqlFileStreamGCSql;

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        bool done = true;

                        while (reader.Read())
                        {
                            int ordCollected = reader.GetOrdinal("num_collected_items");
                            int ordMarked = reader.GetOrdinal("num_marked_for_collection_items");
                            int ordUnprocessed = reader.GetOrdinal("num_unprocessed_items");

                            int numCollected = reader.IsDBNull(ordCollected) ? 0 : reader.GetInt32(ordCollected);
                            int numMarked = reader.IsDBNull(ordMarked) ? 0 : reader.GetInt32(ordMarked);
                            int numUnprocessed = reader.IsDBNull(ordUnprocessed) ? 0 : reader.GetInt32(ordUnprocessed);

                            if (numCollected != 0 ||
                                numMarked != 0 ||
                                numUnprocessed != 0)
                            {
                                done = false;
                                break;
                            }
                        }

                        if (done)
                        {
                            logger("SqlFileStream GC has completed");
                            break;
                        }
                    }
                }

	            TimeSpan diff = DateTime.Now - start;
	            if (diff.TotalSeconds > 30)
	            {
                    logger(string.Format("SqlFileStream GC has exceeded timeout. Waited for {0} seconds, continuing upgrade.", diff.TotalSeconds));

                    break;
	            }

                Thread.Sleep(500);
	        }
	    }


        /// <summary>
        /// Drop files and file groups that reference filestream
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="database"></param>
        /// <param name="logger"></param>
        private static void DropSqlFileStreamFilesAndFileGroups(SqlConnection dbConnection, string database, Action<string> logger)
        {
            WaitForSqlFileStreamGC(dbConnection, logger);

            string[] filesToDelete = { "AppFile_NonIndexed_Data", "File_Indexed_Data", "File_NonIndexed_Data", "File_Temporary_Data", "AppFile_Indexed_Data" };

            foreach (string fileToDelete in filesToDelete)
            {
                logger(string.Format("Dropping file {0}", fileToDelete));

                using (SqlCommand sqlCommand = dbConnection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.Text;                    

                    sqlCommand.CommandText = string.Format(RemoveFileSql, database, fileToDelete);
                    sqlCommand.ExecuteNonQuery();
                }
            }

            foreach (string fileToDelete in filesToDelete)
            {
                logger(string.Format("Dropping file group {0}", fileToDelete));

                using (SqlCommand sqlCommand = dbConnection.CreateCommand())
                {
                    sqlCommand.CommandType = CommandType.Text;                    

                    sqlCommand.CommandText = string.Format(RemoveFileGroupSql, database, fileToDelete);
                    sqlCommand.ExecuteNonQuery();
                }
            }                            
	    }

        /// <summary>
        /// Disables the file stream for this database
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="logger"></param>
        private static void DisableSqlFileStream(SqlConnection dbConnection, Action<string> logger)
        {
            logger("Disabling sql filestream");

            using (SqlCommand sqlCommand = dbConnection.CreateCommand())
            {
                sqlCommand.CommandType = CommandType.Text;

                sqlCommand.CommandText = @"EXEC sp_configure filestream_access_level, 0";
                sqlCommand.ExecuteNonQuery();

                sqlCommand.CommandText = @"RECONFIGURE";
                sqlCommand.ExecuteNonQuery();
            }
        }

		/// <summary>
		///     Globals the tenant exists.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="catalog">The catalog.</param>
		/// <returns></returns>
		public static bool GlobalTenantExists( string server, string catalog )
		{
			using ( var platformDbConnection = new SqlConnection( GetDatabaseConnectionString( server, catalog ) ) )
			{
				platformDbConnection.Open( );

				using ( SqlCommand sqlCommand = platformDbConnection.CreateCommand( ) )
				{
					string commandText = "SELECT 1 FROM Data_Alias WHERE TenantId = 0 AND Data = 'name' AND Namespace = 'core' AND AliasMarkerId = 0";

					sqlCommand.CommandText = commandText;

					var result = sqlCommand.ExecuteScalar( );

					if ( result != null && result != DBNull.Value )
					{
						return ( int ) result == 1;
					}
				}
			}

			return false;
        }

        /// <summary>
        ///     Generates a scheme number for a new database.
        ///     This allows us to recognise a given installation.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="catalog">The catalog.</param>
        /// <returns></returns>
        public static bool SetPlatformInstallInfo( string server, string catalog )
        {
            using ( var platformDbConnection = new SqlConnection( GetDatabaseConnectionString( server, catalog ) ) )
            {
                platformDbConnection.Open( );

                string sql = "insert into PlatformInfo (InstallDate, BookmarkScheme) values (getdate(), convert(int, rand() * 1000000))";

                using ( SqlCommand command = platformDbConnection.CreateCommand( ) )
                {
                    command.CommandText = sql;

                    command.ExecuteNonQuery( );
                }
            }

            return false;
        }

        /// <summary>
        ///     Tenants the exists.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="catalog">The catalog.</param>
        /// <param name="tenantName">Name of the tenant.</param>
        /// <returns></returns>
        public static bool TenantExists( string server, string catalog, string tenantName )
		{
			using ( var platformDbConnection = new SqlConnection( GetDatabaseConnectionString( server, catalog ) ) )
			{
				platformDbConnection.Open( );

				using ( SqlCommand sqlCommand = platformDbConnection.CreateCommand( ) )
				{
					string commandText = "SELECT 1 FROM _vTenant WHERE name = @name";

					sqlCommand.CommandText = commandText;

					var nameParameter = sqlCommand.CreateParameter( );
					nameParameter.DbType = DbType.String;
					nameParameter.Direction = ParameterDirection.Input;
					nameParameter.ParameterName = "@name";
					nameParameter.Value = tenantName;

					sqlCommand.Parameters.Add( nameParameter );

					var result = sqlCommand.ExecuteScalar( );

					if ( result != null && result != DBNull.Value )
					{
						return ( int ) result == 1;
					}
				}
			}

			return false;
		}

		/// <summary>
		///     Gets the database connection string.
		/// </summary>
		/// <param name="server">The server.</param>
		/// <param name="catalog">The catalog.</param>
		/// <returns></returns>
		private static string GetDatabaseConnectionString( string server, string catalog = "master" )
		{
			return $@"Server=tcp:{server};Integrated security=SSPI;database={catalog}";
		}

		/// <summary>
		/// Creates the tenant.
		/// </summary>
		/// <param name="tenantName">Name of the tenant.</param>
		public static void CreateTenant( string tenantName )
		{
			TenantHelper.CreateTenant( tenantName );
		}
	}
}