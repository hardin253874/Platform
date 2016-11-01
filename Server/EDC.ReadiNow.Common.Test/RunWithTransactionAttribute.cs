// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Transactions;
using EDC.ReadiNow.Database;
using NUnit.Framework;

namespace EDC.ReadiNow.Test
{
	/// <summary>
	///     Runs the test within a distributed transaction.
	/// </summary>
	public class RunWithTransactionAttribute : ReadiNowTestAttribute, IDisposable
	{
		private bool _active;
		private bool _disposed;

		/// <summary>
		///     Initializes a new instance of the <see cref="RunWithTransactionAttribute" /> class.
		/// </summary>
		public RunWithTransactionAttribute( )
		{
			Targets = ActionTargets.Test;
		}

		/// <summary>
		///     Gets or sets the context.
		/// </summary>
		/// <value>
		///     The context.
		/// </value>
		private DatabaseContext Scope
		{
			get;
			set;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		///     Executed before each test is run
		/// </summary>
		/// <param name="testDetails">Provides details about the test that is going to be run.</param>
		public override void BeforeTest( TestDetails testDetails )
		{
			object[ ] customAttributes = testDetails?.Method.GetCustomAttributes( typeof ( RunWithoutTransactionAttribute ), false );

			if ( customAttributes?.Length > 0 )
			{
				return;
			}

			Establish( );
		}

		/// <summary>
		///     Executed after each test is run
		/// </summary>
		/// <param name="testDetails">Provides details about the test that has just been run.</param>
		public override void AfterTest( TestDetails testDetails )
		{
			Cleanup( );
		}

		/// <summary>
		///     Provides the target for the action attribute
		/// </summary>
		/// <returns>The target for the action attribute</returns>
		public override ActionTargets Targets
		{
			get;
		}

		/// <summary>
		///     Cleanups this instance.
		/// </summary>
		private void Cleanup( )
		{
			_active = false;

			if ( Scope != null )
			{
				Scope.Dispose( );
				Scope = null;
			}
		}

		/// <summary>
		///     Handles the TransactionCompleted event of the Current control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TransactionEventArgs" /> instance containing the event data.</param>
		private void Current_TransactionCompleted( object sender, TransactionEventArgs e )
		{
			if ( e.Transaction.TransactionInformation.Status == TransactionStatus.Aborted && _active )
			{
				Cleanup( );
				Establish( );
			}
		}

		/// <summary>
		///     Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">
		///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
		///     unmanaged resources.
		/// </param>
		protected virtual void Dispose( bool disposing )
		{
			if ( _disposed )
			{
				return;
			}

			if ( disposing )
			{
				Cleanup( );
			}

			_disposed = true;
		}

		/// <summary>
		///     Establishes this instance.
		/// </summary>
		private void Establish( )
		{
			if ( Scope == null )
			{
				Scope = DatabaseContext.GetContext( true, isUnitTestTransaction: true );

				Transaction.Current.TransactionCompleted += Current_TransactionCompleted;
			}

			_active = true;
		}
	}
}