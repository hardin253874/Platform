// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using EDC.ReadiNow.Scheduling.iCalendar.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     A class that is used to specify exactly when an <see cref="Alarm" /> component will trigger.
	///     Usually this date/time is relative to the component to which the Alarm is associated.
	/// </summary>
	[Serializable]
	public sealed class Trigger : EncodableDataType, ITrigger
	{
		/// <summary>
		///     Date Time.
		/// </summary>
		private IDateTime _dateTime;

		/// <summary>
		///     Duration.
		/// </summary>
		private TimeSpan? _duration;

		/// <summary>
		///     Related.
		/// </summary>
		private TriggerRelation _related = TriggerRelation.Start;

		/// <summary>
		///     Gets or sets the date time.
		/// </summary>
		/// <value>
		///     The date time.
		/// </value>
		public IDateTime DateTime
		{
			get
			{
				return _dateTime;
			}
			set
			{
				_dateTime = value;

				if ( _dateTime != null )
				{
					// NOTE: this, along with the "Duration" setter, fixes the bug tested in
					// TODO11(), as well as this thread: https://sourceforge.net/forum/forum.php?thread_id=1926742&forum_id=656447

					// DateTime and Duration are mutually exclusive
					Duration = null;

					// Do not allow timeless date/time values
					_dateTime.HasTime = true;
				}
			}
		}

		/// <summary>
		///     Gets or sets the duration.
		/// </summary>
		/// <value>
		///     The duration.
		/// </value>
		public TimeSpan? Duration
		{
			get
			{
				return _duration;
			}
			set
			{
				_duration = value;
				if ( _duration != null )
				{
					// NOTE: see above.

					// DateTime and Duration are mutually exclusive
					DateTime = null;
				}
			}
		}

		/// <summary>
		///     Gets or sets the related.
		/// </summary>
		/// <value>
		///     The related.
		/// </value>
		public TriggerRelation Related
		{
			get
			{
				return _related;
			}
			set
			{
				_related = value;
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance is relative.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is relative; otherwise, <c>false</c>.
		/// </value>
		public bool IsRelative
		{
			get
			{
				return _duration != null;
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Trigger" /> class.
		/// </summary>
		public Trigger( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Trigger" /> class.
		/// </summary>
		/// <param name="ts">The ts.</param>
		public Trigger( TimeSpan ts )
		{
			Duration = ts;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Trigger" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public Trigger( string value )
			: this( )
		{
			var serializer = new TriggerSerializer( );
			CopyFrom( serializer.Deserialize( new StringReader( value ) ) as ICopyable );
		}

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="obj"></param>
		public override void CopyFrom( ICopyable obj )
		{
			base.CopyFrom( obj );

			var trigger = obj as ITrigger;

			if ( trigger != null )
			{
				ITrigger t = trigger;
				DateTime = t.DateTime;
				Duration = t.Duration;
				Related = t.Related;
			}
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
			var t = obj as ITrigger;
			if ( t != null )
			{
				if ( DateTime != null && !Equals( DateTime, t.DateTime ) )
				{
					return false;
				}
				if ( Duration != null && !Equals( Duration, t.Duration ) )
				{
					return false;
				}
				return Equals( Related, t.Related );
			}
// ReSharper disable BaseObjectEqualsIsObjectEquals
			return base.Equals( obj );
// ReSharper restore BaseObjectEqualsIsObjectEquals
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			int hash = 13;

			if ( DateTime != null )
			{
				hash = ( hash * 7 ) + DateTime.GetHashCode( );
			}

			if ( Duration != null )
			{
				hash = ( hash * 7 ) + Duration.GetHashCode( );
			}

			hash = ( hash * 7 ) + Related.GetHashCode( );

			return hash;
		}
	}
}