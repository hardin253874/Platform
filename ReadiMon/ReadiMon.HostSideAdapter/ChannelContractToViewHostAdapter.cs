// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.AddIn.Pipeline;
using ReadiMon.Contract;
using ReadiMon.Shared;

namespace ReadiMon.HostSideAdapter
{
	/// <summary>
	///     Chanel contract to view host adapter.
	/// </summary>
	public class ChannelContractToViewHostAdapter : ContractBase, IChannelContract
	{
		/// <summary>
		///     The view
		/// </summary>
		private readonly IChannel _view;

		/// <summary>
		///     Initializes a new instance of the <see cref="ChannelContractToViewHostAdapter" /> class.
		/// </summary>
		/// <param name="view">The view.</param>
		public ChannelContractToViewHostAdapter( IChannel view )
		{
			_view = view;
		}

		/// <summary>
		///     Sends the message.
		/// </summary>
		/// <param name="message">The message.</param>
		public void SendMessage( string message )
		{
			_view.SendMessage( message );
		}
	}
}