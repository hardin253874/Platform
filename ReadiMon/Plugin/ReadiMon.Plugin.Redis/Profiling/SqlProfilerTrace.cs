// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace ReadiMon.Plugin.Redis.Profiling
{
	/// <summary>
	///     The SqlProfilerTrace class.
	/// </summary>
	/// <seealso cref="ReadiMon.Plugin.Redis.Profiling.ProfilerTrace" />
	public class SqlProfilerTrace : ProfilerTrace
	{
		private string _stackTrace;

		/// <summary>
		///     Initializes a new instance of the <see cref="SqlProfilerTrace" /> class.
		/// </summary>
		/// <param name="properties">The properties.</param>
		public SqlProfilerTrace( IDictionary<string, object> properties )
		{
			object value;

			if ( properties.TryGetValue( "Id", out value ) && value != null )
			{
				Id = new Guid( value.ToString( ) );
			}

			if ( properties.TryGetValue( "CommandString", out value ) && value != null )
			{
				var name = value.ToString( );

				int splitPosition = name.IndexOf( " - ", StringComparison.Ordinal );

				if ( splitPosition >= 0 )
				{
					Name = name.Substring( 0, splitPosition );
				}
				else
				{
					Name = name;
				}

				FullName = name;
			}

			if ( properties.TryGetValue( "DurationMilliseconds", out value ) && value != null )
			{
				DurationMilliseconds = value.ToString( );
			}

			if ( properties.TryGetValue( "StartMilliseconds", out value ) && value != null )
			{
				StartMilliseconds = value.ToString( );
			}

			if ( properties.TryGetValue( "StackTraceSnippet", out value ) && value != null )
			{
				StackTrace = value.ToString( );
			}

			Image = "sql.png";

			IsLeaf = true;
		}

		/// <summary>
		///     Gets or sets the stack trace.
		/// </summary>
		/// <value>
		///     The stack trace.
		/// </value>
		public string StackTrace
		{
			get
			{
				return _stackTrace;
			}
			set
			{
				SetProperty( ref _stackTrace, value );
			}
		}

		/// <summary>
		///     Gets the tooltip.
		/// </summary>
		/// <value>
		///     The tooltip.
		/// </value>
		public override string Tooltip
		{
			get
			{
				return FullName;
			}
		}
	}
}