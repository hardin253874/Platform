// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     AttendeeSerializer class.
	/// </summary>
	public class AttendeeSerializer : StringSerializer
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
				return typeof ( Attendee );
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

			IAttendee a = null;
			try
			{
				a = CreateAndAssociate( ) as IAttendee;
				if ( a != null )
				{
					string uriString = Unescape( Decode( a, value ) );

					// Prepend "mailto:" if necessary
					if ( !uriString.StartsWith( "mailto:", StringComparison.InvariantCultureIgnoreCase ) )
					{
						uriString = "mailto:" + uriString;
					}

					a.Value = new Uri( uriString );
				}
			}
// ReSharper disable EmptyGeneralCatchClause
			catch
// ReSharper restore EmptyGeneralCatchClause
			{
			}

			return a;
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public override string SerializeToString( object obj )
		{
			var a = obj as IAttendee;
			if ( a != null && a.Value != null )
			{
				return Encode( a, a.Value.OriginalString );
			}
			return null;
		}
	}
}