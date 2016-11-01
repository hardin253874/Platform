// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Windows;
using System.Windows.Controls;
using ReadiMon.Shared.Model;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Field Template Selector
	/// </summary>
	public class FieldTemplateSelector : DataTemplateSelector
	{
		/// <summary>
		///     Gets or sets the entity template.
		/// </summary>
		/// <value>
		///     The entity template.
		/// </value>
		public DataTemplate EntityTemplate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the read only template.
		/// </summary>
		/// <value>
		///     The read only template.
		/// </value>
		public DataTemplate ReadOnlyTemplate
		{
			get;
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
			var field = item as IFieldInfo;

			if ( field == null )
			{
				throw new InvalidCastException( );
			}

			if ( field.IsReadOnly )
			{
				return ReadOnlyTemplate;
			}

			return EntityTemplate;
		}
	}
}