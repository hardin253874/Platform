// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Windows;
using System.Windows.Data;
using ReadiMon.Shared.Core;

namespace ReadiMon.Core
{
	/// <summary>
	///     Entry class.
	/// </summary>
	public class Entry : ViewModelBase, IComparable<Entry>
	{
		/// <summary>
		///     The options user interface
		/// </summary>
		private readonly Lazy<FrameworkElement> _optionsUserInterface;

		/// <summary>
		///     The user interface
		/// </summary>
		private readonly Lazy<FrameworkElement> _userInterface;

		/// <summary>
		///     The plugin.
		/// </summary>
		private PluginWrapper _plugin;

		/// <summary>
		///     Initializes a new instance of the <see cref="Entry" /> class.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <exception cref="System.ArgumentNullException">entry</exception>
		public Entry( Entry entry )
		{
			if ( entry == null )
			{
				throw new ArgumentNullException( "entry" );
			}

			Plugin = entry.Plugin;

			_userInterface = new Lazy<FrameworkElement>( ( ) => Plugin.Plugin.GetUserInterface( ) );
			_optionsUserInterface = new Lazy<FrameworkElement>( ( ) => Plugin.Plugin.GetOptionsUserInterface( ) );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Entry" /> class.
		/// </summary>
		/// <param name="plugin">The plugin.</param>
		/// <exception cref="System.ArgumentNullException">plugin</exception>
		public Entry( PluginWrapper plugin )
		{
			if ( plugin == null )
			{
				throw new ArgumentNullException( "plugin" );
			}

			Plugin = plugin;

			_userInterface = new Lazy<FrameworkElement>( ( ) => Plugin.Plugin.GetUserInterface( ) );
			_optionsUserInterface = new Lazy<FrameworkElement>( ( ) => Plugin.Plugin.GetOptionsUserInterface( ) );
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
				return null;
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance has options user interface.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has options user interface; otherwise, <c>false</c>.
		/// </value>
		public bool HasOptionsUserInterface
		{
			get
			{
				return Plugin.Plugin.HasOptionsUserInterface;
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance has user interface.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has user interface; otherwise, <c>false</c>.
		/// </value>
		public bool HasUserInterface
		{
			get
			{
				return Plugin.Plugin.HasUserInterface;
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
				return Plugin.Plugin.EntryName;
			}
		}

		/// <summary>
		///     Gets the options user interface.
		/// </summary>
		/// <value>
		///     The options user interface.
		/// </value>
		public FrameworkElement OptionsUserInterface
		{
			get
			{
				return _optionsUserInterface.Value;
			}
		}

		/// <summary>
		///     Gets a value indicating whether [options user interface loaded].
		/// </summary>
		/// <value>
		///     <c>true</c> if [options user interface loaded]; otherwise, <c>false</c>.
		/// </value>
		public bool OptionsUserInterfaceLoaded
		{
			get
			{
				return _optionsUserInterface.IsValueCreated;
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
				return Plugin.Plugin.EntryOrdinal;
			}
		}

		/// <summary>
		///     Gets the plugin.
		/// </summary>
		/// <value>
		///     The plugin.
		/// </value>
		public PluginWrapper Plugin
		{
			get
			{
				return _plugin;
			}
			private set
			{
				SetProperty( ref _plugin, value );
			}
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <value>
		///     The user interface.
		/// </value>
		public FrameworkElement UserInterface
		{
			get
			{
				return _userInterface.Value;
			}
		}

		/// <summary>
		///     Gets a value indicating whether [user interface loaded].
		/// </summary>
		/// <value>
		///     <c>true</c> if [user interface loaded]; otherwise, <c>false</c>.
		/// </value>
		public bool UserInterfaceLoaded
		{
			get
			{
				return _userInterface.IsValueCreated;
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
		public int CompareTo( Entry other )
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
	}
}