// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace ReadiMon.Plugin.Redis.Profiling
{
	/// <summary>
	///     Class representing the RedisProfilerTrace type.
	/// </summary>
	/// <seealso cref="ProfilerTrace" />
	public class RedisProfilerTrace : ProfilerTrace
	{
		private string _stackTrace;

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisProfilerTrace" /> class.
		/// </summary>
		/// <param name="properties">The properties.</param>
		public RedisProfilerTrace( IDictionary<string, object> properties )
		{
			object value;

			if ( properties.TryGetValue( "Id", out value ) && value != null )
			{
				Id = new Guid( value.ToString( ) );
			}

			if ( properties.TryGetValue( "ExecuteType", out value ) && value != null )
			{
				var name = value.ToString( );

				Name = name;
				FullName = name;
			}

			if ( properties.TryGetValue( "CommandString", out value ) && value != null )
			{
				var cmd = value.ToString( );

				if ( cmd.Length > 100 )
				{
					FullName = "Key: " + cmd.Substring( 100 ) + "...";
				}
				else
				{
					FullName = "Key: " + cmd;
				}
			}

			if ( properties.TryGetValue( "DurationMilliseconds", out value ) && value != null )
			{
				DurationMilliseconds = value.ToString( );
			}

			if ( properties.TryGetValue( "StartMilliseconds", out value ) && value != null )
			{
				StartMilliseconds = value.ToString( );
			}

			Image = "red.png";

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