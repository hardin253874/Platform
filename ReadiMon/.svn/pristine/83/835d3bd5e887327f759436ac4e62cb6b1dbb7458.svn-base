// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ReadiMon.Plugin.Graphs
{
	/// <summary>
	///     SeriesData class.
	/// </summary>
	public class SeriesData
	{
		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the display name.
		/// </summary>
		/// <value>
		///     The display name.
		/// </value>
		public string DisplayName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the items.
		/// </summary>
		/// <value>
		///     The items.
		/// </value>
		public ObservableCollection<TestClass> Items
		{
			get;
			set;
		}
	}

	/// <summary>
	///     TestClass class.
	/// </summary>
	/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
	public class TestClass : INotifyPropertyChanged
	{
		private float _number;

		/// <summary>
		///     Gets or sets the category.
		/// </summary>
		/// <value>
		///     The category.
		/// </value>
		public string Category
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the number.
		/// </summary>
		/// <value>
		///     The number.
		/// </value>
		public float Number
		{
			get
			{
				return _number;
			}
			set
			{
				_number = value;
				if ( PropertyChanged != null )
				{
					PropertyChanged( this, new PropertyChangedEventArgs( "Number" ) );
				}
			}
		}

		/// <summary>
		///     Occurs when [property changed].
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
	}
}