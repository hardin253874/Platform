// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.AddIn.Pipeline;
using ReadiMon.Contract;
using ReadiMon.Shared;

namespace ReadiMon.HostSideAdapter
{
	/// <summary>
	///     Event Log Contract To View Host Adapter
	/// </summary>
	public class EventLogContractToViewHostAdapter : ContractBase, IEventLogContract
	{
		/// <summary>
		///     The view
		/// </summary>
		private readonly IEventLog _view;

		/// <summary>
		///     Initializes a new instance of the <see cref="EventLogContractToViewHostAdapter" /> class.
		/// </summary>
		/// <param name="view">The view.</param>
		public EventLogContractToViewHostAdapter( IEventLog view )
		{
			_view = view;
		}

		/// <summary>
		///     Writes the error.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteError( string message, params object[ ] args )
		{
			_view.WriteError( message, args );
		}

		/// <summary>
		///     Writes the exception.
		/// </summary>
		/// <param name="exception">The exception.</param>
		public void WriteException( Exception exception )
		{
			_view.WriteException( exception );
		}

		/// <summary>
		///     Writes the exception.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteException( Exception exception, string message, params object[ ] args )
		{
			_view.WriteException( exception, message, args );
		}

		/// <summary>
		///     Writes the information.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteInformation( string message, params object[ ] args )
		{
			_view.WriteInformation( message, args );
		}

		/// <summary>
		///     Writes the trace.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteTrace( string message, params object[ ] args )
		{
			_view.WriteTrace( message, args );
		}

		/// <summary>
		///     Writes the warning.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteWarning( string message, params object[ ] args )
		{
			_view.WriteWarning( message, args );
		}
	}
}