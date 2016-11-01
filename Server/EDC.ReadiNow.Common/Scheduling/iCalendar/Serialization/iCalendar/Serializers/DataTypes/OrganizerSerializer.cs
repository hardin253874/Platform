// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     OrganizerSerializer class.
	/// </summary>
	public class OrganizerSerializer : StringSerializer
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
				return typeof ( Organizer );
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

			IOrganizer o = null;
			try
			{
				o = CreateAndAssociate( ) as IOrganizer;
				if ( o != null )
				{
					string uriString = Unescape( Decode( o, value ) );

					// Prepend "mailto:" if necessary
					if ( !uriString.StartsWith( "mailto:", StringComparison.InvariantCultureIgnoreCase ) )
					{
						uriString = "mailto:" + uriString;
					}

					o.Value = new Uri( uriString );
				}
			}
// ReSharper disable EmptyGeneralCatchClause
			catch
// ReSharper restore EmptyGeneralCatchClause
			{
			}

			return o;
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public override string SerializeToString( object obj )
		{
			try
			{
				var o = obj as IOrganizer;
				if ( o != null && o.Value != null )
				{
					return Encode( o, Escape( o.Value.OriginalString ) );
				}
				return null;
			}
			catch
			{
				return null;
			}
		}
	}
}