// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using EDC.ReadiNow.Diagnostics.Response;
using Microsoft.Diagnostics.Runtime;
using ProtoBuf;
using ThreadState = System.Diagnostics.ThreadState;

namespace EDC.ReadiNow.Diagnostics.Request
{
	/// <summary>
	///     Thread Request.
	/// </summary>
	[ProtoContract]
	public class ThreadRequest : DiagnosticRequest
	{
		/// <summary>
		///     Gets the response.
		/// </summary>
		/// <returns></returns>
		public override DiagnosticResponse GetResponse( )
		{
			try
			{
				var threadMap = new Dictionary<int, ThreadInfo>( );

				/////
				// Get the per-thread CPU utilization.
				/////
				GetThreadCpuUsage( threadMap );

				/////
				// Attempt to resolve each threads current call stack.
				/////
				GetThreadCallStacks( threadMap );

				var response = new ThreadResponse( );

				foreach ( ThreadInfo thread in threadMap.Values )
				{
					response.Threads.Add( thread );
				}

				return response;
			}
			catch ( Exception exc )
			{
				Debug.WriteLine( exc.Message );
			}

			return null;
		}

		/// <summary>
		///     Gets the thread call stacks.
		/// </summary>
		/// <param name="threadMap">The thread map.</param>
		private static void GetThreadCallStacks( Dictionary<int, ThreadInfo> threadMap )
		{
			using ( DataTarget target = DataTarget.AttachToProcess( Process.GetCurrentProcess( ).Id, 2500, AttachFlag.Passive ) )
			{
				if ( target.ClrVersions.Count > 0 )
				{
					ClrInfo version = target.ClrVersions[ 0 ];

					var runtime = version.CreateRuntime( );

					var appDomains = new Dictionary<ulong, string>( );

					foreach ( var appDomain in runtime.AppDomains )
					{
						appDomains[ appDomain.Address ] = appDomain.Name;
					}

					foreach ( var thread in runtime.Threads )
					{
						if ( !thread.IsAlive )
						{
							continue;
						}

						ThreadInfo threadInfo;

						if ( threadMap.TryGetValue( ( int ) thread.OSThreadId, out threadInfo ) )
						{
							var callStack = new StringBuilder( );

							foreach ( ClrStackFrame frame in thread.StackTrace )
							{
								callStack.AppendLine( frame.ToString( ) );
							}

							threadInfo.CallStack = callStack.ToString( );
							threadInfo.OsThreadId = ( int ) thread.OSThreadId;

							string appDomainName;

							if ( appDomains.TryGetValue( thread.AppDomain, out appDomainName ) )
							{
								threadInfo.AppDomain = appDomainName;
							}

							if ( string.IsNullOrEmpty( threadInfo.CallStack ) )
							{
								if ( thread.IsFinalizer )
								{
									threadInfo.CallStack = "[Finalizer]";
								}
								else if ( thread.IsThreadpoolWorker )
								{
									threadInfo.CallStack = "[ThreadPoolWorker]";
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Gets the thread CPU usage.
		/// </summary>
		/// <param name="threadMap">The thread map.</param>
		private static void GetThreadCpuUsage( Dictionary<int, ThreadInfo> threadMap )
		{
			Process p = Process.GetCurrentProcess( );

			var startTotalProcessorTime = p.TotalProcessorTime;

			foreach ( ProcessThread thread in p.Threads )
			{
				if ( thread.ThreadState == ThreadState.Terminated )
				{
					continue;
				}

				try
				{
					threadMap[ thread.Id ] = new ThreadInfo
					{
						Id = thread.Id,
						StartTicks = thread.TotalProcessorTime.Ticks
					};
				}
				catch ( Exception exc )
				{
					Debug.WriteLine( "Failed to determine thread CPU usage. " + exc.Message );
				}
			}

			/////
			// Wait so that we can calculate a delta.
			/////
			Thread.Sleep( 1000 );

			var endTotalProcessorTime = p.TotalProcessorTime;

			long totalCpuTime = endTotalProcessorTime.Ticks - startTotalProcessorTime.Ticks;

			if ( totalCpuTime > 0 )
			{
				foreach ( ProcessThread thread in p.Threads )
				{
					try
					{
						if ( thread.ThreadState == ThreadState.Terminated )
						{
							continue;
						}

						ThreadInfo threadInfo;

						if ( threadMap.TryGetValue( thread.Id, out threadInfo ) )
						{
							long endTime = thread.TotalProcessorTime.Ticks;
							threadInfo.CpuUsage = ( endTime - threadInfo.StartTicks ) / ( float ) totalCpuTime / Environment.ProcessorCount * 100;
						}
					}
					catch ( Exception exc )
					{
						Debug.WriteLine( "Failed to determine thread CPU usage. " + exc.Message );
					}
				}
			}
		}
	}
}