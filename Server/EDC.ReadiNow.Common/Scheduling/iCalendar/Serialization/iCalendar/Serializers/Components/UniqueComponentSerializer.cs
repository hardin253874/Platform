// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     UniqueComponentSerializer class.
	/// </summary>
	public class UniqueComponentSerializer :
		ComponentSerializer
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="UniqueComponentSerializer" /> class.
		/// </summary>
		public UniqueComponentSerializer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="UniqueComponentSerializer" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		public UniqueComponentSerializer( ISerializationContext ctx )
			: base( ctx )
		{
		}

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
				return typeof ( UniqueComponent );
			}
		}

		/// <summary>
		///     Serializes to string.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public override string SerializeToString( object obj )
		{
			var c = obj as IUniqueComponent;
			if ( c != null )
			{
				if ( c.DtStamp != null &&
				     !c.DtStamp.IsUniversalTime )
				{
					// Ensure the DTSTAMP property is in universal time before
					// it is serialized
					c.DtStamp = new iCalDateTime( c.DtStamp.Utc );
				}
			}
			return base.SerializeToString( obj );
		}
	}
}