// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.ComponentModel;
using System.Windows;

namespace TenantDiffTool.Core
{
	/// <summary>
	///     ViewModel base.
	/// </summary>
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ViewModelBase" /> class.
		/// </summary>
		protected ViewModelBase( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ViewModelBase" /> class.
		/// </summary>
		/// <param name="parent">The parent.</param>
		protected ViewModelBase( Window parent )
			: this( )
		{
			ParentWindow = parent;
		}

		/// <summary>
		///     Gets the owner window.
		/// </summary>
		/// <value>
		///     The owner window.
		/// </value>
		/// <remarks>
		///     This is NOT MVVM compliant.
		/// </remarks>
		protected Window ParentWindow
		{
			get;
			private set;
		}

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