// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PlatformConfigure
{
	/// <summary>
	///     Attribute argument parser.
	/// </summary>
	internal class AttributeArgParser
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="AttributeArgParser" /> class.
		/// </summary>
		public AttributeArgParser( )
		{
			Lookup = new Dictionary<string, FunctionArgumentAttribute>( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="AttributeArgParser" /> class.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="command">The command.</param>
		/// <exception cref="System.ArgumentException"></exception>
		public AttributeArgParser( Type type, string command ) : this( )
		{
			string cmd = command.TrimStart( '-' ).TrimStart( '/' );

			var methods = type.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );

			bool found = false;

			foreach ( var method in methods )
			{
				var functionAttribute = method.GetCustomAttribute<FunctionAttribute>( );

				if ( functionAttribute == null )
				{
					continue;
				}

				if ( functionAttribute.Name.Equals( cmd, StringComparison.CurrentCultureIgnoreCase ) || functionAttribute.Aliases.Any( alias => alias.Equals( cmd, StringComparison.CurrentCultureIgnoreCase ) ) )
				{
					var attributes = method.GetCustomAttributes<FunctionArgumentAttribute>( );

					foreach ( var attribute in attributes )
					{
						Lookup[ attribute.Name.ToLowerInvariant( ) ] = attribute;
					}

					found = true;
					break;
				}
			}

			if ( !found )
			{
				//throw new ArgumentException( string.Format( "No method found for command '{0}'", command ) );
			}
		}

		/// <summary>
		///     Gets or sets the lookup.
		/// </summary>
		/// <value>
		///     The lookup.
		/// </value>
		public Dictionary<string, FunctionArgumentAttribute> Lookup
		{
			get;
		}

		/// <summary>
		///     Gets the argument.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parser">The parser.</param>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns></returns>
		/// <exception cref="PlatformConfigure.CommandLineArgException"></exception>
		/// <exception cref="System.ArgumentException"></exception>
		public T GetArgument<T>( CommandLineParser parser, string argumentName )
		{
			T value;
			string parserArgumentName = argumentName;

			/////
			// If the parser doesn't contain the argument, determine is there is a default.
			/////
			if ( !parser.ContainsArgument( parserArgumentName ) )
			{
				FunctionArgumentAttribute attrib;

				Lookup.TryGetValue( argumentName.ToLowerInvariant( ), out attrib );

				if ( attrib == null || ( attrib.Options & FunctionArgumentOptions.Optional ) != FunctionArgumentOptions.Optional )
				{
					throw new CommandLineArgException( $"Missing required argument '{argumentName}'" );
				}

				string defaultValue = attrib.DefaultValue;

				if ( defaultValue == null )
				{
					return default(T);
				}

				var converter = TypeDescriptor.GetConverter( typeof( T ) );
				value = ( T ) converter.ConvertFromString( defaultValue );
			}
			else
			{
				value = parser.ValueForArgument<T>( parserArgumentName );
			}

			return value;
		}
	}
}