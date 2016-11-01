// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using ProtoBuf;
using ReadiMon.Shared.Diagnostics.Response;

namespace ReadiMon.Plugin.Redis.Diagnostics
{
	/// <summary>
	///     RemoteExecResponse class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Diagnostics.Response.DiagnosticResponse" />
	[ProtoContract]
	public class RemoteExecResponse : DiagnosticResponse
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RemoteExecResponse" /> class.
		/// </summary>
		public RemoteExecResponse( )
		{
			Data = new List<Tuple<string, string>>( );
		}


		/// <summary>
		///     The data.
		/// </summary>
		[ProtoMember( 1 )]
		public List<Tuple<string, string>> Data
		{
			get;
			set;
		}


		/// <summary>
		///     The id of the response.
		/// </summary>
		[ProtoMember( 2 )]
		public string Id
		{
			get;
			set;
		}
	}
}