// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.ObjectModel;
using System.Net.Http;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Message Configuration class.
	/// </summary>
	public static class MessageConfig
	{
		/// <summary>
		///     Registers the message handlers.
		/// </summary>
		/// <param name="handlers">The handlers.</param>
		public static void RegisterMessageHandlers( Collection<DelegatingHandler> handlers )
		{
			handlers.Add( new CompressionHandler( ) );
		}
	}
}