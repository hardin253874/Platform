// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Text;

namespace EDC.Text
{
	/// <summary>
	///     Provides helper methods for interacting with strings.
	/// </summary>
	public static class StringHelper
	{
		/// <summary>
		///     Converts a comma separated list into an array of objects.
		/// </summary>
		/// <param name="list">
		///     A string representing a comma-separated list.
		/// </param>
		/// <param name="type">
		///     The type of the array elements.
		/// </param>
		/// <returns>
		///     An array of objects contained within the comma-separated list.
		/// </returns>
		public static object FromCsv( string list, Type type )
		{
			object values = null;

			if ( !String.IsNullOrEmpty( list ) )
			{
				if ( type == typeof ( Int16 ) )
				{
					string[] elements = list.Split( new[]
						{
							','
						}, StringSplitOptions.RemoveEmptyEntries );
					if ( ( elements.Length > 0 ) )
					{
						var temp = new Int16[elements.Length];
						for ( int x = 0; x < elements.Length; ++x )
						{
							temp[ x ] = Convert.ToInt16( elements[ x ] );
						}
						values = temp;
					}
				}
				else if ( type == typeof ( Int32 ) )
				{
					string[] elements = list.Split( new[]
						{
							','
						}, StringSplitOptions.RemoveEmptyEntries );
					if ( ( elements.Length > 0 ) )
					{
						var temp = new Int32[elements.Length];
						for ( int x = 0; x < elements.Length; ++x )
						{
							temp[ x ] = Convert.ToInt32( elements[ x ] );
						}
						values = temp;
					}
				}
				else if ( type == typeof ( long ) )
				{
					string[] elements = list.Split( new[]
						{
							','
						}, StringSplitOptions.RemoveEmptyEntries );
					if ( ( elements.Length > 0 ) )
					{
						var temp = new Int64[elements.Length];
						for ( int x = 0; x < elements.Length; ++x )
						{
							temp[ x ] = Convert.ToInt64( elements[ x ] );
						}
						values = temp;
					}
				}
				else if ( type == typeof ( Guid ) )
				{
					string[] elements = list.Split( new[]
						{
							','
						}, StringSplitOptions.RemoveEmptyEntries );
					if ( ( elements.Length > 0 ) )
					{
						var temp = new Guid[elements.Length];
						for ( int x = 0; x < elements.Length; ++x )
						{
							temp[ x ] = Guid.Parse( elements[ x ] );
						}
						values = temp;
					}
				}
				else
				{
					throw new InvalidOperationException( "The specified list cannot be converted." );
				}
			}

			return values;
		}

		/// <summary>
		///     Gets the key value pair.
		/// </summary>
		/// <param name="source">
		///     The source.
		/// </param>
		/// <param name="delimiter">
		///     The delimiter.
		/// </param>
		/// <returns>
		///     A KeyValuePair representing the source split on the delimiter.
		/// </returns>
		public static KeyValuePair<string, string> GetKeyValuePair( string source, string delimiter )
		{
			if ( source == null )
			{
				throw new ArgumentNullException( "source" );
			}

			if ( string.IsNullOrEmpty( delimiter ) )
			{
				throw new ArgumentException( @"Invalid delimiter.", "delimiter" );
			}

			if ( source == string.Empty )
			{
				return new KeyValuePair<string, string>( string.Empty, string.Empty );
			}

			/////
			// Split on the delimiter.
			/////
			string[] splitResults = source.Split( new[]
				{
					delimiter
				}, StringSplitOptions.None );

			if ( splitResults.Length == 0 )
			{
				return new KeyValuePair<string, string>( string.Empty, string.Empty );
			}

			if ( splitResults.Length == 1 )
			{
				return new KeyValuePair<string, string>( splitResults[ 0 ], string.Empty );
			}

			return new KeyValuePair<string, string>( splitResults[ 0 ], splitResults[ 1 ] );
		}


