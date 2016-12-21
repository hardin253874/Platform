// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using System.Threading;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.Xml;
using EDC.ReadiNow.Messaging;

namespace EDC.SoftwarePlatform.Services.LongRunningTask
{
	/// <summary>
	///     Provides helpers methods to read and save the ImportTask from the database.
	/// </summary>
	public class LongRunningHelper
	{
		/// <summary>
		///     Delete long running task from database.
		/// </summary>
		public static void DeleteTaskInfo( Guid taskId )
		{
			using ( DatabaseContext databaseContext = DatabaseContext.GetContext( ) )
			{
				// Create and initialize the command object

				using ( IDbCommand command = databaseContext.CreateCommand( "spDeleteLongRunningTask", CommandType.StoredProcedure ) )
				{
					databaseContext.AddParameter( command, "@taskId", DbType.Guid, taskId );

					command.ExecuteNonQuery( );
				}
			}
		}

        /// <summary>
        ///     Get import task from database.
        /// </summary>
        /// <param name="taskId">The task identifier.</param>
        /// <param name="progressOnly">True to get the progress info only, false to get full details from the task xml.</param>        
        public static LongRunningInfo GetTaskInfo( Guid taskId, bool progressOnly )
		{
			var importResult = new LongRunningInfo( );
			using ( DatabaseContext databaseContext = DatabaseContext.GetContext( ) )
			{
				// Create and initialize the command object
				using ( IDbCommand command = databaseContext.CreateCommand( "spGetLongRunningTask", CommandType.StoredProcedure ) )
				{
					databaseContext.AddParameter( command, "@taskId", DbType.Guid, taskId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						if ( reader.Read( ) )
						{                            
                            LongRunningInfo info = null;                            

                            if (progressOnly)
                            {
                                // If the task is in progress, just return the task id
                                // and the status without deserializing the data contract.

                                LongRunningStatus status;
                                string statusAsString = reader.GetString(1);

                                if (!string.IsNullOrWhiteSpace(statusAsString) &&
                                    Enum.TryParse(statusAsString, out status) &&
                                    status == LongRunningStatus.InProgress)
                                {                                    
                                    info = new LongRunningInfo()
                                    {
                                        TaskId = taskId,
                                        Status = status
                                    };                                    
                                }
                            }                                                        
                            
                            if (info == null)
                            {
                                string xml = reader.GetString(2);
                                info = XmlHelper.DeserializeUsingDataContract<LongRunningInfo>(xml);
                            }
                            
                            return info;
						}
					}
				}
			}

			return importResult;
		}


		/// <summary>
		///     Create/Save long running task in the database.
		/// </summary>
		/// <param name="info">The info.</param>
		public static void SaveLongRunningTaskInfo( LongRunningInfo info )
		{
			string infoXml = XmlHelper.SerializeUsingDataContract( info );

			using ( DatabaseContext databaseContext = DatabaseContext.GetContext( ) )
			{
				// Create and initialize the command object
				using ( IDbCommand command = databaseContext.CreateCommand( "spSaveLongRunningTask", CommandType.StoredProcedure ) )
				{
					databaseContext.AddParameter( command, "@taskId", DbType.Guid, info.TaskId );
					databaseContext.AddParameter( command, "@status", DbType.String, info.Status );
					databaseContext.AddParameter( command, "@additionalInfo", DbType.String, infoXml );

					command.ExecuteNonQuery( );
				}
			}
		}

		/// <summary>
		///     Starts a new
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public static LongRunningInfo StartLongRunningInWorkerThread( Action<LongRunningInfo> action )
		{
			var requestContext = RequestContext.GetContext( );

			var info = new LongRunningInfo
			{
				Status = LongRunningStatus.InProgress,
				TaskId = Guid.NewGuid( )
			};

			SaveLongRunningTaskInfo( info );

            var thread = new Thread(StartWorker) { Name = "LongRunningHelper", IsBackground = true };
			thread.Start( new Tuple<Action<LongRunningInfo>, LongRunningInfo, RequestContext>( action, info, requestContext ) );

			return info;
		}

		/// <summary>
		///     Starts the worker.
		/// </summary>
		private static void StartWorker( object startInfo )
		{
			var tuple = ( Tuple<Action<LongRunningInfo>, LongRunningInfo, RequestContext> ) startInfo;
			var action = tuple.Item1;
			var info = tuple.Item2;
			var context = tuple.Item3;

			try
			{                
                using (new DeferredChannelMessageContext())
                {
                    RequestContext.SetContext(context);

                    action(info);
                    UpdateStatus(info.TaskId, LongRunningStatus.Success);
                }				
			}
			catch ( Exception ex )
			{
				EventLog.Application.WriteError( ex.ToString( ) );
				UpdateStatus( info.TaskId, LongRunningStatus.Failed, ex.Message );
			}
		}


		/// <summary>
		///     Update long running task status.
		/// </summary>
		public static void UpdateStatus( Guid taskId, LongRunningStatus status, string errorMessage = null )
		{
			LongRunningInfo info = GetTaskInfo( taskId, false );
			if ( !string.IsNullOrEmpty( errorMessage ) )
			{
				info.ErrorMessage = errorMessage;
			}
			if ( info.Status != LongRunningStatus.Cancelled )
			{
				info.Status = status;
				SaveLongRunningTaskInfo( info );
			}
		}
	}
}