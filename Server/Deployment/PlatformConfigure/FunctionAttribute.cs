// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace PlatformConfigure
{
	/// <summary>
	///     Function attribute
	/// </summary>
	public class FunctionAttribute : Attribute
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="FunctionAttribute" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public FunctionAttribute( string name )
		{
			if ( name == null )
			{
				throw new ArgumentNullException( "name" );
			}

			Name = name;

			Aliases = new List<string>( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="FunctionAttribute" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="aliases">The aliases.</param>
		public FunctionAttribute( string name, string description, params string[ ] aliases ) : this( name )
		{
			Description = description;

			foreach ( string alias in aliases )
			{
				Aliases.Add( alias );
			}
		}

		/// <summary>
		///     Gets the aliases.
		/// </summary>
		/// <value>
		///     The aliases.
		/// </value>
		public List<string> Aliases
		{
			get;
			private set;
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
			private set;
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
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return "-" + Name.Substring( 0, 1 ).ToLower( ) + Name.Substring( 1 );
		}
	}
}