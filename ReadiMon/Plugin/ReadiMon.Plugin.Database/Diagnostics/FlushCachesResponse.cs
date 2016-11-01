// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ProtoBuf;
using ReadiMon.Shared.Diagnostics.Response;

namespace ReadiMon.Plugin.Database.Diagnostics
{
	/// <summary>
	///     The flush caches response class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Diagnostics.Response.DiagnosticResponse" />
	[ProtoContract]
	public class FlushCachesResponse : DiagnosticResponse
	{
	}
}