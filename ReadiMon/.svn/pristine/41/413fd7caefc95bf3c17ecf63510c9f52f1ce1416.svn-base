// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     Thread Key
	/// </summary>
	public class ThreadKey : Tuple<int, int, int>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ThreadKey" /> class.
		/// </summary>
		/// <param name="processId">The process identifier.</param>
		/// <param name="appDomainId">The application domain identifier.</param>
		/// <param name="threadId">The thread identifier.</param>
		public ThreadKey( int processId, int appDomainId, int threadId ) : base( processId, appDomainId, threadId )
		{
		}

		/// <summary>
		///     Gets the application domain identifier.
		/// </summary>
		/// <value>
		///     The application domain identifier.
		/// </value>
		public int AppDomainId
		{
			get
			{
				return Item2;
			}
		}

		/// <summary>
		///     Gets the process identifier.
		/// </summary>
		/// <value>
		///     The process identifier.
		/// </value>
		public int ProcessId
		{
			get
			{
				return Item1;
			}
		}

		/// <summary>
		///     Gets the thread identifier.
		/// </summary>
		/// <value>
		///     The thread identifier.
		/// </value>
		public int ThreadId
		{
			get
			{
				return Item3;
			}
		}
	}
}