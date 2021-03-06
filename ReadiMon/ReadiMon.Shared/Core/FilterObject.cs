﻿// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace ReadiMon.Shared.Core
{
	/// <summary>
	///     The filter object class.
	/// </summary>
	public class FilterObject : ViewModelBase
	{
		/// <summary>
		///     The is filtered
		/// </summary>
		private bool _isFiltered;

		/// <summary>
		///     The value
		/// </summary>
		private object _value;

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterObject" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="displayName">The display name.</param>
		/// <param name="isFiltered">if set to <c>true</c> [is filtered].</param>
		/// <param name="onChangeCallback">The on change callback.</param>
		public FilterObject( object value, string displayName, bool isFiltered, Action onChangeCallback )
		{
			Value = value;
			DisplayName = displayName;
			IsFiltered = isFiltered;

			OnChangeCallback = onChangeCallback;
		}

		/// <summary>
		///     Gets or sets the is filtered.
		/// </summary>
		/// <value>
		///     The is filtered.
		/// </value>
		public bool IsFiltered
		{
			get
			{
				return _isFiltered;
			}
			set
			{
				if ( _isFiltered != value )
				{
					SetProperty( ref _isFiltered, value );

					OnChangeCallback?.Invoke( );
				}
			}
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		/// <summary>
		/// Gets or sets the display name.
		/// </summary>
		/// <value>
		/// The display name.
		/// </value>
		public string DisplayName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the on change callback.
		/// </summary>
		/// <value>
		///     The on change callback.
		/// </value>
		private Action OnChangeCallback
		{
			get;
			set;
		}
	}
}