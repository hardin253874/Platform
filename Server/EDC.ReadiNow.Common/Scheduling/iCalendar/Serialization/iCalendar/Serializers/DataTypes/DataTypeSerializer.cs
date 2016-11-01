// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     DataTypeSerializer class.
	/// </summary>
	public abstract class DataTypeSerializer : SerializerBase
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DataTypeSerializer" /> class.
		/// </summary>
		protected DataTypeSerializer( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DataTypeSerializer" /> class.
		/// </summary>
		/// <param name="ctx">The CTX.</param>
		protected DataTypeSerializer( ISerializationContext ctx )
			: base( ctx )
		{
		}

		/// <summary>
		///     Creates the and associate.
		/// </summary>
		/// <returns></returns>
		protected virtual ICalendarDataType CreateAndAssociate( )
		{
			// Create an instance of the object
			var dt = Activator.CreateInstance( TargetType ) as ICalendarDataType;
			if ( dt != null )
			{
				var associatedObject = SerializationContext.Peek( ) as ICalendarObject;
				if ( associatedObject != null )
				{
					dt.AssociatedObject = associatedObject;
				}

				return dt;
			}
			return null;
		}
	}
}