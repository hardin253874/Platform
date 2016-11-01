// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.AddIn.Pipeline;
using ReadiMon.Contract;
using ReadiMon.Shared;

namespace ReadiMon.AddinSideAdapter
{
	/// <summary>
	///     Event Log Contract To View Addin Adapter
	/// </summary>
	public class EventLogContractToViewAddinAdapter : IEventLog
	{
		private readonly IEventLogContract _contract;
		private ContractHandle _handle;

		/// <summary>
		///     Initializes a new instance of the <see cref="EventLogContractToViewAddinAdapter" /> class.
		/// </summary>
		/// <param name="contract">The contract.</param>
		public EventLogContractToViewAddinAdapter( IEventLogContract contract )
		{
			_contract = contract;
			_handle = new ContractHandle( contract );
		}

		/// <summary>
		///     Writes the error.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteError( string message, params object[ ] args )
		{
			_contract.WriteError( message, args );
		}

		/// <summary>
		///     Writes the exception.
		/// </summary>
		/// <param name="exception">The exception.</param>
		public void WriteException( Exception exception )
		{
			_contract.WriteException( exception );
		}

		/// <summary>
		///     Writes the exception.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteException( Exception exception, string message, params object[ ] args )
		{
			_contract.WriteException( exception, message, args );
		}

		/// <summary>
		///     Writes the information.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteInformation( string message, params object[ ] args )
		{
			_contract.WriteInformation( message, args );
		}

		/// <summary>
		///     Writes the trace.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteTrace( string message, params object[ ] args )
		{
			_contract.WriteTrace( message, args );
		}

		/// <summary>
		///     Writes the warning.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		public void WriteWarning( string message, params object[ ] args )
		{
			_contract.WriteWarning( message, args );
		}
	}
}