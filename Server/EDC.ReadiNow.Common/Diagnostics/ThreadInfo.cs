// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ProtoBuf;

namespace EDC.ReadiNow.Diagnostics
{
	/// <summary>
	///     Thread Info.
	/// </summary>
	[ProtoContract]
	public class ThreadInfo
	{
		/// <summary>
		///     Gets or sets the application domain.
		/// </summary>
		/// <value>
		///     The application domain.
		/// </value>
		[ProtoMember( 4 )]
		public string AppDomain
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the call stack.
		/// </summary>
		/// <value>
		///     The call stack.
		/// </value>
		[ProtoMember( 3 )]
		public string CallStack
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the CPU usage.
		/// </summary>
		/// <value>
		///     The CPU usage.
		/// </value>
		[ProtoMember( 2 )]
		public float CpuUsage
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		[ProtoMember( 1 )]
		public int Id
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the OS thread identifier.
		/// </summary>
		/// <value>
		///     The OS thread identifier.
		/// </value>
		[ProtoMember( 5 )]
		public int OsThreadId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the start ticks.
		/// </summary>
		/// <value>
		///     The start ticks.
		/// </value>
		public long StartTicks
		{
			get;
			set;
		}
	}
}