		/// <summary>
		///     Gets the key value pairs.
		/// </summary>
		/// <param name="source">
		///     The source.
		/// </param>
		/// <param name="delimiter">
		///     The delimiter.
		/// </param>
		/// <param name="keyValuePairDelimiter">
		///     The key value pair delimiter.
		/// </param>
		/// <returns></returns>
		public static IDictionary<string, string> GetKeyValuePairs( string source, string delimiter, string keyValuePairDelimiter )
		{
			if ( source == null )
			{
				throw new ArgumentNullException( "source" );
			}

			if ( string.IsNullOrEmpty( delimiter ) )
			{
				throw new ArgumentException( @"Invalid delimiter.", "delimiter" );
			}

			if ( string.IsNullOrEmpty( delimiter ) )
			{
				throw new ArgumentException( @"Invalid Key Value Pair delimiter.", "keyValuePairDelimiter" );
			}

			IDictionary<string, string> dictionary = new Dictionary<string, string>( );

			string[] values = source.Split( new[]
				{
					delimiter
				}, StringSplitOptions.RemoveEmptyEntries );

			foreach ( string val in values )
			{
				dictionary.Add( GetKeyValuePair( val, keyValuePairDelimiter ) );
			}

			return dictionary;
		}

		/// <summary>
		///     Converts an array of values to a comma-separated list.
		/// </summary>
		/// <param name="values">An array of value to convert.</param>
		/// <param name="format">The format.</param>
		/// <returns>
		///     A string representing a comma-separated list.
		/// </returns>
		public static string ToCSV( Int16[] values, string format )
		{
			string csv = string.Empty;

			if ( ( values != null ) && ( values.Length > 0 ) )
			{
				var builder = new StringBuilder( );
				int count = 0;

				foreach ( Int16 value in values )
				{
					if ( count++ > 0 )
					{
						builder.Append( "," );
					}

					builder.Append( value.ToString( format ) );
				}

				csv = builder.ToString( );
			}

			return csv;
		}

		/// <summary>
		///     Converts an array of values to a comma-separated list.
		/// </summary>
		/// <param name="values">An array of value to convert.</param>
		/// <param name="format">The format.</param>
		/// <returns>
		///     A string representing a comma-separated list.
		/// </returns>
		public static string ToCSV( Int32[] values, string format )
		{
			string csv = string.Empty;

			if ( ( values != null ) && ( values.Length > 0 ) )
			{
				var builder = new StringBuilder( );
				int count = 0;

				foreach ( Int32 value in values )
				{
					if ( count++ > 0 )
					{
						builder.Append( "," );
					}

					builder.Append( value.ToString( format ) );
				}

				csv = builder.ToString( );
			}

			return csv;
		}

		/// <summary>
		///     Converts an array of values to a comma-separated list.
		/// </summary>
		/// <param name="values">An array of value to convert.</param>
		/// <param name="format">The format.</param>
		/// <returns>
		///     A string representing a comma-separated list.
		/// </returns>
		public static string ToCSV( Int64[] values, string format )
		{
			string csv = string.Empty;

			if ( ( values != null ) && ( values.Length > 0 ) )
			{
				var builder = new StringBuilder( );
				int count = 0;

				foreach ( Int64 value in values )
				{
					if ( count++ > 0 )
					{
						builder.Append( "," );
					}

					builder.Append( value.ToString( format ) );
				}

				csv = builder.ToString( );
			}

			return csv;
		}

		/// <summary>
		///     Converts an array of values to a comma-separated list.
		/// </summary>
		/// <param name="values">An array of value to convert.</param>
		/// <param name="format">The format.</param>
		/// <returns>
		///     A string representing a comma-separated list.
		/// </returns>
		public static string ToCSV( Guid[] values, string format )
		{
			string csv = string.Empty;

			if ( ( values != null ) && ( values.Length > 0 ) )
			{
				var builder = new StringBuilder( );
				int count = 0;

				foreach ( Guid value in values )
				{
					if ( count++ > 0 )
					{
						builder.Append( "," );
					}

					builder.Append( value.ToString( format ) );
				}

				csv = builder.ToString( );
			}

			return csv;
		}
	}
}