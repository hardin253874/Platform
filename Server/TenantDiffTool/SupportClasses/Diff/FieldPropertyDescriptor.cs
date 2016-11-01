// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.ComponentModel;
using System.Reflection;

namespace TenantDiffTool.SupportClasses.Diff
{
	/// <summary>
	///     Field Property Descriptor.
	/// </summary>
	public class FieldPropertyDescriptor : PropertyDescriptor
	{
		/// <summary>
		///     Field.
		/// </summary>
		private readonly FieldInfo _field;

		/// <summary>
		///     Initializes a new instance of the <see cref="FieldPropertyDescriptor" /> class.
		/// </summary>
		/// <param name="field">The field.</param>
		public FieldPropertyDescriptor( FieldInfo field )
			: base( field.Name,
				( Attribute[ ] ) field.GetCustomAttributes( typeof( Attribute ), true ) )
		{
			_field = field;
		}

		/// <summary>
		///     When overridden in a derived class, gets the type of the component this property is bound to.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Type" /> that represents the type of component this property is bound to. When the
		///     <see
		///         cref="M:System.ComponentModel.PropertyDescriptor.GetValue(System.Object)" />
		///     or
		///     <see
		///         cref="M:System.ComponentModel.PropertyDescriptor.SetValue(System.Object,System.Object)" />
		///     methods are invoked, the object specified might be an instance of this type.
		/// </returns>
		public override Type ComponentType
		{
			get
			{
				return _field.DeclaringType;
			}
		}

		/// <summary>
		///     Gets the field.
		/// </summary>
		/// <value>
		///     The field.
		/// </value>
		public FieldInfo Field
		{
			get
			{
				return _field;
			}
		}

		/// <summary>
		///     When overridden in a derived class, gets a value indicating whether this property is read-only.
		/// </summary>
		/// <returns>true if the property is read-only; otherwise, false.</returns>
		public override bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		///     When overridden in a derived class, gets the type of the property.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Type" /> that represents the type of the property.
		/// </returns>
		public override Type PropertyType
		{
			get
			{
				return _field.FieldType;
			}
		}

		/// <summary>
		///     When overridden in a derived class, returns whether resetting an object changes its value.
		/// </summary>
		/// <param name="component">The component to test for reset capability.</param>
		/// <returns>
		///     true if resetting the component changes its value; otherwise, false.
		/// </returns>
		public override bool CanResetValue( object component )
		{
			return false;
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">
		///     The <see cref="System.Object" /> to compare with this instance.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var other = obj as FieldPropertyDescriptor;
			return other != null && other._field.Equals( _field );
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			return _field.GetHashCode( );
		}

		/// <summary>
		///     When overridden in a derived class, gets the current value of the property on a component.
		/// </summary>
		/// <param name="component">The component with the property for which to retrieve the value.</param>
		/// <returns>
		///     The value of a property for a given component.
		/// </returns>
		public override object GetValue( object component )
		{
			return _field.GetValue( component );
		}

		/// <summary>
		///     When overridden in a derived class, resets the value for this property of the component to the default value.
		/// </summary>
		/// <param name="component">The component with the property value that is to be reset to the default value.</param>
		public override void ResetValue( object component )
		{
		}

		/// <summary>
		///     When overridden in a derived class, sets the value of the component to a different value.
		/// </summary>
		/// <param name="component">The component with the property value that is to be set.</param>
		/// <param name="value">The new value.</param>
		public override void SetValue( object component, object value )
		{
			_field.SetValue( component, value );
			OnValueChanged( component, EventArgs.Empty );
		}

		/// <summary>
		///     When overridden in a derived class, determines a value indicating whether the value of this property needs to be
		///     persisted.
		/// </summary>
		/// <param name="component">The component with the property to be examined for persistence.</param>
		/// <returns>
		///     true if the property should be persisted; otherwise, false.
		/// </returns>
		public override bool ShouldSerializeValue( object component )
		{
			return true;
		}
	}
}