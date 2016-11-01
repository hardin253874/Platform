// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     CalendarObjectBase class.
	/// </summary>
	[Serializable]
	public class CalendarObjectBase : ICopyable, ILoadable
	{
		/// <summary>
		///     IsLoaded.
		/// </summary>
		private bool _isLoaded;

		/// <summary>
		///     Initializes a new instance of the <see cref="CalendarObjectBase" /> class.
		/// </summary>
		public CalendarObjectBase( )
		{
			// Objects that are loaded using a normal constructor
			// are "Loaded" by default.  Objects that are being
			// deserialized do not use the constructor.
			_isLoaded = true;
		}

		#region ICopyable Members

		/// <summary>
		///     Copies values from the target object to the
		///     current object.
		/// </summary>
		/// <param name="c">The c.</param>
		public virtual void CopyFrom( ICopyable c )
		{
		}

		/// <summary>
		///     Creates a copy of the object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>
		///     The copy of the object.
		/// </returns>
		public virtual T Copy<T>( )
		{
			Type type = GetType( );
			var obj = Activator.CreateInstance( type ) as ICopyable;

			// Duplicate our values
			if ( obj is T )
			{
				obj.CopyFrom( this );
				return ( T ) obj;
			}
			return default( T );
		}

		#endregion

		#region ILoadable Members

		/// <summary>
		///     Gets whether or not the object has been loaded.
		/// </summary>
		public virtual bool IsLoaded
		{
			get
			{
				return _isLoaded;
			}
		}

		/// <summary>
		///     An event that fires when the object has been loaded.
		/// </summary>
		[field: NonSerialized]
		public event EventHandler Loaded;

		/// <summary>
		///     Fires the Loaded event.
		/// </summary>
		public virtual void OnLoaded( )
		{
			_isLoaded = true;
			if ( Loaded != null )
			{
				Loaded( this, EventArgs.Empty );
			}
		}

		#endregion
	}
}