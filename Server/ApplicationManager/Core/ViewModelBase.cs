// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ApplicationManager.Core
{
	/// <summary>
	///     ViewModel base.
	/// </summary>
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		/// <summary>
		///     Occurs when [property changed].
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		///     Called when the property changes.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected void OnPropertyChanged( [CallerMemberName] string propertyName = null )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		/// <summary>
		///     Sets the property.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="storage">The storage.</param>
		/// <param name="value">The value.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns></returns>
		protected void SetProperty<T>( ref T storage, T value, [CallerMemberName] string propertyName = null )
		{
			if ( Equals( storage, value ) )
			{
				return;
			}

			storage = value;

			if ( propertyName != null )
			{
				// ReSharper disable once ExplicitCallerInfoArgument
				OnPropertyChanged( propertyName );
			}
		}
	}
}