// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows.Input;
using TenantDiffTool.Core;
using TenantDiffTool.SupportClasses.Diff;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace TenantDiffTool
{
	/// <summary>
	///     Viewer view model.
	/// </summary>
	public class ViewerViewModel : ViewModelBase
	{
		/// <summary>
		///     Dialog result.
		/// </summary>
		private bool? _closeWindow;

		/// <summary>
		///     Selected object
		/// </summary>
		private object _selectedObject;

		/// <summary>
		///     The selected property value
		/// </summary>
		private string _selectedPropertyValue;

		/// <summary>
		///     Initializes a new instance of the <see cref="ViewerViewModel" /> class.
		/// </summary>
		public ViewerViewModel( )
		{
			CloseCommand = new DelegateCommand( ( ) =>
			{
				CloseWindow = true;
			} );

			SelectedPropertyChanged = new DelegateCommand<PropertyItem>( val =>
			{
				if ( val?.Value != null )
				{
					SelectedPropertyValue = val.Value.ToString( );
				}
				else
				{
					SelectedPropertyValue = string.Empty;
				}
			} );
		}

		/// <summary>
		///     Gets or sets the close command.
		/// </summary>
		/// <value>
		///     The close command.
		/// </value>
		public ICommand CloseCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the close window.
		/// </summary>
		/// <value>
		///     The close window.
		/// </value>
		public bool? CloseWindow
		{
			get
			{
				return _closeWindow;
			}
			set
			{
				if ( _closeWindow != value )
				{
					_closeWindow = value;
					RaisePropertyChanged( "CloseWindow" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the selected object.
		/// </summary>
		/// <value>
		///     The selected object.
		/// </value>
		public object SelectedObject
		{
			get
			{
				return _selectedObject;
			}
			set
			{
				if ( _selectedObject != value )
				{
					_selectedObject = value;
					RaisePropertyChanged( "SelectedObject" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the selected property changed.
		/// </summary>
		/// <value>
		///     The selected property changed.
		/// </value>
		public ICommand SelectedPropertyChanged
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the selected property value.
		/// </summary>
		/// <value>
		///     The selected property value.
		/// </value>
		public string SelectedPropertyValue
		{
			get
			{
				return _selectedPropertyValue;
			}
			set
			{
				if ( _selectedPropertyValue != value )
				{
					_selectedPropertyValue = value;
					RaisePropertyChanged( "SelectedPropertyValue" );
				}
			}
		}

		/// <summary>
		///     Sets the source.
		/// </summary>
		/// <param name="diffBase">The diff base.</param>
		/// <param name="source">The source.</param>
		public void SetSource( DiffBase diffBase, ISource source )
		{
			diffBase.Source = source;

			SelectedObject = diffBase;
		}
	}
}