// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using NUnit.Framework;

namespace EDC.ReadiNow.Test
{
	/// <summary>
	///     Timed attribute.
	/// </summary>
	public class TimedAttribute : ReadiNowTestAttribute
	{
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
		///     Executed after each test is run
		/// </summary>
		/// <param name="testDetails">Provides details about the test that has just been run.</param>
		public override void AfterTest( TestDetails testDetails )
		{
			Timer.Stop( );

			Console.WriteLine( @"Time taken: {0}ms", Timer.ElapsedMilliseconds );
		}

		/// <summary>
		///     Executed before each test is run
		/// </summary>
		/// <param name="testDetails">Provides details about the test that is going to be run.</param>
		public override void BeforeTest( TestDetails testDetails )
		{
			Timer = new Stopwatch( );
			Timer.Start( );
		}
	}
}