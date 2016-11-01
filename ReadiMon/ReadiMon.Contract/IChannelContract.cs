// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.AddIn.Contract;

namespace ReadiMon.Contract
{
	/// <summary>
	///     IChanelContract
	/// </summary>
	public interface IChannelContract : IContract
	{
		/// <summary>
		///     Sends the message.
		/// </summary>
		/// <param name="message">The message.</param>
		void SendMessage( string message );
	}
}