// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Transactions;
using EDC.Database;
using EDC.ReadiNow.Database;
using NUnit.Framework;
using System.Data.SqlClient;

namespace EDC.ReadiNow.Test.Database
{
	/// <summary>
	///     This class is responsible for testing DatabaseContext.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class DatabaseContextTests
	{
		/// <summary>
		///     Deletes the specified test string data.
		/// </summary>
		/// <param name="baseId"></param>
		private void DeleteTestStringData( Guid baseId )
		{
			using ( DatabaseContext context = DatabaseContext.GetContext( ) )
			{
				using ( IDbCommand command = context.CreateCommand( @"DELETE FROM Batch WHERE [BatchGuid] = @base" ) )
				{
					context.AddParameter( command, "@base", DatabaseType.GuidType ).Value = baseId;
					command.ExecuteNonQuery( );
				}
			}
		}


		/// <summary>
		///     Adds the specified test string data.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="baseId"></param>
		private void AddTestStringData( DatabaseContext context, Guid baseId )
		{
			using ( IDbCommand command = context.CreateCommand( @"INSERT INTO Batch([BatchGuid])
														 VALUES(@base)" ) )
			{
				context.AddParameter( command, "@base", DatabaseType.GuidType ).Value = baseId;
				command.ExecuteNonQuery( );
			}
		}


		/// <summary>
		///     Verifies the specified test string data.
		/// </summary>
		/// <param name="baseId"></param>
		/// <param name="exists"></param>
		private void VerifyTestStringData( Guid baseId, bool exists )
		{
			using ( DatabaseContext context = DatabaseContext.GetContext( ) )
			{
				VerifyTestStringData( context, baseId, exists );
			}
		}


		/// <summary>
		///     Verifies the specified test string data.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="baseId"></param>
		/// <param name="exists"></param>
		private void VerifyTestStringData( DatabaseContext context, Guid baseId, bool exists )
		{
			using ( IDbCommand command = context.CreateCommand( @"SELECT COUNT(*) FROM Batch
															WHERE [BatchGuid] = @base" ) )
			{
				context.AddParameter( command, "@base", DatabaseType.GuidType ).Value = baseId;

				var count = ( int ) command.ExecuteScalar( );

				if ( exists )
				{
					Assert.AreEqual( 1, count, "The string test data does not exist" );
				}
				else
				{
					Assert.AreEqual( 0, count, "The string test data should not exist" );
				}
			}
		}

		/// <summary>
		///     Test that the GetContext method works.
		/// </summary>
		[Test]
		public void GetContextNoTranTest( )
		{
			using ( DatabaseContext context = DatabaseContext.GetContext( ) )
			{
				var dataSet = new DataSet( );

				IDbDataAdapter adapter = context.CreateDataAdapter( "SELECT TOP 5 Id FROM Entity" );
				adapter.Fill( dataSet );

				Assert.AreEqual( 1, dataSet.Tables.Count, "The number of tables is invalid." );
				Assert.AreEqual( 5, dataSet.Tables[ 0 ].Rows.Count, "The number of rows is invalid." );

				foreach ( DataRow row in dataSet.Tables[ 0 ].Rows )
				{
					Assert.IsTrue( (long)row[ "Id" ] >= 0 );
				}
			}
		}


		/// <summary>
		///     Test that a nested GetContext method works.
		/// </summary>
		[Test]
		public void RootNonTran_NestedNonTranTest( )
		{
			using ( DatabaseContext.GetContext( ) )
			{
				using ( DatabaseContext context2 = DatabaseContext.GetContext( ) )
				{
					var dataSet = new DataSet( );

					IDbDataAdapter adapter = context2.CreateDataAdapter( "SELECT TOP 5 Id FROM Entity" );
					adapter.Fill( dataSet );

					Assert.AreEqual( 1, dataSet.Tables.Count, "The number of tables is invalid." );
					Assert.AreEqual( 5, dataSet.Tables[ 0 ].Rows.Count, "The number of rows is invalid." );

					foreach ( DataRow row in dataSet.Tables[ 0 ].Rows )
					{
						Assert.IsTrue( ( long ) row [ "Id" ] >= 0 );
					}
				}
			}
		}


		/// <summary>
		///     Tests the following scenario:
		///     - Root context non-transaction
		///     - Nested context transaction with commit
		///     Outcome: data should be saved to the database.
		/// </summary>
		[Test]
		public void RootNonTran_NestedTranCommitTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				// Outer is non-transactional
				using ( DatabaseContext contextRoot = DatabaseContext.GetContext( ) )
				{
					// Inner is transactional
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );

						// Commit transaction
						contextTran.CommitTransaction( );
					}

					// Verify that the added data does exist
					VerifyTestStringData( contextRoot, baseId, true );
				}
			}
			finally
			{
				// Cleanup
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context non-transaction
		///     - Nested context transaction with rollback
		///     Outcome: data should not be saved to the database.
		/// </summary>
		[Test]
		public void RootNonTran_NestedTranRollbackTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				// Outer is non-transactional
				using ( DatabaseContext contextRoot = DatabaseContext.GetContext( ) )
				{
					// Inner is transactional
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );

						// Rollback transaction.
					}

					// Verify that the added data does not exist
					VerifyTestStringData( contextRoot, baseId, false );
				}
			}
			finally
			{
				// Cleanup.
				DeleteTestStringData( baseId );
			}
		}


		/// <summary>
		///     Tests the following scenario:
		///     - Root context transaction with commit
		///     - Nested context non-transaction
		///     Outcome: data should be saved to the database.
		/// </summary>
		[Test]
		public void RootTranCommit_NestedNonTranTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				// Outer is transactional
				using ( DatabaseContext contextRootTran = DatabaseContext.GetContext( true ) )
				{
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );
					}

					contextRootTran.CommitTransaction( );
				}

				// Verify the data is there
				VerifyTestStringData( baseId, true );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context transaction with commit
		///     - Nested context transaction with commit
		///     Outcome: data should be saved to the database.
		/// </summary>
		[Test]
		public void RootTranCommit_NestedTranCommitTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				using ( DatabaseContext contextRoot = DatabaseContext.GetContext( true ) )
				{
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );

						// Commit transaction
						contextTran.CommitTransaction( );
					}

					// Commit transaction
					contextRoot.CommitTransaction( );
				}

				// Verify that the added data does exist
				VerifyTestStringData( baseId, true );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context transaction with commit
		///     - Nested context transaction with rollback
		///     Outcome: data should not be saved to the database.
		/// </summary>
		[Test]
		public void RootTranCommit_NestedTranRollbackTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				// Outer is transactional
				using ( DatabaseContext contextRootTran = DatabaseContext.GetContext( true ) )
				{
					// Inner is transactional. The inner basically enlists with the
					// current outer transaction.
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );

						// Rollback transaction. This rolls back the outer transaction too.
					}

					// As the inner has rolled back this does not commit any data.
					contextRootTran.CommitTransaction( );
				}

				// Verify the data is not there
				VerifyTestStringData( baseId, false );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}


		/// <summary>
		///     Tests the following scenario:
		///     - Root context scope with commit
		///     - Nested context transaction scope with commit
		///     Outcome: data should be saved to the database.
		/// </summary>
		[Test]
		public void RootTranCommit_NestedTranScopeCommitTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
				{
					using ( var scope = new TransactionScope( TransactionScopeOption.Required, new TransactionOptions
						{
							IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
						} ) )
					{
						using ( DatabaseContext.GetContext( ) )
						{
							// Add test data
							AddTestStringData( contextTran, baseId );

							// Verify the data is there
							VerifyTestStringData( contextTran, baseId, true );
						}

						// Commit
						scope.Complete( );
					}

					// Commit
					contextTran.CommitTransaction( );
				}

				// Verify the data is there
				VerifyTestStringData( baseId, true );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context scope with commit
		///     - Nested context transaction scope with rollback
		///     Outcome: data should not be saved to the database.
		/// </summary>
		[Test]
		public void RootTranCommit_NestedTranScopeRollbackTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				try
				{
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
					{
						using ( DatabaseContext.GetContext( true ) )
						{
							using ( DatabaseContext context = DatabaseContext.GetContext( ) )
							{
								// Add test data
								AddTestStringData( context, baseId );

								// Verify the data is there
								VerifyTestStringData( context, baseId, true );
							}

							// no commit
						}

						// Commit
						contextTran.CommitTransaction( );
					}
				}
				catch ( TransactionAbortedException )
				{
				}

				// Verify the data is not there
				VerifyTestStringData( baseId, false );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context transaction with commit
		///     - Nested context non-transaction
		///     Outcome: data should not be saved to the database.
		/// </summary>
		[Test]
		public void RootTranRollback_NestedNonTranTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				// Outer is transactional
				using ( DatabaseContext.GetContext( true ) )
				{
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );
					}

					// No commit
				}

				// Verify the data is not there
				VerifyTestStringData( baseId, false );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context transaction with rollback
		///     - Nested context transaction with commit
		///     Outcome: data should not be saved to the database.
		/// </summary>
		[Test]
		public void RootTranRollback_NestedTranCommitTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				using ( DatabaseContext.GetContext( true ) )
				{
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );

						// Commit transaction
						contextTran.CommitTransaction( );
					}

					// No commit from the root.
				}

				// Verify the data does not exist
				VerifyTestStringData( baseId, false );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context transaction with rollback
		///     - Nested context transaction with rollback
		///     Outcome: data should not be saved to the database.
		/// </summary>
		[Test]
		public void RootTranRollback_NestedTranRollbackTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				using ( DatabaseContext.GetContext( true ) )
				{
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );

						// No commit                        
					}

					// No commit from the root.
				}

				// Verify the data does not exist
				VerifyTestStringData( baseId, false );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}


		/// <summary>
		///     Tests the following scenario:
		///     - Root context scope with rollback
		///     - Nested context transaction scope with commit
		///     Outcome: data should not be saved to the database.
		/// </summary>
		[Test]
		public void RootTranRollback_NestedTranScopeCommitTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
				{
					using ( var scope = DatabaseContext.GetContext( true ) )	
					{
						using ( DatabaseContext.GetContext( ) )
						{
							// Add test data
							AddTestStringData( contextTran, baseId );

							// Verify the data is there
							VerifyTestStringData( contextTran, baseId, true );
						}

						// Commit
						scope.CommitTransaction( );
					}

					// rollback
				}

				// Verify the data is not there
				VerifyTestStringData( baseId, false );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context scope with rollback
		///     - Nested context transaction scope with rollback
		///     Outcome: data should not be saved to the database.
		/// </summary>
		[Test]
		public void RootTranRollback_NestedTranScopeRollbackTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				using ( DatabaseContext.GetContext( true ) )
				{
					using ( DatabaseContext.GetContext( true ) )
					{
						using ( DatabaseContext context = DatabaseContext.GetContext( ) )
						{
							// Add test data
							AddTestStringData( context, baseId );

							// Verify the data is there
							VerifyTestStringData( context, baseId, true );
						}

						// no commit
					}

					// no commit
				}

				// Verify the data is not there
				VerifyTestStringData( baseId, false );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context transaction scope with commit
		///     - Nested context non-transaction
		///     Outcome: data should be saved to the database.
		/// </summary>
		[Test]
		public void RootTranScopeCommit_NestedNonTranTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				// Outer is transactional
				using ( var scope = new TransactionScope( TransactionScopeOption.Required ) )
				{
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );
					}

					// Commit
					scope.Complete( );
				}

				// Verify the data is there
				VerifyTestStringData( baseId, true );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context transaction scope with commit
		///     - Nested context transaction with commit
		///     Outcome: data should be saved to the database.
		/// </summary>
		[Test]
		public void RootTranScopeCommit_NestedTranCommitTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				// Outer is transactional
				using ( var scope = DatabaseContext.GetContext( true ) )
				{
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );

						// Commit
						contextTran.CommitTransaction( );
					}

					// Commit
					scope.CommitTransaction( );
				}

				// Verify the data is there
				VerifyTestStringData( baseId, true );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context transaction scope with commit
		///     - Nested context transaction with rollback
		///     Outcome: data should not be saved to the database.
		/// </summary>
		[Test]
		public void RootTranScopeCommit_NestedTranRollbackTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				try
				{
					// Outer is transactional
					using ( var scope = DatabaseContext.GetContext( true ) )
					{
						using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
						{
							// Add test data
							AddTestStringData( contextTran, baseId );

							// Verify the data is there
							VerifyTestStringData( contextTran, baseId, true );
						}

						// Commit
						scope.CommitTransaction( );
					}
				}
				catch ( TransactionAbortedException )
				{
				}

				// Verify the data is not there
				VerifyTestStringData( baseId, false );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context transaction scope with rollback
		///     - Nested context non-transaction
		///     Outcome: data should not be saved to the database.
		/// </summary>
		[Test]
		public void RootTranScopeRollback_NestedNonTranTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				// Outer is transactional
				using ( DatabaseContext.GetContext( true ) )
				{
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );
					}

					// No commit
				}

				// Verify the data is not there
				VerifyTestStringData( baseId, false );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context transaction scope with rollback
		///     - Nested context transaction with commit
		///     Outcome: data should not be saved to the database.
		/// </summary>
		[Test]
		public void RootTranScopeRollback_NestedTranCommitTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				// Outer is transactional
				using ( DatabaseContext.GetContext( true ) )
				{
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );

						// commit
						contextTran.CommitTransaction( );
					}

					// No commit
				}

				// Verify the data is not there
				VerifyTestStringData( baseId, false );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

		/// <summary>
		///     Tests the following scenario:
		///     - Root context transaction scope with rollback
		///     - Nested context transaction with rollback
		///     Outcome: data should not be saved to the database.
		/// </summary>
		[Test]
		public void RootTranScopeRollback_NestedTranRollbackTest( )
		{
			Guid baseId = Guid.NewGuid( );

			try
			{
				// Outer is transactional
				using ( DatabaseContext.GetContext( true ) )
				{
					using ( DatabaseContext contextTran = DatabaseContext.GetContext( true ) )
					{
						// Add test data
						AddTestStringData( contextTran, baseId );

						// Verify the data is there
						VerifyTestStringData( contextTran, baseId, true );

						// no commit
					}

					// No commit
				}

				// Verify the data is not there
				VerifyTestStringData( baseId, false );
			}
			finally
			{
				// Clean up
				DeleteTestStringData( baseId );
			}
		}

        [Test]
        [Description("Ensure that post commit actions runs immediatly if there is no transaction")]
		[RunWithoutTransaction]
        public void PostCommitAction_RunAfterCloses()
        {
            string result = "A";
            using (var ctx = DatabaseContext.GetContext())
            {
                ctx.AddPostDisposeAction(() => result += "B");
                result += "C";
            }
            result += "D";

            Assert.AreEqual("ABCD", result);
        }

        [Test]
        [Description("Ensure that post commit actions run after the context with transaction closes")]
        [RunWithoutTransaction]
        public void PostCommitAction_RunAfterTransCloses()
        {
            string result = "A";
            using (var ctx = DatabaseContext.GetContext(true))
            {
                ctx.AddPostDisposeAction(() => result += "B");
                result += "C";
                ctx.CommitTransaction();
            }
            result += "D";

            Assert.AreEqual("ACBD", result);
	    }

        [Test]
		[Description( "Ensure that post commit actions do not run if the transaction rolled back" )]
        public void PostCommitAction_NotRunAfterTransRolledBack()
        {
            string result = "A";
            using (var ctx = DatabaseContext.GetContext(true))
            {
                ctx.AddPostDisposeAction(() => result += "B");
                result += "C";
                // Rollback
            }
            result += "D";

            Assert.AreEqual("ACD", result);
        }


        [Test]
		[Description( "Ensure that post commit actions are promoted to a higher context" )]
		[RunWithoutTransaction]
        public void PostCommitAction_Promotion()
        {
            string result = "A";
            using (var ctx = DatabaseContext.GetContext(true))
            {
                ctx.AddPostDisposeAction(() => result += "B");
                result += "C";

                using (var ctx2 = DatabaseContext.GetContext(true))
                {
                    ctx2.AddPostDisposeAction(() => result += "D");
                    result += "E";
                    ctx2.CommitTransaction();
                }
                result += "F";

                ctx.CommitTransaction();

                result += "G";
            }
            result += "H";

            Assert.AreEqual("ACEFGBDH", result);
        }

        [Test]
        [Description("Ensure that post commit actions are promoted past a a non transactional context to a higher transactional context")]
        [RunWithoutTransaction]
        public void PostCommitAction_PromotionThroughNonTran()
        {
            string result = "A";
            using (var ctx = DatabaseContext.GetContext(true))
            {
                ctx.AddPostDisposeAction(() => result += "B");
                result += "C";

                using (var ctxNoTran = DatabaseContext.GetContext())
                {

                    using (var ctx2 = DatabaseContext.GetContext(true))
                    {
                        ctx2.AddPostDisposeAction(() => result += "D");
                        result += "E";
                        ctx2.CommitTransaction();
                    }
                }
                result += "F";

                ctx.CommitTransaction();

                result += "G";
            }
            result += "H";

            Assert.AreEqual("ACEFGBDH", result);
        }

        [Test]
		[Description( "Ensure that post commit actions in a transaction that is rolled back is not promoted" )]
		[RunWithoutTransaction]
        public void PostCommitAction_RolledBackNotPromoted()
        {
            string result = "A";
            using (var ctx = DatabaseContext.GetContext(true))
            {
                ctx.AddPostDisposeAction(() => result += "B");
                result += "C";

                using (var ctx2 = DatabaseContext.GetContext(true))
                {
                    ctx2.AddPostDisposeAction(() => result += "D");
                    result += "E";
                    // Rollback
                }
                result += "F";

                ctx.CommitTransaction();
            }
            result += "G";

            Assert.AreEqual("ACEFBG", result);
        }


        [Test]
        [Description( "Ensure that slow statements run." )]
        [RunWithoutTransaction]
        public void TestSlowCommand( )
        {
            using ( var ctx = DatabaseContext.GetContext( ) )
            {
                // Note: Database command runs additional logic for slow statements
                var cmd = ctx.CreateCommand( "WAITFOR DELAY '00:00:02'" );
                cmd.ExecuteNonQuery( );
            }
        }

        [Test]
        [Description( "Ensure that slow statements run." )]
        [ExpectedException(typeof(SqlException))]
        [RunWithoutTransaction]
        public void TestSlowFailingCommand( )
        {
            using ( var ctx = DatabaseContext.GetContext( ) )
            {
                // Note: Database command runs additional logic for slow statements
                var cmd = ctx.CreateCommand( "WAITFOR DELAY '00:00:02'\r\nSELECT 1/0" );
                cmd.ExecuteNonQuery( );
            }
        }



		[Test]
		[RunWithoutTransactionAttribute]
		public void TestDeadlock( )
		{
			var threadA = new Thread( ThreadAEntryPoint );
			var threadB = new Thread( ThreadBEntryPoint );

			threadA.Start( );
			threadB.Start( );

			threadA.Join( );
			threadB.Join( );
		}

		private ManualResetEvent _threadAEvent = new ManualResetEvent( false );
		private ManualResetEvent _threadBEvent = new ManualResetEvent( false );

		private void ThreadAEntryPoint( )
		{
			try
			{
				using ( DatabaseContext ctxA = DatabaseContext.GetContext( true ) )
				using ( DatabaseContext ctxX = DatabaseContext.GetContext( true ) )
				{
					using ( DatabaseContext ctxB = DatabaseContext.GetContext( true ) )
					{
						using ( var cmd = ctxB.CreateCommand( "UPDATE Data_Alias SET Data = 'default_' WHERE Data = 'default'" ) )
						{
							cmd.ExecuteNonQuery( );
						}

						_threadAEvent.Set( );

						_threadBEvent.WaitOne( );

						using ( var cmd = ctxB.CreateCommand( "UPDATE Data_NVarChar SET Data = 'Default_' WHERE Data = 'Default'" ) )
						{
							cmd.ExecuteNonQuery( );
						}
					}
				}
			}
			catch ( Exception exc )
			{
				Debug.WriteLine( exc );
				_threadAEvent.Set( );
			}

			using ( DatabaseContext ctxX = DatabaseContext.GetContext( true ) )
			{
			}
		}

		private void ThreadBEntryPoint( )
		{
			try
			{
				using ( DatabaseContext ctxA = DatabaseContext.GetContext( true ) )
				using ( DatabaseContext ctxX = DatabaseContext.GetContext( true ) )
				{
					using ( DatabaseContext ctxB = DatabaseContext.GetContext( true ) )
					{
						using ( var cmd = ctxB.CreateCommand( "UPDATE Data_NVarChar SET Data = 'Default_' WHERE Data = 'Default'" ) )
						{
							cmd.ExecuteNonQuery( );
						}

						_threadBEvent.Set( );

						_threadAEvent.WaitOne( );

						using ( var cmd = ctxB.CreateCommand( "UPDATE Data_Alias SET Data = 'default_' WHERE Data = 'default'" ) )
						{
							cmd.ExecuteNonQuery( );
						}
					}
				}
			}
			catch ( Exception exc )
			{
				Debug.WriteLine( exc );
				_threadBEvent.Set( );
			}

			using ( DatabaseContext ctxX = DatabaseContext.GetContext( true ) )
			{
			}
		}
	}
}