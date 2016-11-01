// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     Publish options.
	/// </summary>
	public enum PublishOptions
	{
		/// <summary>
		///     Default
		/// </summary>
		None = 0,

		/// <summary>
		///     Do not wait for a response from the server.
		/// </summary>
		FireAndForget = 1
	}
}