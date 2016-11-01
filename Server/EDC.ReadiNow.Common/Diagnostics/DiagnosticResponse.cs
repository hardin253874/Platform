// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Diagnostics.Response;
using ProtoBuf;

namespace EDC.ReadiNow.Diagnostics
{
	/// <summary>
	///     Diagnostic response message.
	/// </summary>
	[ProtoContract]
	[ProtoInclude( 100, typeof( ThreadResponse ) )]
	[ProtoInclude( 101, typeof( WorkflowResponse ) )]
	[ProtoInclude( 102, typeof( RemoteExecResponse ) )]
	[ProtoInclude( 103, typeof( FlushCachesResponse ) )]
	public abstract class DiagnosticResponse
	{
	}
}