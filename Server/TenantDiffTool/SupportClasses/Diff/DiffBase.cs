// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.ComponentModel;
using System.Reflection;
using TenantDiffTool.Core;

namespace TenantDiffTool.SupportClasses.Diff
{
	/// <summary>
	///     DiffBase class.
	/// </summary>
	public abstract class DiffBase : ICustomTypeDescriptor
	{
		/// <summary>
		///     Filter cache
		/// </summary>
		private FilterCache _filterCache;

		/// <summary>
		///     Property cache.
		/// </summary>
		private PropertyDescriptorCollection _propCache;

		/// <summary>
		///     Gets or sets the source.
		/// </summary>
		/// <value>
		///     The source.
		/// </value>
		[Browsable( false )]
		public ISource Source
		{
			get;
			set;
		}

		/// <summary>
		///     Returns a collection of custom attributes for this instance of a component.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.ComponentModel.AttributeCollection" /> containing the attributes for this object.
		/// </returns>
		public AttributeCollection GetAttributes( )
		{
			return TypeDescriptor.GetAttributes( this, true );
		}

		/// <summary>
		///     Returns the class name of this instance of a component.
		/// </summary>
		/// <returns>
		///     The class name of the object, or null if the class does not have a name.
		/// </returns>
		public string GetClassName( )
		{
			return TypeDescriptor.GetClassName( this, true );
		}

		/// <summary>
		///     Returns the name of this instance of a component.
		/// </summary>
		/// <returns>
		///     The name of the object, or null if the object does not have a name.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public string GetComponentName( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Returns a type converter for this instance of a component.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.ComponentModel.TypeConverter" /> that is the converter for this object, or null if there is
		///     no
		///     <see
		///         cref="T:System.ComponentModel.TypeConverter" />
		///     for this object.
		/// </returns>
		public TypeConverter GetConverter( )
		{
			return new TypeConverter( );
		}

		/// <summary>
		///     Returns the default event for this instance of a component.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.ComponentModel.EventDescriptor" /> that represents the default event for this object, or
		///     null if this object does not have events.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public EventDescriptor GetDefaultEvent( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Returns the default property for this instance of a component.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.ComponentModel.PropertyDescriptor" /> that represents the default property for this object,
		///     or null if this object does not have properties.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public PropertyDescriptor GetDefaultProperty( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Returns an editor of the specified type for this instance of a component.
		/// </summary>
		/// <param name="editorBaseType">
		///     A <see cref="T:System.Type" /> that represents the editor for this object.
		/// </param>
		/// <returns>
		///     An <see cref="T:System.Object" /> of the specified type that is the editor for this object, or null if the editor
		///     cannot be found.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public object GetEditor( Type editorBaseType )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Returns the events for this instance of a component using the specified attribute array as a filter.
		/// </summary>
		/// <param name="attributes">
		///     An array of type <see cref="T:System.Attribute" /> that is used as a filter.
		/// </param>
		/// <returns>
		///     An <see cref="T:System.ComponentModel.EventDescriptorCollection" /> that represents the filtered events for this
		///     component instance.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public EventDescriptorCollection GetEvents( Attribute[ ] attributes )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Returns the events for this instance of a component.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.ComponentModel.EventDescriptorCollection" /> that represents the events for this component
		///     instance.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public EventDescriptorCollection GetEvents( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Returns the properties for this instance of a component using the attribute array as a filter.
		/// </summary>
		/// <param name="attributes">
		///     An array of type <see cref="T:System.Attribute" /> that is used as a filter.
		/// </param>
		/// <returns>
		///     A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> that represents the filtered properties for
		///     this component instance.
		/// </returns>
		public PropertyDescriptorCollection GetProperties( Attribute[ ] attributes )
		{
			bool filtering = attributes != null && attributes.Length > 0;
			PropertyDescriptorCollection props = _propCache;
			FilterCache cache = _filterCache;

			// Use a cached version if possible
			if ( filtering && cache != null && cache.IsValid( attributes ) )
			{
				return cache.FilteredProperties;
			}

			if ( !filtering && props != null )
			{
				return props;
			}

			// Create the property collection and filter
			props = new PropertyDescriptorCollection( null );
			foreach ( PropertyDescriptor prop in
				TypeDescriptor.GetProperties(
					this, attributes, true ) )
			{
				props.Add( prop );
			}
			foreach ( FieldInfo field in GetType( ).GetFields( ) )
			{
				var fieldDesc =
					new FieldPropertyDescriptor( field );
				if ( !filtering ||
				     fieldDesc.Attributes.Contains( attributes ) )
				{
					props.Add( fieldDesc );
				}
			}

			// Store the computed properties
			if ( filtering )
			{
				cache = new FilterCache
				{
					Attributes = attributes,
					FilteredProperties = props
				};
				_filterCache = cache;
			}
			else
			{
				_propCache = props;
			}

			GetData( props );

			return props;
		}

		/// <summary>
		///     Returns the properties for this instance of a component.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> that represents the properties for this
		///     component instance.
		/// </returns>
		public PropertyDescriptorCollection GetProperties( )
		{
			return ( ( ICustomTypeDescriptor ) this ).GetProperties( null );
		}

		/// <summary>
		///     Returns an object that contains the property described by the specified property descriptor.
		/// </summary>
		/// <param name="pd">
		///     A <see cref="T:System.ComponentModel.PropertyDescriptor" /> that represents the property whose owner is to be
		///     found.
		/// </param>
		/// <returns>
		///     An <see cref="T:System.Object" /> that represents the owner of the specified property.
		/// </returns>
		public object GetPropertyOwner( PropertyDescriptor pd )
		{
			return this;
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <param name="props">The props.</param>
		protected virtual void GetData( PropertyDescriptorCollection props )
		{
		}
	}
}