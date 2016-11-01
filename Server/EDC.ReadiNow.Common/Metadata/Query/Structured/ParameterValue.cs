// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using EDC.ReadiNow.Annotations;
using ProtoBuf;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
	/// <summary>
	///     Parameter value.
	/// </summary>
	[ProtoContract]
	public class ParameterValue : IStructuralEquatable, IStructuralComparable, IComparable
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="ParameterValue" /> class from being created.
		/// </summary>
		[UsedImplicitly]
		private ParameterValue( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ParameterValue" /> class.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="value">The value.</param>
		public ParameterValue( DbType type, string value )
		{
			Type = type;
			Value = value;
		}

		/// <summary>
		///     Gets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		[ProtoMember( 1 )]
		public DbType Type
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		[ProtoMember( 2 )]
		public string Value
		{
			get;
			private set;
		}

		/// <summary>
		///     Compares the current instance with another object of the same type and returns an integer that indicates whether
		///     the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>
		///     A value that indicates the relative order of the objects being compared. The return value has these meanings: Value
		///     Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs
		///     in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows
		///     <paramref name="obj" /> in the sort order.
		/// </returns>
		Int32 IComparable.CompareTo( Object obj )
		{
			return ( ( IStructuralComparable ) this ).CompareTo( obj, Comparer<Object>.Default );
		}

		/// <summary>
		///     Determines whether the current collection object precedes, occurs in the same position as, or follows another
		///     object in the sort order.
		/// </summary>
		/// <param name="other">The object to compare with the current instance.</param>
		/// <param name="comparer">
		///     An object that compares members of the current collection object with the corresponding members
		///     of <paramref name="other" />.
		/// </param>
		/// <returns>
		///     An integer that indicates the relationship of the current collection object to <paramref name="other" />, as shown
		///     in the following table.Return valueDescription-1The current instance precedes <paramref name="other" />.0The
		///     current instance and <paramref name="other" /> are equal.1The current instance follows <paramref name="other" />.
		/// </returns>
		/// <exception cref="System.ArgumentException">@Object not of type 'SqlParam;other</exception>
		Int32 IStructuralComparable.CompareTo( Object other, IComparer comparer )
		{
			if ( other == null )
				return 1;

			var objSqlParam = other as ParameterValue;

			if ( objSqlParam == null )
			{
				throw new ArgumentException( @"Object not of type 'SqlParam", "other" );
			}

			int c = comparer.Compare( Type, objSqlParam.Type );

			if ( c != 0 )
			{
				return c;
			}

			return comparer.Compare( Value, objSqlParam.Value );
		}

		/// <summary>
		///     Determines whether an object is structurally equal to the current instance.
		/// </summary>
		/// <param name="other">The object to compare with the current instance.</param>
		/// <param name="comparer">An object that determines whether the current instance and <paramref name="other" /> are equal.</param>
		/// <returns>
		///     true if the two objects are equal; otherwise, false.
		/// </returns>
		Boolean IStructuralEquatable.Equals( Object other, IEqualityComparer comparer )
		{
			if ( other == null )
				return false;

			var sqlParam = other as ParameterValue;

			if ( sqlParam == null )
			{
				return false;
			}

			return comparer.Equals( Type, sqlParam.Type ) && comparer.Equals( Value, sqlParam.Value );
		}

		/// <summary>
		///     Returns a hash code for the current instance.
		/// </summary>
		/// <param name="comparer">An object that computes the hash code of the current object.</param>
		/// <returns>
		///     The hash code for the current instance.
		/// </returns>
		Int32 IStructuralEquatable.GetHashCode( IEqualityComparer comparer )
		{
			return CombineHashCodes( comparer.GetHashCode( Type ), comparer.GetHashCode( Value ) );
		}

		/// <summary>
		///     Combines the hash codes.
		/// </summary>
		/// <param name="h1">The h1.</param>
		/// <param name="h2">The h2.</param>
		/// <returns></returns>
		internal static int CombineHashCodes( int h1, int h2 )
		{
			return ( ( ( h1 << 5 ) + h1 ) ^ h2 );
		}

		/// <summary>
		///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
		///     <see cref="T:System.Object" />.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>
		///     true if the specified object  is equal to the current object; otherwise, false.
		/// </returns>
		public override Boolean Equals( Object obj )
		{
			return ( ( IStructuralEquatable ) this ).Equals( obj, EqualityComparer<Object>.Default );
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			return ( ( IStructuralEquatable ) this ).GetHashCode( EqualityComparer<Object>.Default );
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			var sb = new StringBuilder( );
			sb.Append( "(" );
			return ToString( sb );
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <param name="sb">The sb.</param>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		private string ToString( StringBuilder sb )
		{
			sb.Append( Type );
			sb.Append( ", " );
			sb.Append( Value );
			sb.Append( ")" );
			return sb.ToString( );
		}

		/// <summary>
		///     Implements the operator ==.
		/// </summary>
		/// <param name="param1">The param1.</param>
		/// <param name="param2">The param2.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator ==( ParameterValue param1, ParameterValue param2 )
		{
			if ( ReferenceEquals( param1, null ) )
			{
				return ReferenceEquals( param2, null );
			}

			return param1.Equals( param2 );
		}

		/// <summary>
		///     Implements the operator !=.
		/// </summary>
		/// <param name="param1">The param1.</param>
		/// <param name="param2">The param2.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator !=( ParameterValue param1, ParameterValue param2 )
		{
			return !( param1 == param2 );
		}
	}
}