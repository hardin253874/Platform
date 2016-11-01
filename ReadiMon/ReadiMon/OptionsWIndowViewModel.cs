// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ReadiMon.Core;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Support;

namespace ReadiMon
{
	/// <summary>
	///     The Options window view model.
	/// </summary>
	public class OptionsWindowViewModel : ViewModelBase
	{
		/// <summary>
		///     The current view.
		/// </summary>
		private FrameworkElement _currentView;

		/// <summary>
		///     The sections
		/// </summary>
		private ObservableCollection<Section> _sections = new ObservableCollection<Section>( );

		/// <summary>
		///     The sections view.
		/// </summary>
		private CollectionViewSource _sectionsView;

		/// <summary>
		///     The selected item
		/// </summary>
		private object _selectedItem;

		/// <summary>
		///     Initializes a new instance of the <see cref="OptionsWindowViewModel" /> class.
		/// </summary>
		/// <param name="parentWindow">The parent window.</param>
		/// <exception cref="System.ArgumentNullException">parentWindow</exception>
		public OptionsWindowViewModel( Window parentWindow )
		{
			if ( parentWindow == null )
			{
				throw new ArgumentNullException( "parentWindow" );
			}

			ParentWindow = parentWindow;

			OkCommand = new DelegateCommand( Ok );
			CancelCommand = new DelegateCommand( Cancel );

			LoadedViews = new List<FrameworkElement>( );
		}

		/// <summary>
		///     Gets or sets the cancel command.
		/// </summary>
		/// <value>
		///     The cancel command.
		/// </value>
		public ICommand CancelCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the current view.
		/// </summary>
		/// <value>
		///     The current view.
		/// </value>
		public FrameworkElement CurrentView
		{
			get
			{
				return _currentView;
			}
			private set
			{
				SetProperty( ref _currentView, value );
			}
		}

		/// <summary>
		///     Gets or sets the loaded views.
		/// </summary>
		/// <value>
		///     The loaded views.
		/// </value>
		private List<FrameworkElement> LoadedViews
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the OK command.
		/// </summary>
		/// <value>
		///     The OK command.
		/// </value>
		public ICommand OkCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the parent window.
		/// </summary>
		/// <value>
		///     The parent window.
		/// </value>
		private Window ParentWindow
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the sections.
		/// </summary>
		/// <value>
		///     The sections.
		/// </value>
		public ObservableCollection<Section> Sections
		{
			get
			{
				return _sections;
			}
			set
			{
				SetProperty( ref _sections, value );

				SectionsView = new CollectionViewSource
				{
					Source = Sections
				};

				SectionsView.Filter += SectionsView_Filter;

				if ( SelectedItem == null )
				{
					if ( SectionsView.View.MoveCurrentToFirst( ) )
					{
						var section = SectionsView.View.CurrentItem as Section;

						if ( section != null )
						{
							if ( section.EntriesView.View.MoveCurrentToFirst( ) )
							{
								var entry = section.EntriesView.View.CurrentItem as Entry;

								if ( entry != null )
								{
									SelectedItem = entry;
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Gets or sets the sections view.
		/// </summary>
		/// <value>
		///     The sections view.
		/// </value>
		public CollectionViewSource SectionsView
		{
			get
			{
				return _sectionsView;
			}
			set
			{
				SetProperty( ref _sectionsView, value );
			}
		}

		/// <summary>
		///     Gets or sets the selected item.
		/// </summary>
		/// <value>
		///     The selected item.
		/// </value>
		public object SelectedItem
		{
			get
			{
				return _selectedItem;
			}
			set
			{
				using ( MouseCursor.Set( Cursors.Wait ) )
				{
					SetProperty( ref _selectedItem, value );

					var entry = value as Entry;

					if ( entry != null )
					{
						CurrentView = entry.OptionsUserInterface;
					}
					else
					{
						var section = value as Section;

						if ( section != null )
						{
							section.EntriesView.View.MoveCurrentToFirst( );

							entry = section.EntriesView.View.CurrentItem as Entry;

							if ( entry != null )
							{
								CurrentView = entry.OptionsUserInterface;
							}
						}
					}
				}

				// Call ConfigurationOnLoad here (might need to add more methods to the contract. Also may need to make elements static in the plugin classes themselves )
				// Attempting to be able to load a configuration from the .config file and populate the options view model. Then when saving, save the changes back to the .config file.

				if ( CurrentView != null )
				{
					LoadedViews.Add( CurrentView );
				}
			}
		}

		/// <summary>
		///     Cancel clicked.
		/// </summary>
		private void Cancel( )
		{
			ParentWindow.Close( );
		}

		/// <summary>
		///     OK clicked.
		/// </summary>
		private void Ok( )
		{
			foreach ( Section section in SectionsView.View )
			{
				foreach ( Entry entry in section.EntriesView.View )
				{
					entry.Plugin.Plugin.SaveOptions( );
				}
			}

			ParentWindow.Close( );
		}

		/// <summary>
		///     Handles the Filter event of the SectionsView control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="FilterEventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.ArgumentException">Invalid section.</exception>
		private void SectionsView_Filter( object sender, FilterEventArgs e )
		{
			var section = e.Item as Section;

			if ( section == null )
			{
				throw new ArgumentException( "Invalid section." );
			}

			if ( !section.EntriesView.View.IsEmpty )
			{
				if ( section.EntriesView.View.Cast<Entry>( ).Any( entry => entry.HasOptionsUserInterface ) )
				{
					e.Accepted = true;
					return;
				}
			}

			e.Accepted = false;
		}
	}
}