// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Support;

namespace ReadiMon.Core
{
	/// <summary>
	///     Section base class.
	/// </summary>
	public class Section : ViewModelBase, IComparable<Section>
	{
		/// <summary>
		///     The section entries.
		/// </summary>
		private readonly ObservableCollection<Entry> _entries = new ObservableCollection<Entry>( );

		/// <summary>
		///     The entries view
		/// </summary>
		private CollectionViewSource _entriesView;

		/// <summary>
		///     The name.
		/// </summary>
		private string _name;

		/// <summary>
		///     The ordinal.
		/// </summary>
		private int _ordinal;

		/// <summary>
		///     The selected entry.
		/// </summary>
		private Entry _selectedEntry;

		/// <summary>
		///     Initializes a new instance of the <see cref="Section" /> class.
		/// </summary>
		/// <param name="section">The section.</param>
		/// <param name="filter">The filter.</param>
		/// <exception cref="System.ArgumentNullException">section</exception>
		public Section( Section section, FilterEventHandler filter )
		{
			if ( section == null )
			{
				throw new ArgumentNullException( "section" );
			}

			Name = section.Name;
			Ordinal = section.Ordinal;

			EntriesView = new CollectionViewSource
			{
				Source = Entries
			};

			EntriesView.Filter += filter;

			var ordinalSort = new SortDescription( "Ordinal", ListSortDirection.Ascending );
			var nameSort = new SortDescription( "Name", ListSortDirection.Ascending );

			EntriesView.SortDescriptions.Add( ordinalSort );
			EntriesView.SortDescriptions.Add( nameSort );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Section" /> class.
		/// </summary>
		public Section( PluginWrapper plugin )
		{
			if ( plugin == null )
			{
				throw new ArgumentNullException( "plugin" );
			}

			Name = plugin.Plugin.SectionName;
			Ordinal = plugin.Plugin.SectionOrdinal;

			EntriesView = new CollectionViewSource
			{
				Source = Entries
			};

			EntriesView.Filter += EntriesFilter;

			var ordinalSort = new SortDescription( "Ordinal", ListSortDirection.Ascending );
			var nameSort = new SortDescription( "Name", ListSortDirection.Ascending );

			EntriesView.SortDescriptions.Add( ordinalSort );
			EntriesView.SortDescriptions.Add( nameSort );
		}

		/// <summary>
		///     Gets the entries.
		/// </summary>
		/// <value>
		///     The entries.
		/// </value>
		public ObservableCollection<Entry> Entries
		{
			get
			{
				return _entries;
			}
		}

		/// <summary>
		///     Gets or sets the entries view.
		/// </summary>
		/// <value>
		///     The entries view.
		/// </value>
		public CollectionViewSource EntriesView
		{
			get
			{
				return _entriesView;
			}
			set
			{
				SetProperty( ref _entriesView, value );
			}
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get
			{
				return _name;
			}
			private set
			{
				SetProperty( ref _name, value );
			}
		}

		/// <summary>
		///     Gets the ordinal.
		/// </summary>
		/// <value>
		///     The ordinal.
		/// </value>
		public int Ordinal
		{
			get
			{
				return _ordinal;
			}
			private set
			{
				SetProperty( ref _ordinal, value );
			}
		}

		/// <summary>
		///     Gets or sets the selected entry.
		/// </summary>
		/// <value>
		///     The selected entry.
		/// </value>
		public Entry SelectedEntry
		{
			get
			{
				return _selectedEntry;
			}
			set
			{
				Status status = null;

				try
				{
					if ( value != null )
					{
						status = Status.Set( $"Loading Plugin '{value.Name}'..." );
					}

					using ( MouseCursor.Set( Cursors.Wait ) )
					{
						SetProperty( ref _selectedEntry, value );
					}

				}
				finally
				{
					status?.Dispose( );
				}
			}
		}

		/// <summary>
		///     Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///     A value that indicates the relative order of the objects being compared. The return value has the following
		///     meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This
		///     object is equal to <paramref name="other" />. Greater than zero This object is greater than
		///     <paramref name="other" />.
		/// </returns>
		public int CompareTo( Section other )
		{
			if ( other == null )
			{
				return -1;
			}

			int result = Ordinal.CompareTo( other.Ordinal );

			if ( result != 0 )
			{
				return result;
			}

			return String.Compare( Name, other.Name, StringComparison.Ordinal );
		}

		/// <summary>
		///     Filters the entries.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="FilterEventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.ArgumentException">Invalid entry.</exception>
		private void EntriesFilter( object sender, FilterEventArgs e )
		{
			var entry = e.Item as Entry;

			if ( entry == null )
			{
				throw new ArgumentException( "Invalid entry." );
			}

			e.Accepted = entry.Plugin.Enabled && entry.Plugin.Plugin.HasUserInterface;
		}
	}
}