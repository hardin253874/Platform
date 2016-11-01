// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReadiMonUpdater
{
	/// <summary>
	///     View Model base class.
	/// </summary>
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		/// <summary>
		///     Occurs when the property changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		///     Called when the property changes.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected void OnPropertyChanged( [CallerMemberName] string propertyName = null )
		{
			PropertyChangedEventHandler eventHandler = PropertyChanged;

			if ( eventHandler != null )
			{
				eventHandler( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}

		/// <summary>
		///     Sets the property.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="storage">The storage.</param>
		/// <param name="value">The value.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		protected bool SetProperty<T>( ref T storage, T value, [CallerMemberName] String propertyName = null )
		{
			if ( Equals( storage, value ) )
			{
				return false;
			}

			storage = value;

			if ( propertyName != null )
			{
				OnPropertyChanged( propertyName );
			}

			return true;
		}
	}
}