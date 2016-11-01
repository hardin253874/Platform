// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;

namespace ApplicationManager.Core
{
	/// <summary>
	///     Routed processing context.
	/// </summary>
	public class RoutedProcessingContext : IProcessingContext
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RoutedProcessingContext" /> class.
		/// </summary>
		/// <param name="infoAction">The info action.</param>
		/// <param name="warningAction">The warning action.</param>
		/// <param name="errorAction"></param>
		/// <param name="progressAction"></param>
		public RoutedProcessingContext( Action<string> infoAction, Action<string> warningAction, Action<string> errorAction, Action<string> progressAction )
		{
			WriteInfoAction = infoAction;
			WriteWarningAction = warningAction;
			WriteErrorAction = errorAction;
			WriteProgressAction = progressAction;
			Report = new StatisticsReport( WriteError, WriteWarning, WriteInfo );
		}

		/// <summary>
		///     Gets the write error action.
		/// </summary>
		/// <value>
		///     The write error action.
		/// </value>
		public Action<string> WriteErrorAction
		{
			get;
		}

		/// <summary>
		///     Gets the write info action.
		/// </summary>
		/// <value>
		///     The write info action.
		/// </value>
		public Action<string> WriteInfoAction
		{
			get;
		}

		/// <summary>
		///     Gets the write progress action.
		/// </summary>
		/// <value>
		///     The write progress action.
		/// </value>
		public Action<string> WriteProgressAction
		{
			get;
		}

		/// <summary>
		///     Gets the write warning action.
		/// </summary>
		/// <value>
		///     The write warning action.
		/// </value>
		public Action<string> WriteWarningAction
		{
			get;
		}

		/// <summary>
		///     Writes the info.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void WriteInfo( string message )
		{
			WriteInfoAction?.Invoke( message );
		}

		/// <summary>
		///     Writes the warning.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void WriteWarning( string message )
		{
			WriteWarningAction?.Invoke( message );
		}

		/// <summary>
		///     Gets the report.
		/// </summary>
		/// <value>
		///     The report.
		/// </value>
		public StatisticsReport Report
		{
			get;
		}

		/// <summary>
		///     Writes the error.
		/// </summary>
		/// <param name="message">The message.</param>
		public void WriteError( string message )
		{
			WriteErrorAction?.Invoke( message );
		}

		/// <summary>
		///     Writes the progress.
		/// </summary>
		/// <param name="message">The message.</param>
		public void WriteProgress( string message )
		{
			WriteProgressAction?.Invoke( message );
		}
	}
}