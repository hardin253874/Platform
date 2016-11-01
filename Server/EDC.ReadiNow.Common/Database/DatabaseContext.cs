// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using System.Web;
using EDC.Database;
using EDC.ReadiNow.Configuration;
using EDC.Security;
using ReadiNow.Database;

namespace EDC.ReadiNow.Database
{
    using Trans = System.Transactions;
    using EDC.ReadiNow.IO;
    using Messaging;

    /// <summary>
    ///     Provides methods and properties for easily interacting with the application database.
    ///     This type provides an easy way for dynamic code paths to share and nest transactions without
    ///     directly exposing the IDbConnection and IDbTransaction parameters throughout all code paths.
    ///     Note: This type must be used within a 'using' block to ensure the correct and timely release
    ///     of managed resources (e.g. commands, adapters, etc.).
    ///     For example:
    ///     using(DatabaseContext databaseContext = DatabaseContext.GetContext())
    ///     {
    ///     ...
    ///     }
    /// </summary>
    public sealed class DatabaseContext : IDatabaseContext, IDisposable
    {
        /// <summary>
        /// The number of times a deadlock will be retried before giving up
        /// </summary>
        public const int DeadlockRetryCount = 3;

        /// <summary>
        /// Milliseconds
        /// </summary>
        public const long LogSlowCommandThreshold = 2000;

        private const string ContextKey = "ReadiNow Database Context";
        private readonly ArrayList _disposables = new ArrayList();
        private int _commandTimeout;
        private int _transactionTimeout;

        private Lazy<IDbConnection> _connection;
        private bool _connectionOwner;
        private DatabaseInfo _databaseInfo;
        private bool _disposed;
        private bool _transactionActive;
        private bool _transactionOwner;
        private Trans.TransactionScope _transactionScope;
        private string _transactionId;
        private bool _transactionCommitted;

        private readonly List<Action> _postDisposeActions = new List<Action>(); // actions that are to be performed after all the nested transactions are committed.
        private bool _runPostDisposeActions;
        private bool _preventPostDisposeActionsPropagating;
	    private bool _isUnitTestTransaction;

        [ThreadStatic]
        private static bool _deadlockOccurred;

		/// <summary>
		/// The cached database information
		/// </summary>
	    private static DatabaseInfo _cachedDatabaseInfo;

		/// <summary>
		///		Initializes the <see cref="DatabaseContext"/> class.
		/// </summary>
	    static DatabaseContext( )
	    {
			ConfigurationSettings.Changed += ConfigurationSettings_Changed;
	    }

		/// <summary>
		///		Handles the Changed event of the ConfigurationSettings control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		static void ConfigurationSettings_Changed( object sender, EventArgs e )
		{
			_cachedDatabaseInfo = null;
		}

	    /// <summary>
        ///     Initializes a new instance of the DatabaseContext class.
        /// </summary>
        private DatabaseContext()
        {
        }

