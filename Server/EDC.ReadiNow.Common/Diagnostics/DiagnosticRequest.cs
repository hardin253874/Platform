// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Diagnostics.Request;
using ProtoBuf;

namespace EDC.ReadiNow.Diagnostics
{
	/// <summary>
	///     Diagnostic Request message.
	/// </summary>
	[ProtoContract]
	[ProtoInclude( 100, typeof( ThreadRequest ) )]
	[ProtoInclude( 101, typeof( WorkflowRequest ) )]
	[ProtoInclude( 102, typeof( RemoteExecRequest ) )]
	[ProtoInclude( 103, typeof( FlushCachesRequest ) )]
	public abstract class DiagnosticRequest
	{
		/// <summary>
		///     Gets the response.
		/// </summary>
		/// <returns></returns>
		public abstract DiagnosticResponse GetResponse( );
	}
}