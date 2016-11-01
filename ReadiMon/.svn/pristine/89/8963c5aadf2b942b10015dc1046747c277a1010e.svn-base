// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Windows;
using System.Windows.Controls;
using ReadiMon.Shared.Model;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     EntityTemplate selector
	/// </summary>
	public class EntityTemplateSelector : DataTemplateSelector
	{
		/// <summary>
		///     Gets or sets the alias template.
		/// </summary>
		/// <value>
		///     The alias template.
		/// </value>
		public DataTemplate AliasTemplate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the bit template.
		/// </summary>
		/// <value>
		///     The bit template.
		/// </value>
		public DataTemplate BitTemplate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the date time template.
		/// </summary>
		/// <value>
		///     The date time template.
		/// </value>
		public DataTemplate DateTimeTemplate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the decimal template.
		/// </summary>
		/// <value>
		///     The decimal template.
		/// </value>
		public DataTemplate DecimalTemplate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the unique identifier template.
		/// </summary>
		/// <value>
		///     The unique identifier template.
		/// </value>
		public DataTemplate GuidTemplate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the integer template.
		/// </summary>
		/// <value>
		///     The integer template.
		/// </value>
		public DataTemplate IntegerTemplate
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
		///     Gets or sets the string template.
		/// </summary>
		/// <value>
		///     The string template.
		/// </value>
		public DataTemplate StringTemplate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the XML template.
		/// </summary>
		/// <value>
		///     The XML template.
		/// </value>
		public DataTemplate XmlTemplate
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

			if ( field is DataAliasInfo )
			{
				return AliasTemplate;
			}

			if ( field is DataBitInfo )
			{
				return BitTemplate;
			}

			if ( field is DataDateTimeInfo )
			{
				return DateTimeTemplate;
			}

			if ( field is DataDecimalInfo )
			{
				return DecimalTemplate;
			}

			if ( field is DataGuidInfo )
			{
				return GuidTemplate;
			}

			if ( field is DataNVarCharInfo )
			{
				return StringTemplate;
			}

			if ( field is DataXmlInfo )
			{
				return StringTemplate;
			}

			if ( field is FieldInfo<long> )
			{
				return ReadOnlyTemplate;
			}

			return StringTemplate;
		}
	}
}