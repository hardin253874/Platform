// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Database
{
	/// <summary>
	///     The database performance tests class.
	/// </summary>
	[ReadiNowTestFixture]
	[Category( "ExtendedTests" )]
	public class DatabasePerformanceTests
	{
		/// <summary>
		///     Tests the trigger impact.
		/// </summary>
		[Test]
        public void TestTriggerImpact( )
		{
			int runCount = 1;
			int threadCount = 5;
			int threadIterationCount = 100;

			List<RunResult> results = new List<RunResult>( );

			for ( int run = 0; run < runCount; run++ )
			{
				RunResult result = RunComparison( threadCount, threadIterationCount );

				results.Add( result );
			}

			double averageNonTriggerTime = results.Average( r => r.NonTriggerTime.Ticks );
			double averageTriggerTime = results.Average( r => r.TriggerTime.Ticks );

			if ( averageTriggerTime > averageNonTriggerTime )
			{
				double difference = ( averageTriggerTime - averageNonTriggerTime ) / averageTriggerTime * 100;

				Assert.LessOrEqual( difference, 50 );

				Console.WriteLine( difference );
			}
		}

		/// <summary>
		///     Creates the data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="value">The value.</param>
		/// <param name="data">The data.</param>
		/// <param name="additionalColumns">The additional columns.</param>
		/// <param name="additionalValues">The additional values.</param>
		private void CreateData( DatabaseContext context, string tableName, string value, Dictionary<string, Queue<Tuple<long, long>>> data, string additionalColumns = null, string additionalValues = null )
		{
			using ( IDbCommand command = context.CreateCommand( $@"
INSERT INTO Entity (TenantId, UpgradeId) VALUES ( @tenantId, NEWID() )
DECLARE @entityId BIGINT = SCOPE_IDENTITY( )
INSERT INTO Entity (TenantId, UpgradeId) VALUES ( @tenantId, NEWID() )
DECLARE @fieldId BIGINT = SCOPE_IDENTITY( )
INSERT INTO Data_{tableName} (EntityId, TenantId, FieldId, Data{additionalColumns}) VALUES ( @entityId, @tenantId, @fieldId, {value}{additionalValues} )
SELECT @entityId, @fieldId" ) )
			{
				context.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					if ( reader.Read( ) )
					{
						Queue<Tuple<long, long>> queue;

						if ( !data.TryGetValue( tableName, out queue ) )
						{
							queue = new Queue<Tuple<long, long>>( );
							data[ tableName ] = queue;
						}

						queue.Enqueue( new Tuple<long, long>( reader.GetInt64( 0 ), reader.GetInt64( 1 ) ) );
					}
				}
			}
		}

		/// <summary>
		///     Creates the entity.
		/// </summary>
		/// <param name="context">The context.</param>
		private void CreateEntity( DatabaseContext context )
		{
			using ( IDbCommand command = context.CreateCommand( "INSERT INTO Entity ( TenantId, UpgradeId ) VALUES ( @tenantId, NEWID() )" ) )
			{
				context.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

				command.ExecuteNonQuery( );
			}
		}

		/// <summary>
		///     Creates the relationship.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="relationship">The relationship.</param>
		private void CreateRelationship( DatabaseContext context, Queue<Tuple<long, long, long>> relationship )
		{
			using ( IDbCommand command = context.CreateCommand( @"
INSERT INTO Entity (TenantId, UpgradeId) VALUES ( @tenantId, NEWID() )
DECLARE @typeId BIGINT = SCOPE_IDENTITY( )
INSERT INTO Entity (TenantId, UpgradeId) VALUES ( @tenantId, NEWID() )
DECLARE @fromId BIGINT = SCOPE_IDENTITY( )
INSERT INTO Entity (TenantId, UpgradeId) VALUES ( @tenantId, NEWID() )
DECLARE @toId BIGINT = SCOPE_IDENTITY( )
INSERT INTO Relationship (TenantId, TypeId, FromId, ToId) VALUES ( @tenantId, @typeId, @fromId, @toId )
SELECT @typeId, @fromId, @toId" ) )
			{
				context.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

				using ( IDataReader reader = command.ExecuteReader( ) )
				{
					if ( reader.Read( ) )
					{
						relationship.Enqueue( new Tuple<long, long, long>( reader.GetInt64( 0 ), reader.GetInt64( 1 ), reader.GetInt64( 2 ) ) );
					}
				}
			}
		}

		/// <summary>
		///     Deletes the data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="data">The data.</param>
		private void DeleteData( DatabaseContext context, string tableName, Dictionary<string, Queue<Tuple<long, long>>> data )
		{
			Queue<Tuple<long, long>> queue;

			if ( data.TryGetValue( tableName, out queue ) )
			{
				if ( queue.Count > 0 )
				{
					Tuple<long, long> value = queue.Dequeue( );

					using ( IDbCommand command = context.CreateCommand( $"DELETE FROM Data_{tableName} WHERE TenantId = @tenantId AND EntityId = {value.Item1} AND FieldId = {value.Item2}" ) )
					{
						context.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

						command.ExecuteNonQuery( );
					}
				}
			}
		}

		/// <summary>
		///     Deletes the relationship.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="relationship">The relationship.</param>
		private void DeleteRelationship( DatabaseContext context, Queue<Tuple<long, long, long>> relationship )
		{
			if ( relationship.Count > 0 )
			{
				Tuple<long, long, long> value = relationship.Dequeue( );

				using ( IDbCommand command = context.CreateCommand( $"DELETE FROM Relationship WHERE TenantId = @tenantId AND TypeId = {value.Item1} AND FromId = {value.Item2} AND ToId = {value.Item3}" ) )
				{
					context.AddParameter( command, "@tenantId", DbType.Int64, RequestContext.TenantId );

					command.ExecuteNonQuery( );
				}
			}
		}

		/// <summary>
		///     Generates the random load.
		/// </summary>
		/// <param name="state">The state.</param>
		private void GenerateRandomLoad( object state )
		{
			int iterationCount = ( int ) state;

			Queue<Tuple<long, long, long>> relationship = new Queue<Tuple<long, long, long>>( );

			Dictionary<string, Queue<Tuple<long, long>>> data = new Dictionary<string, Queue<Tuple<long, long>>>( );

			Random random = new Random( Guid.NewGuid( ).GetHashCode( ) );

			using ( TenantAdministratorContext.SetContext( RunAsDefaultTenant.DefaultTenantName ) )
			using ( DatabaseContext context = DatabaseContext.GetContext( true ) )
			{
				for ( int iteration = 0; iteration < iterationCount; iteration++ )
				{
					int val = random.Next( 0, 29 );

					/////
					// Double the likelyhood of inserts. (Cheap way)
					/////
					switch ( val )
					{
						case 0:
						case 1:
							CreateEntity( context );
							break;
						case 2:
						case 3:
							CreateRelationship( context, relationship );
							break;
						case 4:
							DeleteRelationship( context, relationship );
							break;
						case 5:
						case 6:
							CreateData( context, "Alias", "'optimus'", data, ", Namespace, AliasMarkerId", ", 'core', 0" );
							break;
						case 7:
							DeleteData( context, "Alias", data );
							break;
						case 8:
						case 9:
							CreateData( context, "Bit", "0", data );
							break;
						case 10:
							DeleteData( context, "Bit", data );
							break;
						case 11:
						case 12:
							CreateData( context, "DateTime", "'2000-01-01'", data );
							break;
						case 13:
							DeleteData( context, "DateTime", data );
							break;
						case 14:
						case 15:
							CreateData( context, "Decimal", "123", data );
							break;
						case 16:
							DeleteData( context, "Decimal", data );
							break;
						case 17:
						case 18:
							CreateData( context, "Guid", $"'{Guid.NewGuid( )}'", data );
							break;
						case 19:
							DeleteData( context, "Guid", data );
							break;
						case 20:
						case 21:
							CreateData( context, "Int", "0", data );
							break;
						case 22:
							DeleteData( context, "Int, data", data );
							break;
						case 23:
						case 24:
							CreateData( context, "NVarChar", "'Optimus Prime'", data );
							break;
						case 25:
							DeleteData( context, "NVarChar", data );
							break;
						case 26:
						case 27:
							CreateData( context, "Xml", "'<a></a>'", data );
							break;
						case 28:
							DeleteData( context, "Xml", data );
							break;
					}
				}
			}
		}

		/// <summary>
		///     Runs the comparison.
		/// </summary>
		/// <param name="threadCount">The thread count.</param>
		/// <param name="threadIterationCount">The thread iteration count.</param>
		/// <returns></returns>
		private RunResult RunComparison( int threadCount, int threadIterationCount )
		{
			bool enabled = DatabaseChangeTracking.Enabled;

			try
			{
				DatabaseChangeTracking.Enabled = false;

				TimeSpan nonTriggerTime = RunTimedTest( threadCount, threadIterationCount );

				DatabaseChangeTracking.Enabled = true;

				TimeSpan triggerTime = RunTimedTest( threadCount, threadIterationCount );

				return new RunResult( nonTriggerTime, triggerTime );
			}
			finally
			{
				DatabaseChangeTracking.Enabled = enabled;
			}
		}

		/// <summary>
		///     Runs the timed test.
		/// </summary>
		/// <param name="threadCount">The thread count.</param>
		/// <param name="threadIterationCount">The thread iteration count.</param>
		/// <returns></returns>
		private TimeSpan RunTimedTest( int threadCount, int threadIterationCount )
		{
			Thread[ ] threads = new Thread[threadCount];

			for ( int i = 0; i < threadCount; i++ )
			{
				threads[ i ] = new Thread( GenerateRandomLoad )
				{
					IsBackground = true,
					Name = "Test Trigger Impact thread"
				};
			}

			long startTime = Environment.TickCount;

			for ( int i = 0; i < threadCount; i++ )
			{
				threads[ i ].Start( threadIterationCount );
			}

			for ( int i = 0; i < threadCount; i++ )
			{
				threads[ i ].Join( );
			}

			long endTime = Environment.TickCount;

			return new TimeSpan( endTime - startTime );
		}

		/// <summary>
		///     The run result class.
		/// </summary>
		private class RunResult
		{
			/// <summary>
			///     Initializes a new instance of the <see cref="RunResult" /> class.
			/// </summary>
			/// <param name="nonTriggerTime">The non trigger time.</param>
			/// <param name="triggerTime">The trigger time.</param>
			public RunResult( TimeSpan nonTriggerTime, TimeSpan triggerTime )
			{
				NonTriggerTime = nonTriggerTime;
				TriggerTime = triggerTime;
			}

			/// <summary>
			///     Gets or sets the non trigger time.
			/// </summary>
			/// <value>
			///     The non trigger time.
			/// </value>
			public TimeSpan NonTriggerTime
			{
				get;
			}

			/// <summary>
			///     Gets or sets the trigger time.
			/// </summary>
			/// <value>
			///     The trigger time.
			/// </value>
			public TimeSpan TriggerTime
			{
				get;
			}
		}
	}
}