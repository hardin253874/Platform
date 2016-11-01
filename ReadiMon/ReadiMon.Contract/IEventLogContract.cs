// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.AddIn.Contract;

namespace ReadiMon.Contract
{
	/// <summary>
	///     IEventLog interface.
	/// </summary>
	public interface IEventLogContract : IContract
	{
		/// <summary>
		///     Writes the error.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		void WriteError( string message, params object[ ] args );

		/// <summary>
		///     Writes the exception.
		/// </summary>
		/// <param name="exception">The exception.</param>
		void WriteException( Exception exception );

		/// <summary>
		///     Writes the exception.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		void WriteException( Exception exception, string message, params object[ ] args );

		/// <summary>
		///     Writes the information.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		void WriteInformation( string message, params object[ ] args );

		/// <summary>
		///     Writes the trace.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		void WriteTrace( string message, params object[ ] args );

		/// <summary>
		///     Writes the warning.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		void WriteWarning( string message, params object[ ] args );
	}
}