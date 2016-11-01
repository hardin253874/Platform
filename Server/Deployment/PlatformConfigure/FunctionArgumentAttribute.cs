// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Text;

namespace PlatformConfigure
{
	/// <summary>
	///     Function argument.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = true, Inherited = false )]
	public class FunctionArgumentAttribute : Attribute
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="FunctionArgumentAttribute" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <param name="options">The options.</param>
		/// 
		public FunctionArgumentAttribute( string name, string description, string defaultValue = null, FunctionArgumentOptions options = FunctionArgumentOptions.None )
		{
			if ( string.IsNullOrEmpty( name ) )
			{
				throw new ArgumentNullException( nameof( name ) );
			}

			if ( string.IsNullOrEmpty( description ) )
			{
				throw new ArgumentNullException( nameof( description ) );
			}

			Name = name;
			Description = description;
			DefaultValue = defaultValue;
			Options = options;
		}

		/// <summary>
		///     Gets or sets the default value.
		/// </summary>
		/// <value>
		///     The default value.
		/// </value>
		public string DefaultValue
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get;
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
		}

		/// <summary>
		///     Gets the options.
		/// </summary>
		/// <value>
		///     The options.
		/// </value>
		public FunctionArgumentOptions Options
		{
			get;
		}

		/// <summary>
		///     To the long string.
		/// </summary>
		/// <returns></returns>
		public string ToLongString( int padding )
		{
			var value = new StringBuilder( );

			value.Append( $"<{Name}> ".PadRight( padding ) );

			value.Append( Description );

			bool newLine = false;

			if ( ( Options & FunctionArgumentOptions.Optional ) == FunctionArgumentOptions.Optional )
			{
				value.AppendLine( );
				newLine = true;
				value.Append( "Optional" );

				if ( !string.IsNullOrEmpty( DefaultValue ) )
				{
					value.Append( " - " );
				}
			}

			if ( !string.IsNullOrEmpty( DefaultValue ) )
			{
				if ( !newLine )
				{
					value.AppendLine( );
				}

				value.AppendFormat( "Default: {0}", DefaultValue );
			}

			return value.ToString( );
		}

		/// <summary>
		///     To the short string.
		/// </summary>
		/// <returns></returns>
		public string ToShortString( )
		{
			string value = $"-{Name}";

			if ( ( Options & FunctionArgumentOptions.Optional ) == FunctionArgumentOptions.Optional )
			{
				value = $"[{value}]";
			}

			return value;
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return ToShortString( );
		}
	}
}