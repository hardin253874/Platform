// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Remoting.Messaging;

namespace EDC.Diagnostics
{
	/// <summary>
	///     Performance Monitor class.
	/// </summary>
	public class PerformanceMonitor : IDisposable
	{
		/// <summary>
		/// Initializes the <see cref="PerformanceMonitor"/> class.
		/// </summary>
		static PerformanceMonitor( )
		{
			Monitor = true;
			Target = PerformanceMonitorOuput.Debug;
			MinimumThreshold = 2;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceMonitor" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public PerformanceMonitor( string name )
		{
			if ( Monitor == false )
			{
				return;
			}

			Name = name;

			Timer = new Stopwatch( );
			Timer.Start( );

			var depth = CallContext.LogicalGetData( "PerformanceMonitor" ) as Stack<PerformanceMonitor>;

			if ( depth == null )
			{
				depth = new Stack<PerformanceMonitor>( );
				CallContext.LogicalSetData( "PerformanceMonitor", depth );
			}

			depth.Push( this );
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the timer.
		/// </summary>
		/// <value>
		///     The timer.
		/// </value>
		private Stopwatch Timer
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="PerformanceMonitor"/> is monitor.
		/// </summary>
		/// <value>
		///   <c>true</c> if monitor; otherwise, <c>false</c>.
		/// </value>
		public static bool Monitor
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the minimum threshold.
		/// </summary>
		/// <value>
		/// The minimum threshold.
		/// </value>
		public static int MinimumThreshold
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the target.
		/// </summary>
		/// <value>
		/// The target.
		/// </value>
		public static PerformanceMonitorOuput Target
		{
			get;
			set;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public void Dispose( )
		{
			if ( Monitor == false )
			{
				return;
			}

			Timer.Stop( );

			var depth = CallContext.LogicalGetData( "PerformanceMonitor" ) as Stack<PerformanceMonitor>;

			if ( Timer.ElapsedMilliseconds > MinimumThreshold )
			{
				if ( Target == PerformanceMonitorOuput.Console )
				{
					if ( depth != null && depth.Count > 0 )
					{
						Console.Write( @"{0} ", new object[]
							{
								string.Empty.PadLeft( depth.Count, '-' )
							} );
					}

					Console.Write( @"{0} - ", new object[]
						{
							Name
						} );

					if ( Timer.ElapsedMilliseconds > 100 )
					{
						Console.ForegroundColor = ConsoleColor.Red;
					}
					else if ( Timer.ElapsedMilliseconds > 50 )
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Gray;
					}

					Console.Write( @"{0}", new object[]
						{
							Timer.ElapsedMilliseconds.ToString( CultureInfo.InvariantCulture )
						} );

					Console.ForegroundColor = ConsoleColor.Gray;

					Console.WriteLine( @"ms" );
				}
				else
				{
					Debug.WriteLine( depth == null ? string.Format( "{0} - {1}ms", Name, Timer.ElapsedMilliseconds.ToString( CultureInfo.InvariantCulture ) ) : string.Format( "{1} {2} - {0}ms", string.Empty.PadLeft( depth.Count, '-' ), Name, Timer.ElapsedMilliseconds.ToString( CultureInfo.InvariantCulture ) ) );
				}
			}

			depth.Pop( );

			if ( depth.Count <= 0 )
			{
				CallContext.FreeNamedDataSlot( "PerformanceMonitor" );
			}
		}
	}

	/// <summary>
	/// Output to
	/// </summary>
	public enum PerformanceMonitorOuput
	{
		/// <summary>
		/// The debug window
		/// </summary>
		Debug = 0,

		/// <summary>
		/// The console window
		/// </summary>
		Console = 1
	}
}