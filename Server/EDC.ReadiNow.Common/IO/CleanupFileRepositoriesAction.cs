// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using EDC.IO;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;

namespace EDC.ReadiNow.IO
{
    /// <summary>
    /// Cleanup file repositories action.
    /// This fires every 24 hours.
    /// </summary>
    public class CleanupFileRepositoriesAction : ItemBase
    {        
        /// <summary>
        ///     The batch size to use when checking for unreferenced files.
        /// </summary>
        private readonly int _batchSize;       


        /// <summary>
        ///     The sql to run to get unreferenced tokens.
        /// </summary>
        private const string GetUnreferencedTokensSql =
            @";WITH dataHashRows AS
            (
                SELECT d.Data 
	            FROM 
		            Data_NVarChar d
	            JOIN 
		            Data_Alias fdha ON fdha.TenantId = d.TenantId AND fdha.EntityId = d.FieldId
	            WHERE	
		            fdha.Namespace = N'core' AND 
		            fdha.Data = N'fileDataHash' AND  
		            fdha.AliasMarkerId = 0
            )
            SELECT DISTINCT t.Data 
            FROM 
                #tokensToCheck t
            WHERE 
                NOT EXISTS (
	                SELECT 1 FROM dataHashRows d
	                WHERE d.Data = t.Data
            )

            TRUNCATE TABLE #tokensToCheck";


        /// <summary>
        /// The list of repos to clean.
        /// </summary>
        private readonly List<IFileRepository> _repositoriesToClean = new List<IFileRepository>();


        protected override bool RunAsOwner
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Default constructor. Called by schedule.
        /// </summary>
        public CleanupFileRepositoriesAction()
        {
            _repositoriesToClean.Add(Factory.BinaryFileRepository);
            _repositoriesToClean.Add(Factory.DocumentFileRepository);            
            _batchSize = 1000;            
        }


        /// <summary>
        ///     Constructor used for testing.
        /// </summary>
        /// <param name="repositories"></param>
        /// <param name="batchSize"></param>
        public CleanupFileRepositoriesAction(IEnumerable<IFileRepository> repositories, int batchSize)
        {
            if (repositories == null)
            {
                throw new ArgumentNullException("repositories");
            }           

            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException("batchSize");
            }            

            _repositoriesToClean.AddRange(repositories);
            _batchSize = batchSize;
        }


        /// <summary>
        ///     Gets any unreferenced tokens.
        /// </summary>
        /// <returns></returns>
        private ISet<string> GetUnreferencedTokens()
        {
            ISet<string> unreferencedTokens = new HashSet<string>();

            using (var context = DatabaseContext.GetContext())
            {
                var command = context.CreateCommand();
                command.CommandText = GetUnreferencedTokensSql;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var token = reader.GetString(0);
                        unreferencedTokens.Add(token);
                    }
                }
            }

            return unreferencedTokens;
        }


        /// <summary>
        ///     Deletes the specified files from the specified repository.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="tokensToDelete"></param>
        private ICollection<string> DeleteFilesFromRepository(IFileRepository repository, ICollection<string> tokensToDelete)
        {
            var deletedTokens = new HashSet<string>();

            if (tokensToDelete == null || tokensToDelete.Count == 0)
            {
                return deletedTokens;
            }

            foreach (var token in tokensToDelete)
            {
                try
                {
                    repository.Delete(token);
                    deletedTokens.Add(token);
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteWarning("An error occurred deleting file with token {0} from file repository. Error: {1}", token, ex);
                }
            }

            return deletedTokens;
        }


        /// <summary>
        ///     Bulk insert the specified set into a temp table
        /// </summary>
        /// <param name="tokensToCheck"></param>
        private void BulkCopyTokensToCheck(ICollection<string> tokensToCheck)
        {
            if (tokensToCheck == null || tokensToCheck.Count == 0)
            {
                return;
            }

            var dataTable = new DataTable();
            dataTable.Columns.Add("Data", typeof (string));

            foreach (var token in tokensToCheck)
            {
                var row = dataTable.NewRow();
                row[0] = token;

                dataTable.Rows.Add(row);
            }

            using (var context = DatabaseContext.GetContext())
            {
                var sqlConnection = context.GetUnderlyingConnection() as SqlConnection;

                if (sqlConnection == null)
                {
                    return;
                }

                using (var bulkCopy = new SqlBulkCopy(sqlConnection))
                {
                    bulkCopy.BulkCopyTimeout = 600;
                    bulkCopy.DestinationTableName = "#tokensToCheck";
                    bulkCopy.WriteToServer(dataTable);
                }
            }
        }


        /// <summary>
        ///     Checks for any unreferenced tokens from the specified list and delete them
        ///     from the specified repository.
        /// </summary>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="tokensToCheck">The set of tokens to check.</param>
        private void CheckAndDeleteUnreferencedFilesFromRepository(IFileRepository fileRepository, ICollection<string> tokensToCheck)
        {
            // Bulk copy the tokens to check into a temp table
            BulkCopyTokensToCheck(tokensToCheck);

            // Run a query and get any unreferenced file tokens
            var tokensToDelete = GetUnreferencedTokens();

            // Delete the unreferenced files from the repository
            DeleteFilesFromRepository(fileRepository, tokensToDelete);
        }


        /// <summary>
        ///     Delete any unreferenced files from the specified repository.
        /// </summary>
        /// <param name="fileRepository"></param>
        private void DeleteUnreferencedFilesFromRepository(IFileRepository fileRepository)
        {
            ISet<string> tokensToCheck = new HashSet<string>();

            // Get the tokens in batches
            foreach (var token in fileRepository.GetTokens())
            {
                tokensToCheck.Add(token);

                if (tokensToCheck.Count > _batchSize)
                {
                    CheckAndDeleteUnreferencedFilesFromRepository(fileRepository, tokensToCheck);
                    tokensToCheck.Clear();
                }
            }

            if (tokensToCheck.Count > 0)
            {
                CheckAndDeleteUnreferencedFilesFromRepository(fileRepository, tokensToCheck);
            }
        }

        /// <summary>
        ///     Delete all the unreferenced files in the file repositories.
        /// </summary>
        /// <param name="scheduledItemRef"></param>
        public override void Execute(EntityRef scheduledItemRef)
        {
            EventLog.Application.WriteInformation("Cleaning file repositories");

            using (var context = DatabaseContext.GetContext())
            {
                var command = context.CreateCommand();

                try
                {
                    command.CommandText = @"CREATE TABLE #tokensToCheck ( Data NVARCHAR(200) COLLATE Latin1_General_CI_AI PRIMARY KEY NOT NULL )";
                    command.ExecuteNonQuery();

                    foreach (var fileRepository in _repositoriesToClean)
                    {
                        DeleteUnreferencedFilesFromRepository(fileRepository);
                    }
                }
                finally
                {
                    command.CommandText = @"DROP TABLE #tokensToCheck";
                    command.ExecuteNonQuery();
                }
            }
        }        
    }
}