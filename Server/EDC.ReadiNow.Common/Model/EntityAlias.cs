// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Represents an entity alias.
	/// </summary>
	[Immutable]
	public class EntityAlias
	{
		/// <summary>
		///     Core namespace.
		/// </summary>
		private const string CoreNamespace = "core";

		/// <summary>
		///     Namespace separator.
		/// </summary>
		private const char NamespaceSeperator = ':';

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityAlias" /> class.
		/// </summary>
		/// <param name="alias">The alias.</param>
		public EntityAlias( string alias )
		{
			if ( alias == null )
			{
				throw new ArgumentNullException( "alias" );
			}

			/////
			// Ensure the alias is trimmed.
			/////
			alias = alias.Trim( );

			if ( string.IsNullOrWhiteSpace( alias ) )
			{
				throw new ArgumentException( @"Invalid format.", "alias" );
			}

			/////
			// Process the input string.
			/////
			ProcessInputString( alias );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityAlias" /> class.
		/// </summary>
		/// <param name="aliasNamespace">The namespace.</param>
		/// <param name="alias">The alias.</param>
		public EntityAlias( string aliasNamespace, string alias )
		{
			if ( alias == null )
			{
				throw new ArgumentNullException( "alias" );
			}

			alias = alias.Trim( );

			if ( string.IsNullOrWhiteSpace( alias ) )
			{
				throw new ArgumentException( @"Invalid format.", "alias" );
			}

			/////
			// Ensure that the namespace is set to something.
			/////
			if ( string.IsNullOrWhiteSpace( aliasNamespace ) )
			{
				aliasNamespace = CoreNamespace;
			}

			Namespace = aliasNamespace;
			Alias = alias;
		}

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityAlias" /> class.
        /// </summary>
        /// <param name="aliasNamespace">The namespace.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="noChecksOverload">Overload that bypasses all checks because they've already been performed. Pass true.</param>
        public EntityAlias( string aliasNamespace, string alias, bool noChecksOverload )
        {
            Namespace = aliasNamespace;
            Alias = alias;
        }

		/// <summary>
		///     Gets the alias.
		/// </summary>
		public string Alias
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the namespace.
		/// </summary>
		public string Namespace
		{
			get;
			private set;
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">
		///     The <see cref="System.Object" /> to compare with this instance.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			if ( obj == null )
			{
				return false;
			}

			var alias = obj as EntityAlias;

			if ( alias == null )
			{
				return false;
			}

			return Namespace == alias.Namespace && Alias == alias.Alias;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			unchecked
			{
				int hash = 17;

				if ( Namespace != null )
				{
					hash = hash * 92821 + Namespace.GetHashCode( );
				}

				if ( Alias != null )
				{
					hash = hash * 92821 + Alias.GetHashCode( );
				}

				return hash;
			}
		}

		/// <summary>
		///     Converts the current entity alias to its corresponding entity id.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns>
		///     Entity id of the current entity alias.
		/// </returns>
		public static long ToEntityId( EntityAlias alias )
		{
			if ( alias == null || alias.Alias == null )
			{
				throw new ArgumentNullException( "alias" );
			}

			if ( alias.Alias.Length == 0 )
			{
				throw new ArgumentException( @"alias was empty", "alias" );
			}

			return EntityIdentificationCache.GetId( alias );
		}

		/// <summary>
		///     Converts the current entity alias to its corresponding entity id.
		/// </summary>
		/// <returns>
		///     Entity id of the current entity alias.
		/// </returns>
		public long ToEntityId( )
		{
			return ToEntityId( this );
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return Namespace + ":" + Alias;
		}

		/// <summary>
		///     Processes the input string.
		/// </summary>
		/// <param name="alias">The alias.</param>
		private void ProcessInputString( string alias )
		{
			/////
			// Handle name spaced aliases.
			/////
            int sep = alias.IndexOf(':');

            if (sep == -1)
			{
                Namespace = CoreNamespace;
                Alias = alias;

				if ( string.IsNullOrWhiteSpace( alias ) )
				{
					throw new ArgumentException( @"Invalid format.", "alias" );
				}
			}
			else
			{
                Namespace = alias.Substring(0, sep);
                Alias = alias.Substring(sep + 1);

				if ( string.IsNullOrWhiteSpace( Alias ) )
				{
					throw new ArgumentException( @"Invalid format.", "alias" );
				}

				if ( string.IsNullOrWhiteSpace( Namespace ) )
				{
                    Namespace = CoreNamespace;
				}
			}
		}

		/// <summary>
		///     Implements the operator !=.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator !=( EntityAlias a, EntityAlias b )
		{
			return !( a == b );
		}

		/// <summary>
		///     Implements the operator ==.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator ==( EntityAlias a, EntityAlias b )
		{
			if ( ReferenceEquals( a, b ) )
			{
				return true;
			}

			if ( ( ( object ) a == null ) || ( ( object ) b == null ) )
			{
				return false;
			}

			return a.Namespace == b.Namespace && a.Alias == b.Alias;
		}
	}
}