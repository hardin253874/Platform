// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.AddIn.Pipeline;
using ReadiMon.Contract;
using ReadiMon.Shared;

namespace ReadiMon.AddinSideAdapter
{
	/// <summary>
	///     Channel contract to view addin adapter.
	/// </summary>
	public class ChannelContractToViewAddinAdapter : IChannel
	{
		/// <summary>
		///     The contract
		/// </summary>
		private readonly IChannelContract _contract;

		/// <summary>
		///     The handle
		/// </summary>
		private ContractHandle _handle;

		/// <summary>
		///     Initializes a new instance of the <see cref="ChannelContractToViewAddinAdapter" /> class.
		/// </summary>
		/// <param name="contract">The contract.</param>
		public ChannelContractToViewAddinAdapter( IChannelContract contract )
		{
			_contract = contract;
			_handle = new ContractHandle( contract );
		}

		/// <summary>
		///     Sends the message.
		/// </summary>
		/// <param name="message">The message.</param>
		public void SendMessage( string message )
		{
			_contract.SendMessage( message );
		}
	}
}