        /// <summary>
        ///     Gets the database information associated with the context.
        /// </summary>
        public DatabaseInfo DatabaseInfo
        {
            get
            {
                return _databaseInfo;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether a transaction is currently active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if a transaction is currently active; otherwise, <c>false</c>.
        /// </value>
        public bool TransactionActive
        {
            get
            {
                return _transactionActive;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether a transaction is currently active for the current call context.
        /// </summary>
        /// <value>
        ///     <c>true</c> if a transaction is currently active; otherwise, <c>false</c>.
        /// </value>
        public static bool IsTransactionActive
        {
            get
            {
                return GetIsTransactionActive();
            }
        }

        /// <summary>
        /// Stop the post save actions from propagating up the db context stack. Usually only used in testing. 
        /// </summary>
        public bool PreventPostSaveActionsPropagating
        {
            get
            {
                return _preventPostDisposeActionsPropagating;
            }
			internal set
			{
				_preventPostDisposeActionsPropagating = value;
			}
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~DatabaseContext()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Releases any unmanaged resources and optionally any managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Adds a parameter to the specified command object.
        /// </summary>
        /// <param name="command">
        ///     The command object to update.
        /// </param>
        /// <param name="name">
        ///     A string containing the name of the parameter.
        /// </param>
        /// <param name="type">
        ///     The type of the parameter.
        /// </param>
        /// <param name="value">
        ///     The object representing the parameter value.
        /// </param>
        public IDbDataParameter AddParameter(IDbCommand command, string name, DatabaseType type, object value = null)
        {
            // This method is obsolete.
            // Call command.AddParameter(name, type, value) directly.

            return command.AddParameter(name, type, value);
        }

        /// <summary>
        ///     Adds a parameter to the specified command object.
        /// </summary>
        /// <param name="command">The command object to update.</param>
        /// <param name="name">A string containing the name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="value">The object representing the parameter value.</param>
        /// <returns></returns>
        public IDbDataParameter AddParameter(IDbCommand command, string name, DbType type, object value = null)
        {
            // This method is obsolete.
            // Call command.AddParameter(name, type, value) directly.

            return command.AddParameter(name, type, value);
        }

        /// <summary>
        ///     Commits the database transaction.
        /// </summary>
        public void CommitTransaction()
        {
            if (_connection == null || _connection.Value == null)
            {
                throw new InvalidOperationException("The database connection is invalid.");
            }

            if (_connection.Value.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("The database connection is not open.");
            }

            if (!_transactionActive)
            {
                throw new InvalidOperationException("There is no transaction to commit.");
            }

            if (!_transactionOwner)
            {
                throw new InvalidOperationException("The transaction cannot be committed because the current call context is the not the owner.");
            }

            if (_transactionActive && !string.IsNullOrEmpty(_transactionId))
            {
                _transactionCommitted = true;
            }

            if (_transactionScope != null)
            {
                _transactionScope.Complete();
                _transactionActive = false;
            }

            AddPostDisposeAction(FlushDeferredMessageContextMessages);

            _runPostDisposeActions = true;
        }

        /// <summary>
        /// Flush any deferred messages.
        /// </summary>
        private void FlushDeferredMessageContextMessages()
        {
            if (!DeferredChannelMessageContext.IsSet())
            {
                return;
            }

            using (DeferredChannelMessageContext messageContext = DeferredChannelMessageContext.GetContext())
            {
                messageContext.FlushMessages();
            }
        }

        /// <summary>
        ///     Creates a command object associated with the connection.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="commandText">A string containing the command text.</param>
        /// <returns>
        ///     A command object associated with the connection.
        /// </returns>
        public IDbCommand CreateCommand(string commandText = null, CommandType commandType = CommandType.Text)
        {
            if (_databaseInfo == null)
            {
                throw new InvalidOperationException("The database context is invalid.");
            }

            IDbCommand command;

            try
            {
                // Create the command object associated with the context
                command = DatabaseHelper.CreateCommand(_connection.Value, commandText, commandType, _commandTimeout, OnExecuteCommand);
            }
            catch (Exception exception)
            {
                throw new Exception("Unable to create a command object associated with the connection.", exception);
            }

            _disposables.Add(command);

            return command;
        }

        /// <summary>
        /// Attaches the entry point parameter to the specified command.
        /// </summary>
        /// <param name="command"></param>
        private void AttachEntryPointParameter(IDbCommand command)
        {
            if (command == null)
            {
                return;
            }

            if (command.CommandType == CommandType.Text && !string.IsNullOrEmpty(EntryPointContext.EntryPoint))
            {
                // Safety check because some application library code re-uses command objects, but changes the command type
                if (command.Parameters == null || !command.Parameters.Contains("ep"))
                {
                    int size = 50;
                    string ep = EntryPointContext.EntryPoint;
                    ep = ep.Length > size ? ep.Substring(0, size) : ep;
                    command.AddParameterWithValue("ep", ep, size);
                }
            }
            else if (command.Parameters != null && command.Parameters.Contains("ep"))
            {
                // Safety check because some application library code re-uses command objects, but changes the command type
                command.Parameters.RemoveAt("ep");
            }
        }

        /// <summary>
        /// Called when a database command executes.
        /// </summary>
        /// <param name="command">The command being executed.</param>
        /// <param name="actionToExecute">A callback that must be called, and will perform the actual execution.</param>
        private void OnExecuteCommand(IDbCommand command, Action actionToExecute)
        {
            // Start timing
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string message = null;

            // Include the entry point as a parameter to all queries
            AttachEntryPointParameter(command);

            try
            {
                // Run the command
                actionToExecute();
            }
            catch (SqlException sqlException)
            {
                message = sqlException.Message;

                // Catch deadlock
                if (sqlException.Number == 1205)
                {
                    _deadlockOccurred = true;
                }

                throw;
            }
            finally
            {
                // Detect slow queries
                long duration = stopwatch.ElapsedMilliseconds;
                stopwatch.Stop();

                if (duration > LogSlowCommandThreshold)
                {
                    if (message != null)
                        message = " then failed (" + message + ")";

                    EDC.ReadiNow.Diagnostics.EventLog.Application.WriteWarning("Slow SQL took {0}ms{1}: {2}", duration, message, command.CommandText);
                }
            }
        }

        /// <summary>
        ///     Creates an adapter with the specified select command.
        /// </summary>
        /// <param name="commandText">
        ///     A string containing a select statement.
        /// </param>
        /// <returns>
        ///     An adapter with the specified select command.
        /// </returns>
        public IDbDataAdapter CreateDataAdapter(string commandText)
        {
            if (_databaseInfo == null)
            {
                throw new InvalidOperationException("The database context is invalid.");
            }

            IDbDataAdapter adapter;

            try
            {
                SqlCommand command = _connection.Value.CreateCommand() as SqlCommand;
                command.CommandTimeout = _commandTimeout;

                if (commandText != null)
                {
                    command.CommandText = commandText;
                }

                AttachEntryPointParameter(command);

                // Create an adapter associated with the command
                adapter = new SqlDataAdapter(command);
            }
            catch (Exception exception)
            {
                throw new Exception("Unable to create an adapter with the specified select command.", exception);
            }

            _disposables.Add(adapter);

            return adapter;
        }

        /// <summary>
        ///     Releases any unmanaged resources and optionally any managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     true indicates whether method has been invoked by user code; otherwise false indicates that the method has been invoked by the run-time.
        /// </param>
        public void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // Only dispose of managed resources if invoked by user code
                if (disposing)
                {
                    if (_transactionActive && !string.IsNullOrEmpty(_transactionId) && !_transactionCommitted && !_deadlockOccurred)
                    {
                        CreateCommand("ROLLBACK TRANSACTION " + _transactionId).ExecuteNonQuery();
                    }

                    // Dispose of any managed resources
                    foreach (IDisposable obj in _disposables)
                    {
                        obj.Dispose();
                    }

                    _disposables.Clear();

                    // Pop the stack if the current context owns the connection or transaction
                    if ((_connectionOwner) || (_transactionOwner))
                    {
                        // Retrieve the stack
                        //var stack = ( Stack ) CallContext.GetData( ContextKey );
                        var stack = GetContext<Stack>(ContextKey);

                        // Validate the stack and context
                        Trace.Assert((stack != null), "The database context stack is invalid.");
                        Trace.Assert(((stack.Count > 0) && (stack.Peek() == this)), "The database context stack is corrupt.");

                        stack.Pop();

                        if (stack.Count <= 0)
                        {
                            CallContext.FreeNamedDataSlot(ContextKey);
                        }
                    }

                    // Roll the transaction back if the current context owns the transaction and it is pending                    
                    if (_transactionScope != null)
                    {
                        try
                        {
                            _transactionScope.Dispose();
                        }
                        catch (Trans.TransactionAbortedException)
                        {
                            // Ignore aborted exceptions during rollback
                        }
                        catch (InvalidOperationException)
                        {
                            // Ignore disposals that happen on different threads
                        }
                    }
                }                

                // Close the connection if the current context owns it
                if (_connectionOwner && _connection != null && _connection.IsValueCreated && _connection.Value != null)
                {
                    _connection.Value.Close();
                    _connection = null;
                }

                if (disposing)
                {
                    HandlePostDisposeActions();    
                } 
                else if (_postDisposeActions.Any())
                {
                    Diagnostics.EventLog.Application.WriteError("Disposing of a Database context when there are outstanding actions to be performed. This should never occur, probably a result of using a GetContext without a using block. Look in the code for the string 'DatabaseContext.GetContext().'}");
                }


                _disposed = true;
            }
        }


        /// <summary>
        ///     Creates a raw connection to the default database.
        /// </summary>
        public static SqlConnection GetConnection()
        {
	        DatabaseInfo databaseInfo = _cachedDatabaseInfo;

	        if ( databaseInfo == null )
	        {
		        // Get access to the database configuration settings
		        DatabaseConfiguration databaseConfiguration = ConfigurationSettings.GetDatabaseConfigurationSection( );
		        if ( databaseConfiguration == null )
		        {
			        throw new Exception( "The application database has not been configured." );
		        }

		        // Initialize the database information
		        databaseInfo = DatabaseConfigurationHelper.Convert( databaseConfiguration.ConnectionSettings );
		        if ( databaseInfo == null )
		        {
			        throw new InvalidOperationException( "The application database configuration settings are invalid." );
		        }

		        _cachedDatabaseInfo = databaseInfo;
	        }

	        IDbConnection conn = DatabaseHelper.GetConnection(databaseInfo);
            return (SqlConnection)conn;
        }

		/// <summary>
		/// Gets an object that encapsulates an open and authorized database connection.
		/// </summary>
		/// <param name="requireTransaction">if set to <c>true</c> a valid database transaction is required.</param>
		/// <param name="commandTimeout">The command timeout.</param>
		/// <param name="transactionTimeout">The transaction timeout.</param>
		/// <param name="databaseInfo">An object describing the database properties.</param>
		/// <param name="enlistTransaction">if set to <c>true</c> enlists the current transaction if possible.</param>
		/// <param name="preventPostSaveActionsPropagating">if set to <c>true</c> the post save actions will not propagate up the db context stack.</param>
		/// <param name="createConnectionImmediately">Create the connection immediately rather than when first used. Use this to prevent distributed transaction escalation problems.</param>
		/// <param name="isUnitTestTransaction">if set to <c>true</c> [is unit test transaction].</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">The application database has not been configured.</exception>
		/// <exception cref="System.InvalidOperationException">The application database configuration settings are invalid.</exception>
		public static DatabaseContext GetContext(bool requireTransaction = false, int commandTimeout = -1, int transactionTimeout = -1, DatabaseInfo databaseInfo = null, bool enlistTransaction = true, bool preventPostSaveActionsPropagating = false, bool createConnectionImmediately = false, bool isUnitTestTransaction = false )
        {
            if (DatabaseOverride.IsActive)
            {
                databaseInfo = DatabaseOverride.Current;
            }

            if (databaseInfo == null)
            {
	            databaseInfo = _cachedDatabaseInfo;

	            if ( databaseInfo == null )
	            {
		            /////
		            // Get access to the database configuration settings
		            /////
		            DatabaseConfiguration databaseConfiguration = ConfigurationSettings.GetDatabaseConfigurationSection( );

		            if ( databaseConfiguration == null )
		            {
			            throw new Exception( "The application database has not been configured." );
		            }

		            /////
		            // Initialize the database information
		            /////
		            databaseInfo = DatabaseConfigurationHelper.Convert( databaseConfiguration.ConnectionSettings );

		            if ( databaseInfo == null )
		            {
			            throw new InvalidOperationException( "The application database configuration settings are invalid." );
		            }

					_cachedDatabaseInfo = databaseInfo;
	            }
            }

            /////
            // Initialize command timeout.
            /////
            if (commandTimeout <= 0)
            {
                commandTimeout = databaseInfo.CommandTimeout;
            }

            /////
            // Initialize Transaction timeout.
            /////
            if (transactionTimeout <= 0)
            {
                transactionTimeout = databaseInfo.TransactionTimeout;
            }

            return GetContext_Impl(databaseInfo, requireTransaction, commandTimeout, transactionTimeout, enlistTransaction, preventPostSaveActionsPropagating, createConnectionImmediately, isUnitTestTransaction);
        }

        /// <summary>
        ///     Gets the underlying connection.
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetUnderlyingConnection()
        {
            return _connection.Value;
        }

        /// <summary>
        ///     Sets the value of an existing parameter.
        /// </summary>
        /// <param name="command">
        ///     The command object to update.
        /// </param>
        /// <param name="index">
        ///     The index of the parameter containing the name of the parameter.
        /// </param>
        /// <param name="value">
        ///     The value of the parameter.
        /// </param>
        public void SetParameter(IDbCommand command, int index, object value)
        {
            ((SqlParameter)command.Parameters[index]).Value = value;
        }

        /// <summary>
        ///     Sets the value of an existing parameter.
        /// </summary>
        /// <param name="command">
        ///     The command object to update.
        /// </param>
        /// <param name="name">
        ///     The parameter name.
        /// </param>
        /// <param name="value">
        ///     The value of the parameter.
        /// </param>
        public void SetParameter(IDbCommand command, string name, object value)
        {
            ((SqlParameter)command.Parameters[name]).Value = value;
        }

        /// <summary>
        ///     Get the value of an existing parameter.
        /// </summary>
        /// <param name="command">
        ///     The command object to update.
        /// </param>
        /// <param name="name">
        ///     The parameter name.
        /// </param>
        /// <returns>
        ///     The value of the parameter.
        /// </returns>
        public T GetParameterValue<T>(IDbCommand command, string name)
        {
            return (T)((SqlParameter)command.Parameters[name]).Value;
        }

        /// <summary>
        ///     Populate the data table.
        /// </summary>
        /// <param name="adapter">
        ///     The adapter to load the data.
        /// </param>
        /// <param name="dataTable">
        ///     The data table to load the data into.
        /// </param>
        public void Fill(IDbDataAdapter adapter, DataTable dataTable)
        {
            OnExecuteCommand(adapter.SelectCommand, () =>
            {
                ((SqlDataAdapter)adapter).Fill(dataTable);
            });
        }

        /// <summary>
        ///     Add a post dispose action to the context. These actions happen after the top level transaction is completed.
        ///     Actions within a transaction are only run if the transaction is committed.) Actions are run in the order they are added in the transaction completion order.
        ///     If there is no active transaction the action is run immediately.
        /// </summary>
        public void AddPostDisposeAction(Action action)
        {
            if (action == null)
                throw new ArgumentException();

            if (_transactionActive)
            {
                _postDisposeActions.Add(action);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        ///     Add a post dispose action to the context. These actions happen after the top level transaction is completed.
        ///     Actions within a transaction are only run if the transaction is committed.) Actions are run in the order they are added in the transaction completion order.
        ///     If there is no active transaction the action is run immediately.
        /// </summary>
        public void AddPostDisposeAction(IEnumerable<Action> action)
        {
            if (action == null)
                throw new ArgumentException();

            if (_transactionActive)
            {
                _postDisposeActions.AddRange(action);
            }
            else
            {
                foreach (var act in action)
                    act();
            }
        }


        [ThreadStatic]
        static bool _insideRetrier;            // if true we are already nestled within a retrier so skip retries.

        /// <summary>
        /// Run an action, retry a number of times if a deadlock or key violation occurs.
        /// This is an optimisic approach to dealing with concurrency issues.
        /// NOTE: Only the outermost retry will be used.
        /// NOTE:Retries will only be attempted at the top level of the DBContext statch when not in a transaction.
        /// </summary>
        /// 
        /// <param name="action">The action to perform.</param>
        public static void RunWithRetry(Action action)
        {
            bool previousInsideRetrier = _insideRetrier;
            try
            {
                bool skipRetries = _insideRetrier;

                if (!_insideRetrier)
                {
                    // check if there is a current context. If so bomb out. We have to be at the top level of the transaction stack.
                    //var stack = CallContext.GetData(ContextKey) as Stack;
                    var stack = GetContext<Stack>(ContextKey);

                    skipRetries = stack != null && stack.Count > 1;        // NOTE: This assumes there is at least one root DbContext call 

                    if (skipRetries)
                    {
                        Diagnostics.EventLog.Application.WriteWarning("RunWithRetry can only perform retries at the top level of the DBContext stack. No retries will be performed in case of a deadlock.\nStacktrace: \n{0}", Environment.StackTrace);
                    }
                }

                _insideRetrier = true;

                int tries = 0;

                do
                {
                    try
                    {
                        tries++;
                        action();
                        return;
                    }
                    catch (SqlException ex)
                    {
                        string reasonToRetry;

                        if (ShouldRetry(ex, out reasonToRetry))      // Deadlock
                        {

                            if (!skipRetries && tries < DeadlockRetryCount)
                            {
                                // retrying
                                Diagnostics.EventLog.Application.WriteWarning("Failed due to {0}, retrying. Attempt {1}", reasonToRetry, tries);
                            }
                            else
                                throw;
                        }
                        else
                            throw;
                    }

                } while (tries < DeadlockRetryCount);
            }
            finally
            {
                _insideRetrier = previousInsideRetrier;
            }
        }


        /// <summary>
        /// Should we retry this type of exception?
        /// </summary>
        private static bool ShouldRetry(SqlException ex, out string reasonToRetry)
        {
            switch (ex.Number)
            {
                case 1205: // Deadlock
                    reasonToRetry = "deadlock";
                    return true;
                case 547: // Foreign Key violation
                    reasonToRetry = "foreign Key violation";
                    return true;
                case 2601: // Primary key violation
                    reasonToRetry = "primary Key violation";
                    return true;
                case 823: 
                    reasonToRetry = "Sql error 823, Sql server IO subsystem error";
                    return true;
                case 824:
                    reasonToRetry = "Sql error 824, Sql server IO error: corruption detected";
                    return true;
                case 829:
                    reasonToRetry = "Sql error 829, Sql server IO error: corrupted page detected";
                    return true;
                default:
                    reasonToRetry = "not retrying";
                    return false;
            }
        }

        /// <summary>
        ///     Process the post dispose actions. If we are the top level context, run them, otherwise push them up a level.
        /// </summary>
        private void HandlePostDisposeActions()
        {
            if (PreventPostSaveActionsPropagating)
            {
                RunPostDisposeActions();
            }
            else
            {
				var stack = GetContext<Stack>( ContextKey );

				DatabaseContext parentContext = null;

				if ( stack != null && stack.Count > 0 )
				{
					parentContext = ( DatabaseContext ) stack.Peek( );
				}

				// run any post commit actions
				if ( _postDisposeActions.Count != 0 && ( _transactionScope == null || _runPostDisposeActions ) && ( ( parentContext != null && parentContext._isUnitTestTransaction && _transactionCommitted ) || !( _transactionActive && !string.IsNullOrEmpty( _transactionId ) && !_transactionCommitted ) ) )
				{
					if ( parentContext != null && !parentContext._isUnitTestTransaction && !PreventPostSaveActionsPropagating )
					{
						parentContext.AddPostDisposeAction( _postDisposeActions );
					}
					else
					{
						RunPostDisposeActions( );
					}
				}
            }
        }

        private void RunPostDisposeActions()
        {
            foreach (var action in _postDisposeActions)
            {
                try
                {
                    action();                    
                }
                catch(Exception ex)
                {
                    Diagnostics.EventLog.Application.WriteError("An error occurred running DatabaseContext post dispose action, error: {0}. Continuing to run other actions.", ex.ToString());
                }
            }
        }

        /// <summary>
        ///     Creates and opens a database connection using the specified properties.
        /// </summary>
        /// <param name="databaseInfo">An object describing the database properties.</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>
        ///     An open database connection.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">databaseInfo</exception>
        /// <exception cref="System.InvalidOperationException">The specified database provider is not supported.</exception>
        /// <exception cref="System.Exception">Cannot open the database connection.</exception>
        private static IDbConnection CreateConnection(DatabaseInfo databaseInfo)
        {
            if (databaseInfo == null)
            {
                throw new ArgumentNullException("databaseInfo");
            }

            IDbConnection connection = null;

            try
            {
                // Attempt to open a connection to the database               
                        
                connection = new SqlConnection(databaseInfo.ConnectionString);
                bool impersonate = false;

                if (databaseInfo.Authentication == DatabaseAuthentication.Integrated)
                {
                    if (databaseInfo.Credentials != null)
                    {
                        impersonate = true;

                        // Check if the context identity matches the current windows identity
                        WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();

                        if (windowsIdentity != null)
                        {
                            var principal = new WindowsPrincipal(windowsIdentity);
                            string account = ((WindowsIdentity)principal.Identity).Name;
                            if (String.Compare(CredentialHelper.GetFullyQualifiedName(databaseInfo.Credentials), account, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                impersonate = false;
                            }
                        }
                    }
                }

                // Open the database connection (impersonate if required)
                if (impersonate)
                {
                    using (ImpersonationContext.GetContext(databaseInfo.Credentials))
                    {
                        connection.Open();
                    }
                }
                else
                {
                    connection.Open();
                }                
            }
            catch (Exception)
            {
                if (connection != null)
                {
                    connection.Close();
                }
                throw;
            }

            return connection;
        }

		/// <summary>
		/// Creates a new instance of the DatabaseContext class.
		/// </summary>
		/// <param name="existingContext">The existing context.</param>
		/// <param name="databaseInfo">An object describing the database properties.</param>
		/// <param name="requireTransaction">A boolean indicating whether a transaction is required.</param>
		/// <param name="commandTimeout">The command timeout.</param>
		/// <param name="transactionTimeout">The transaction timeout.</param>
		/// <param name="enlistTransaction">if set to <c>true</c> [enlist transaction].</param>
		/// <param name="preventPostDisposeActionsPropagating">if set to <c>true</c> [prevent post dispose actions propagating].</param>
		/// <param name="createConnectionImmediately">if set to <c>true</c> [create connection immediately].</param>
		/// <param name="isUnitTestTransaction">if set to <c>true</c> [is unit test transaction].</param>
		/// <param name="newContext">if set to <c>true</c> [new context].</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">databaseInfo</exception>
		private static DatabaseContext CreateContext(DatabaseContext existingContext, DatabaseInfo databaseInfo, bool requireTransaction, int commandTimeout, int transactionTimeout, bool enlistTransaction, bool preventPostDisposeActionsPropagating, bool createConnectionImmediately, bool isUnitTestTransaction, out bool newContext)
        {
            if (databaseInfo == null)
            {
                throw new ArgumentNullException("databaseInfo");
            }

            newContext = false;

            bool saveTransaction = false;

            var context = new DatabaseContext
            {
                _databaseInfo = databaseInfo,
                _commandTimeout = commandTimeout,
                _transactionTimeout = transactionTimeout,
                _preventPostDisposeActionsPropagating = preventPostDisposeActionsPropagating
            };

            if (requireTransaction)
            {
                context._transactionOwner = true;
                context._transactionActive = true;
	            context._isUnitTestTransaction = isUnitTestTransaction;


				if (existingContext != null && existingContext._transactionActive && enlistTransaction)
                {
                    context._transactionId = TransactionId.Create();
                    saveTransaction = true;
                }
                else
                {
                    context._transactionScope = new Trans.TransactionScope(Trans.TransactionScopeOption.RequiresNew, new Trans.TransactionOptions
                    {
                        Timeout = TimeSpan.FromSeconds(context._transactionTimeout),
                        IsolationLevel = Trans.IsolationLevel.ReadCommitted
                    });
                }

                if (existingContext == null || !existingContext._transactionActive || !databaseInfo.Equals(existingContext._databaseInfo))
                {
                    // A transaction is not active, create a new connection.
                    // This is required, because connections that are to participate in
                    // transactions must be created after the transaction scope is created.
                    context._connection = new Lazy<IDbConnection>(() => CreateConnection(databaseInfo));
                    context._connectionOwner = true;

                    if (createConnectionImmediately)
                    {
                        var dummy = context._connection.Value;
                    }
                }
                else
                {
                    // A transaction is already active. Re-use the existing connection.
                    context._connection = existingContext._connection;
                    context._connectionOwner = false;
                }

                /////
                // Save transaction once the context connection has been specified.
                /////
                if (saveTransaction)
                {
                    context.CreateCommand("SAVE TRANSACTION " + context._transactionId).ExecuteNonQuery();
                }

                newContext = true;
            }
            else
            {
                if (existingContext == null)
                {
                    context._connection = new Lazy<IDbConnection>(() => CreateConnection(databaseInfo));
                    context._connectionOwner = true;

                    newContext = true;

                    if (createConnectionImmediately)
                    {
                        var dummy = context._connection.Value;
                    }
                }
                else
                {
                    context._connection = existingContext._connection;
                    context._connectionOwner = false;

                    if (existingContext._transactionActive)
                    {
                        context._transactionActive = existingContext._transactionActive;
                        context._transactionOwner = false;
                    }
                }
            }

            return context;
        }

        private static T GetContext<T>(string key)
        {
            var data = CallContext.GetData(key);

            if (data != null)
            {
                return (T)data;
            }

            var httpContext = CallContext.HostContext as HttpContext;

            if (httpContext != null)
            {
                data = httpContext.Items[key];

                if (data != null)
                {
                    return (T)data;
                }
            }

            return default(T);
        }

		/// <summary>
		/// Gets an object that encapsulates an open and authorized database connection.
		/// </summary>
		/// <param name="databaseInfo">The database info.</param>
		/// <param name="requireTransaction">if set to <c>true</c> a valid database transaction is required.</param>
		/// <param name="commandTimeout">The command timeout.</param>
		/// <param name="transactionTimeout">The transaction timeout.</param>
		/// <param name="enlistTransaction">if set to <c>true</c> enlists the current transaction if possible.</param>
		/// <param name="preventPostDisposeActionsPropagating">if set to <c>true</c> any post displose actions will occur when this context is disposed rather than propagatings up the stack.</param>
		/// <param name="createConnectionImmediately">if set to <c>true</c> [create connection immediately].</param>
		/// <param name="isUnitTestTransaction">if set to <c>true</c> [is unit test transaction].</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">databaseInfo</exception>
		private static DatabaseContext GetContext_Impl(DatabaseInfo databaseInfo, bool requireTransaction, int commandTimeout, int transactionTimeout, bool enlistTransaction, bool preventPostDisposeActionsPropagating, bool createConnectionImmediately, bool isUnitTestTransaction)
        {
            if (databaseInfo == null)
            {
                throw new ArgumentNullException("databaseInfo");
            }

            DatabaseContext context = null;

            // Retrieve the current context stack
            //var stack = ( Stack ) CallContext.GetData( ContextKey );
            var stack = GetContext<Stack>(ContextKey);
            bool newStack = false;

            // Allocate a new stack if the current stack is invalid
            if (stack == null)
            {
                stack = new Stack();
                newStack = true;
            }

            bool newContext;

            if (stack.Count > 0)
            {
                context = (DatabaseContext)stack.Peek();
            }
            else
            {
                _deadlockOccurred = false;
            }

            context = CreateContext(context, databaseInfo, requireTransaction, commandTimeout, transactionTimeout, enlistTransaction, preventPostDisposeActionsPropagating, createConnectionImmediately, isUnitTestTransaction, out newContext);

            // Push the context onto the stack (if new)
            if (newContext)
            {
                stack.Push(context);
            }

            // Assign the stack to the logical context (if not already set)
            if (newStack)
            {
                CallContext.SetData(ContextKey, stack);
            }

            return context;
        }

        /// <summary>
        /// Returns true if there is a transaction already active for the current call context.
        /// </summary>
        /// <returns>True if a transaction is active, false otherwise.</returns>
        private static bool GetIsTransactionActive()
        {
            bool isTransactionActive = false;

            var stack = GetContext<Stack>(ContextKey);

            if (stack == null || stack.Count == 0)
            {
                return false;
            }
            
            var context = (DatabaseContext)stack.Peek();

            if (context != null)
            {
                isTransactionActive = context._transactionActive;
            }            

            return isTransactionActive;
        }
    }
}