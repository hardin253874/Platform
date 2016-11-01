// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     Publish Methods.
	/// </summary>
	public enum PublishMethod
	{
		/// <summary>
		///     Messages are published as they are received.
		/// </summary>
		Immediate = 0,

		/// <summary>
		///     Messages are queued and sent en masse when the channel is disposed.
		/// </summary>
		Deferred = 1
	}
}