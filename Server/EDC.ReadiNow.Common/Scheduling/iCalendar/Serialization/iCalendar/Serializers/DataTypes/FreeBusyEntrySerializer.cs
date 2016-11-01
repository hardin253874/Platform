// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     FreeBusyEntrySerializer class.
	/// </summary>
	public class FreeBusyEntrySerializer : PeriodSerializer
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
				return typeof ( FreeBusyEntry );
			}
		}

		/// <summary>
		///     Deserializes the specified tr.
		/// </summary>
		/// <param name="tr">The tr.</param>
		/// <returns></returns>
		public override object Deserialize( TextReader tr )
		{
			var entry = base.Deserialize( tr ) as IFreeBusyEntry;
			if ( entry != null )
			{
				if ( entry.Parameters.ContainsKey( "FBTYPE" ) )
				{
					string value = entry.Parameters.Get( "FBTYPE" );
					if ( value != null )
					{
						switch ( value.ToUpperInvariant( ) )
						{
							case "FREE":
								entry.Status = FreeBusyStatus.Free;
								break;
							case "BUSY":
								entry.Status = FreeBusyStatus.Busy;
								break;
							case "BUSY-UNAVAILABLE":
								entry.Status = FreeBusyStatus.BusyUnavailable;
								break;
							case "BUSY-TENTATIVE":
								entry.Status = FreeBusyStatus.BusyTentative;
								break;
						}
					}
				}
			}

			return entry;
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public override string SerializeToString( object obj )
		{
			var entry = obj as IFreeBusyEntry;
			if ( entry != null )
			{
				switch ( entry.Status )
				{
					case FreeBusyStatus.Busy:
						entry.Parameters.Remove( "FBTYPE" );
						break;
					case FreeBusyStatus.BusyTentative:
						entry.Parameters.Set( "FBTYPE", "BUSY-TENTATIVE" );
						break;
					case FreeBusyStatus.BusyUnavailable:
						entry.Parameters.Set( "FBTYPE", "BUSY-UNAVAILABLE" );
						break;
					case FreeBusyStatus.Free:
						entry.Parameters.Set( "FBTYPE", "FREE" );
						break;
				}
			}

			return base.SerializeToString( obj );
		}
	}
}