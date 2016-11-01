// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.ComponentModel;

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
		///     Raises the property changed.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected void RaisePropertyChanged( string propertyName )
		{
			PropertyChangedEventHandler handler = PropertyChanged;

			if ( handler != null )
			{
				handler( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}
	}
}