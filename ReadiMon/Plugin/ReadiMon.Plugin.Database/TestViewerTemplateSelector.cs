// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Windows;
using System.Windows.Controls;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	/// </summary>
	public class TestViewerTemplateSelector : DataTemplateSelector
	{
		/// <summary>
		///     Gets or sets the entity template.
		/// </summary>
		/// <value>
		///     The entity template.
		/// </value>
		public DataTemplate EntityTemplate
		{
			private get;
			set;
		}

		/// <summary>
		///     Gets or sets the index.
		/// </summary>
		/// <value>
		///     The index.
		/// </value>
		public int Index
		{
			private get;
			set;
		}

		/// <summary>
		///     Gets or sets the static  template.
		/// </summary>
		/// <value>
		///     The static template.
		/// </value>
		public DataTemplate StaticTemplate
		{
			private get;
			set;
		}

		/// <summary>
		///     When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate" /> based on custom logic.
		/// </summary>
		/// <param name="item">The data object for which to select the template.</param>
		/// <param name="container">The data-bound object.</param>
		/// <returns>
		///     Returns a <see cref="T:System.Windows.DataTemplate" /> or null. The default value is null.
		/// </returns>
		/// <exception cref="System.InvalidCastException"></exception>
		public override DataTemplate SelectTemplate( object item, DependencyObject container )
		{
			var row = item as FailureRow;

			if ( row == null )
			{
				throw new InvalidCastException( );
			}

			if ( row.EntityColumns.Contains( Index ) )
			{
				return EntityTemplate;
			}

			return StaticTemplate;
		}
	}
}