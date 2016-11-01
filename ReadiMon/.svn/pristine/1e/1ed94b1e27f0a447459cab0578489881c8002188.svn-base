// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using ProtoBuf;
using ReadiMon.Shared.Diagnostics.Response;

namespace ReadiMon.Plugin.Redis.Diagnostics
{
	/// <summary>
	///     The ThreadResponse class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Diagnostics.Response.DiagnosticResponse" />
	[ProtoContract]
	public class ThreadResponse : DiagnosticResponse
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ThreadResponse" /> class.
		/// </summary>
		public ThreadResponse( )
		{
			Threads = new List<ThreadInfo>( );
		}

		/// <summary>
		///     Gets or sets the threads.
		/// </summary>
		/// <value>
		///     The threads.
		/// </value>
		[ProtoMember( 1 )]
		public List<ThreadInfo> Threads
		{
			get;
			set;
		}
	}
}