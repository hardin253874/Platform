// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     IChannelMessage interface.
	/// </summary>
	public interface IChannelMessage<T>
	{
		/// <summary>
		///     Gets the message.
		/// </summary>
		/// <value>
		///     The message.
		/// </value>
		T Message
		{
			get;
		}
	}
}