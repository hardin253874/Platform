// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Messaging;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Diagnostics
{
	/// <summary>
	///     Diagnostic channel
	/// </summary>
	public static class DiagnosticChannel
	{
		/// <summary>
		///     The diagnostic request channel name
		/// </summary>
		private const string DiagnosticRequestChannelName = "ReadiNowDiagnosticRequests";

		/// <summary>
		///     The diagnostic response channel name
		/// </summary>
		private const string DiagnosticResponseChannelName = "ReadiNowDiagnosticResponses";

		/// <summary>
		///     The request channel
		/// </summary>
		private static IChannel<DiagnosticRequest> _requestChannel;

		/// <summary>
		///     The response channel
		/// </summary>
		private static IChannel<DiagnosticResponse> _responseChannel;

		/// <summary>
		///     Gets the response channel.
		/// </summary>
		/// <value>
		///     The response channel.
		/// </value>
		private static IChannel<DiagnosticResponse> ResponseChannel
		{
			get
			{
				return _responseChannel;
			}
		}

		/// <summary>
		///     Establishes the request channel.
		/// </summary>
		private static void EstablishRequestChannel( )
		{
			_requestChannel = Entity.DistributedMemoryManager.GetChannel<DiagnosticRequest>( DiagnosticRequestChannelName );

			_requestChannel.MessageReceived += MessageReceived;

			_requestChannel.Subscribe( );

			EventLog.Application.WriteInformation( "Diagnostics channel open." );
		}

		/// <summary>
		///     Establishes the response channel.
		/// </summary>
		private static void EstablishResponseChannel( )
		{
			_responseChannel = Entity.DistributedMemoryManager.GetChannel<DiagnosticResponse>( DiagnosticResponseChannelName );
		}

		/// <summary>
		///     Event handler that processes incoming diagnostic messages.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">
		///     The <see cref="Messaging.MessageEventArgs{DiagnosticChannelRequest}" /> instance containing the event
		///     data.
		/// </param>
		private static void MessageReceived( object sender, MessageEventArgs<DiagnosticRequest> e )
		{
			if ( e == null || e.Message == null )
			{
				return;
			}

			DiagnosticRequest request = e.Message;

			DiagnosticResponse response = request.GetResponse( );

			Publish( response );
		}

		/// <summary>
		///     Publishes the specified response.
		/// </summary>
		/// <param name="response">The response.</param>
		public static void Publish( DiagnosticResponse response )
		{
			if ( _responseChannel == null )
			{
				return;
			}

			if ( response == null )
			{
				return;
			}

			ResponseChannel.Publish( response, PublishMethod.Immediate, PublishOptions.FireAndForget );
		}

		/// <summary>
		///     Starts this instance.
		/// </summary>
		public static void Start( )
		{
			EstablishRequestChannel( );

			EstablishResponseChannel( );
		}
	}
}