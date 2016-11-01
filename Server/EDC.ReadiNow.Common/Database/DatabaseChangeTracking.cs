// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Text;
using EDC.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security.AuditLog;

namespace EDC.ReadiNow.Database
{
	/// <summary>
	///     The database change tracking class.
	/// </summary>
	public static class DatabaseChangeTracking
	{
		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="DatabaseChangeTracking" /> is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		public static bool Enabled
		{
			get
			{
				bool enabled = IsTrackingEnabled( );

				return enabled;
			}
			set
			{
				SetEnabledState( value );
			}
		}

		/// <summary>
		/// Creates the restore point.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="userDefined">if set to <c>true</c> [user defined].</param>
		/// <param name="systemUpgrade">if set to <c>true</c> [system upgrade].</param>
		/// <param name="revertTo">The revert to.</param>
		/// <returns></returns>
		public static long CreateRestorePoint( string message, long tenantId = -1, bool userDefined = false, bool systemUpgrade = false, long? revertTo = null )
		{
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( "spCreateRestorePoint", CommandType.StoredProcedure ) )
				{
					if ( tenantId < 0 && RequestContext.IsSet )
					{
						tenantId = RequestContext.TenantId;
					}

					if ( message != null && !message.StartsWith( "u:", StringComparison.OrdinalIgnoreCase ) && RequestContext.IsSet )
					{
						var requestContext = RequestContext.GetContext( );

						if ( requestContext?.Identity != null )
						{
							message = $"u:{requestContext.Identity.Id},{message}";
						}
					}

					if ( tenantId < 0 )
					{
						tenantId = -1;
					}

					ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
					ctx.AddParameter( command, "@context", DbType.AnsiString, message );
					ctx.AddParameter( command, "@userDefined", DbType.Boolean, userDefined );
					ctx.AddParameter( command, "@systemUpgrade", DbType.Boolean, systemUpgrade );
					ctx.AddParameter( command, "@revertTo", DbType.Int64, (object) revertTo ?? DBNull.Value );
					var tranIdParameter = ctx.AddParameter( command, "@tranId", DbType.Int64 );
					tranIdParameter.Direction = ParameterDirection.Output;

					command.ExecuteNonQuery( );

					return ( long ) tranIdParameter.Value;
				}
			}
		}

		/// <summary>
		///     Gets the last transaction identifier.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Failed to retrieve the last transaction id.</exception>
		public static long GetLastTransactionId( long tenantId = -1 )
		{
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				string commandText = "SELECT MAX(TransactionId) FROM Hist_Transaction";

				if ( tenantId >= 0 )
				{
					commandText += " WHERE TenantId = @tenantId";
				}

				using ( IDbCommand command = ctx.CreateCommand( commandText ) )
				{
					if ( tenantId >= 0 )
					{
						ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
					}

					object result = command.ExecuteScalar( );

					if ( result == null || result == DBNull.Value )
					{
						throw new InvalidOperationException( "Failed to retrieve the last transaction id." );
					}

					long transactionId = ( long ) result;

					return transactionId;
				}
			}
		}

		/// <summary>
		/// Gets the transaction identifier.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">Failed to retrieve the last transaction id.</exception>
		public static long GetTransactionId( DateTime date, long tenantId = -1 )
		{
			if ( date < SqlDateTime.MinValue || date > SqlDateTime.MaxValue )
			{
				throw new ArgumentOutOfRangeException( nameof( date ) );
			}

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				string commandText = $"SELECT TOP 1 TransactionId FROM Hist_Transaction WHERE [Timestamp] >= @date {(tenantId >= 0 ? "AND TenantId = @tenantId" : string.Empty)} ORDER BY [Timestamp]";

				using ( IDbCommand command = ctx.CreateCommand( commandText ) )
				{
					ctx.AddParameter( command, "@date", DbType.DateTime, date );

					if ( tenantId >= 0 )
					{
						ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
					}

					object result = command.ExecuteScalar( );

					if ( result == null || result == DBNull.Value )
					{
						throw new InvalidOperationException( "Failed to retrieve the last transaction id." );
					}

					long transactionId = ( long ) result;

					return transactionId;
				}
			}
		}

		/// <summary>
		/// Gets the tenant rollback data.
		/// </summary>
		/// <param name="days">The days.</param>
		/// <returns></returns>
		public static TenantRollbackData GetTenantRollbackData( int days )
		{
			if ( days < 0 )
			{
				days = -days;
			}

			TenantRollbackData result = new TenantRollbackData( );

			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( var command = ctx.CreateCommand( "spGetTenantRollbackData", CommandType.StoredProcedure ) )
				{
					ctx.AddParameter( command, "@days", DbType.Int32, days );
					ctx.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							RestorePoint restorePoint = new RestorePoint( reader );

							result.RestorePoints.Add( restorePoint );
						}

						if ( reader.NextResult( ) )
						{
							while ( reader.Read( ) )
							{
								CustomRestorePoint customRestorePoint = new CustomRestorePoint( reader );

								result.CustomRestorePoints.Add( customRestorePoint );
							}

							if ( reader.NextResult( ) )
							{
								while ( reader.Read( ) )
								{
									RollbackLog rollbackLog = new RollbackLog( reader );

									result.RollbackLogs.Add( rollbackLog );
								}
							}
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		///     Reverts the specified transaction identifier.
		/// </summary>
		/// <param name="transactionId">The transaction identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		public static void Revert( long transactionId, long tenantId = -1 )
		{
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( "spRevert", CommandType.StoredProcedure ) )
				{
					ctx.AddParameter( command, "@transactionId", DbType.Int64, transactionId );

					if ( tenantId >= 0 )
					{
						ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
					}

					command.ExecuteNonQuery( );
				}
			}
		}

		/// <summary>
		///     Reverts the range.
		/// </summary>
		/// <param name="fromTransactionId">From transaction identifier.</param>
		/// <param name="toTransactionId">To transaction identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		public static void RevertRange( long fromTransactionId, long toTransactionId, long tenantId = -1 )
		{
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( "spRevertRange", CommandType.StoredProcedure ) )
				{
					ctx.AddParameter( command, "@fromTransactionId", DbType.Int64, fromTransactionId );
					ctx.AddParameter( command, "@toTransactionId", DbType.Int64, toTransactionId );

					if ( tenantId >= 0 )
					{
						ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
					}

					command.ExecuteNonQuery( );
				}
			}
		}

		/// <summary>
		///     Reverts to.
		/// </summary>
		/// <param name="transactionId">The transaction identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		public static void RevertTo( long transactionId, long? tenantId = null )
		{
			string message = $"Reverting to transaction {transactionId}";

			if ( tenantId >= 0 )
			{
				message += $" for tenant {tenantId}";
			}

			long userId;
			RequestContext.TryGetUserId( out userId );

			using ( DatabaseContextInfo info = new DatabaseContextInfo( message ) )
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( "spRevertTo", CommandType.StoredProcedure ) )
				{
					ctx.AddParameter( command, "@transactionId", DbType.Int64, transactionId );

					if ( tenantId != null )
					{
						ctx.AddParameter( command, "@tenantId", DbType.Int64, tenantId );
					}



					ctx.AddParameter( command, "@context", DbType.AnsiString, DatabaseContextInfo.GetMessageChain( userId ) );

					command.ExecuteNonQuery( );
				}
			}

			AuditLogInstance.Get( ).OnTenantRollback( true, transactionId.ToString( ) );
		}

		/// <summary>
		///     Determines whether [is tracking enabled].
		/// </summary>
		/// <returns>
		///     <c>true</c> if [is tracking enabled]; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="InvalidOperationException">Failed to determine if tracking is enabled.</exception>
		private static bool IsTrackingEnabled( )
		{
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( "SELECT is_disabled FROM sys.triggers WHERE name = @triggerName" ) )
				{
					ctx.AddParameter( command, "@triggerName", DbType.String, Triggers.EntityTrigger );

					object result = command.ExecuteScalar( );

					if ( result == null || result == DBNull.Value )
					{
						throw new InvalidOperationException( "Failed to determine if tracking is enabled." );
					}

					bool isDisabled = ( bool ) result;

					return !isDisabled;
				}
			}
		}

		/// <summary>
		///     Sets the state of the enabled.
		/// </summary>
		/// <param name="value">if set to <c>true</c> [value].</param>
		private static void SetEnabledState( bool value )
		{
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				StringBuilder commandText = new StringBuilder( );

				foreach ( string trigger in Triggers.AllTriggers )
				{
					commandText.AppendLine( $"{( value ? "ENABLE" : "DISABLE" )} TRIGGER {trigger} ON {trigger.Replace( "trg", string.Empty )};" );
				}

				using ( IDbCommand command = ctx.CreateCommand( commandText.ToString( ) ) )
				{
					command.ExecuteNonQuery( );
				}
			}
		}

		/// <summary>
		///     The triggers class.
		/// </summary>
		private static class Triggers
		{
			/// <summary>
			///     The entity trigger
			/// </summary>
			public static readonly string EntityTrigger = "trgEntity";

			/// <summary>
			///     The relationship trigger
			/// </summary>
			private static readonly string RelationshipTrigger = "trgRelationship";

			/// <summary>
			///     The data alias trigger
			/// </summary>
			private static readonly string Data_AliasTrigger = "trgData_Alias";

			/// <summary>
			///     The data bit trigger
			/// </summary>
			private static readonly string Data_BitTrigger = "trgData_Bit";

			/// <summary>
			///     The data date time trigger
			/// </summary>
			private static readonly string Data_DateTimeTrigger = "trgData_DateTime";

			/// <summary>
			///     The data decimal trigger
			/// </summary>
			private static readonly string Data_DecimalTrigger = "trgData_Decimal";

			/// <summary>
			///     The data unique identifier trigger
			/// </summary>
			private static readonly string Data_GuidTrigger = "trgData_Guid";

			/// <summary>
			///     The data int trigger
			/// </summary>
			private static readonly string Data_IntTrigger = "trgData_Int";

			/// <summary>
			///     The data nVarChar trigger
			/// </summary>
			private static readonly string Data_NVarCharTrigger = "trgData_NVarChar";

			/// <summary>
			///     The data XML trigger
			/// </summary>
			private static readonly string Data_XmlTrigger = "trgData_Xml";

			/// <summary>
			///     All triggers
			/// </summary>
			public static readonly IEnumerable<string> AllTriggers = new List<string>
			{
				EntityTrigger,
				RelationshipTrigger,
				Data_AliasTrigger,
				Data_BitTrigger,
				Data_DateTimeTrigger,
				Data_DecimalTrigger,
				Data_GuidTrigger,
				Data_IntTrigger,
				Data_NVarCharTrigger,
				Data_XmlTrigger
			};
		}
	}
}