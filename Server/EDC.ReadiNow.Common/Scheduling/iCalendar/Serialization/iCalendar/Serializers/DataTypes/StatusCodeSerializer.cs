// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     StatusCodeSerializer class.
	/// </summary>
	public class StatusCodeSerializer : StringSerializer
	{
		/// <summary>
		///     Gets the type of the target.
		/// </summary>
		/// <value>
		///     The type of the target.
		/// </value>
		public override Type TargetType
		{
			get
			{
				return typeof ( StatusCode );
			}
		}

		/// <summary>
		///     Deserializes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public override object Deserialize( TextReader tr )
		{
			string value = tr.ReadToEnd( );

			var sc = CreateAndAssociate( ) as IStatusCode;
			if ( sc != null )
			{
				// Decode the value as needed
				value = Decode( sc, value );

				Match match = Regex.Match( value, @"\d(\.\d+)*" );
				if ( match.Success )
				{
					string[] parts = match.Value.Split( '.' );
					var iparts = new int[parts.Length];
					for ( int i = 0; i < parts.Length; i++ )
					{
						int num;
						if ( !Int32.TryParse( parts[ i ], out num ) )
						{
							return false;
						}
						iparts[ i ] = num;
					}

					sc.Parts = iparts;
					return sc;
				}
			}
			return null;
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public override string SerializeToString( object obj )
		{
			var sc = obj as IStatusCode;
			if ( sc != null )
			{
				var vals = new string[sc.Parts.Length];
				for ( int i = 0; i < sc.Parts.Length; i++ )
				{
					vals[ i ] = sc.Parts[ i ].ToString( CultureInfo.InvariantCulture );
				}
				return Encode( sc, Escape( string.Join( ".", vals ) ) );
			}
			return null;
		}
	}
}