// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace TenantDiffTool.SupportClasses.Diff
{
	/// <summary>
	///     Entity field info.
	/// </summary>
	public class EntityFieldInfo : FieldInfo
	{
		/// <summary>
		///     Category.
		/// </summary>
		private readonly CategoryAttribute _category;

		/// <summary>
		///     Description.
		/// </summary>
		private readonly DescriptionAttribute _descrition;

		/// <summary>
		///     Display Name.
		/// </summary>
		private readonly DisplayNameAttribute _displayName;

		/// <summary>
		///     Name.
		/// </summary>
		private readonly string _name;

		/// <summary>
		///     Value.
		/// </summary>
		private object _value;

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityFieldInfo" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="category">The category.</param>
		/// <param name="displayName">The display name.</param>
		/// <param name="description">The description.</param>
		public EntityFieldInfo( string name, string category, string displayName, string description )
		{
			_name = name;
			_category = new CategoryAttribute( category );
			_displayName = new DisplayNameAttribute( displayName );
			_descrition = new DescriptionAttribute( description );
		}

		/// <summary>
		///     Gets the attributes associated with this field.
		/// </summary>
		/// <returns>The FieldAttributes for this field.</returns>
		public override FieldAttributes Attributes => FieldAttributes.Public;

		/// <summary>
		///     Gets the class that declares this member.
		/// </summary>
		/// <returns>The Type object for the class that declares this member.</returns>
		public override Type DeclaringType => typeof( Entity );

		/// <summary>
		///     Gets a RuntimeFieldHandle, which is a handle to the internal metadata representation of a field.
		/// </summary>
		/// <returns>A handle to the internal metadata representation of a field.</returns>
		public override RuntimeFieldHandle FieldHandle => new RuntimeFieldHandle( );

		/// <summary>
		///     Gets the type of this field object.
		/// </summary>
		/// <returns>The type of this field object.</returns>
		public override Type FieldType => _value.GetType( );

		/// <summary>
		///     Gets the name of the current member.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.String" /> containing the name of this member.
		/// </returns>
		public override string Name => _name;

		/// <summary>
		///     Gets the class object that was used to obtain this instance of MemberInfo.
		/// </summary>
		/// <returns>The Type object through which this MemberInfo object was obtained.</returns>
		public override Type ReflectedType => typeof( Entity );

		/// <summary>
		///     When overridden in a derived class, returns an array of custom attributes applied to this member and identified by
		///     <see
		///         cref="T:System.Type" />
		///     .
		/// </summary>
		/// <param name="attributeType">
		///     The type of attribute to search for. Only attributes that are assignable to this type are
		///     returned.
		/// </param>
		/// <param name="inherit">
		///     true to search this member's inheritance chain to find the attributes; otherwise, false. This
		///     parameter is ignored for properties and events; see Remarks.
		/// </param>
		/// <returns>
		///     An array of custom attributes applied to this member, or an array with zero elements if no attributes assignable to
		///     <paramref
		///         name="attributeType" />
		///     have been applied.
		/// </returns>
		public override object[ ] GetCustomAttributes( Type attributeType, bool inherit )
		{
			var attributes = new List<Attribute>( );

			if ( inherit == false )
			{
				if ( attributeType == typeof( CategoryAttribute ) )
				{
					attributes.Add( _category );
				}
				else if ( attributeType == typeof( DisplayNameAttribute ) )
				{
					attributes.Add( _displayName );
				}
				else if ( attributeType == typeof( DescriptionAttribute ) )
				{
					attributes.Add( _descrition );
				}
			}
			else
			{
				if ( attributeType.IsAssignableFrom( typeof( CategoryAttribute ) ) )
				{
					attributes.Add( _category );
				}

				if ( attributeType.IsAssignableFrom( typeof( DisplayNameAttribute ) ) )
				{
					attributes.Add( _displayName );
				}

				if ( attributeType.IsAssignableFrom( typeof( DescriptionAttribute ) ) )
				{
					attributes.Add( _descrition );
				}
			}

// ReSharper disable CoVariantArrayConversion
			return attributes.ToArray( );
// ReSharper restore CoVariantArrayConversion
		}

		/// <summary>
		///     When overridden in a derived class, returns an array of all custom attributes applied to this member.
		/// </summary>
		/// <param name="inherit">
		///     true to search this member's inheritance chain to find the attributes; otherwise, false. This
		///     parameter is ignored for properties and events; see Remarks.
		/// </param>
		/// <returns>
		///     An array that contains all the custom attributes applied to this member, or an array with zero elements if no
		///     attributes are defined.
		/// </returns>
		public override object[ ] GetCustomAttributes( bool inherit )
		{
			return new object[ ]
			{
				_category,
				_displayName,
				_descrition
			};
		}

		/// <summary>
		///     When overridden in a derived class, returns the value of a field supported by a given object.
		/// </summary>
		/// <param name="obj">The object whose field value will be returned.</param>
		/// <returns>
		///     An object containing the value of the field reflected by this instance.
		/// </returns>
		public override object GetValue( object obj )
		{
			return _value;
		}

		/// <summary>
		///     When overridden in a derived class, indicates whether one or more attributes of the specified type or of its
		///     derived types is applied to this member.
		/// </summary>
		/// <param name="attributeType">The type of custom attribute to search for. The search includes derived types.</param>
		/// <param name="inherit">
		///     true to search this member's inheritance chain to find the attributes; otherwise, false. This
		///     parameter is ignored for properties and events; see Remarks.
		/// </param>
		/// <returns>
		///     true if one or more instances of <paramref name="attributeType" /> or any of its derived types is applied to this
		///     member; otherwise, false.
		/// </returns>
		public override bool IsDefined( Type attributeType, bool inherit )
		{
			return attributeType == typeof( CategoryAttribute ) ||
			       attributeType == typeof( DisplayNameAttribute ) ||
			       attributeType == typeof( DescriptionAttribute );
		}

		/// <summary>
		///     When overridden in a derived class, sets the value of the field supported by the given object.
		/// </summary>
		/// <param name="obj">The object whose field value will be set.</param>
		/// <param name="value">The value to assign to the field.</param>
		/// <param name="invokeAttr">
		///     A field of Binder that specifies the type of binding that is desired (for example,
		///     Binder.CreateInstance or Binder.ExactBinding).
		/// </param>
		/// <param name="binder">
		///     A set of properties that enables the binding, coercion of argument types, and invocation of members through
		///     reflection. If
		///     <paramref
		///         name="binder" />
		///     is null, then Binder.DefaultBinding is used.
		/// </param>
		/// <param name="culture">The software preferences of a particular culture.</param>
		public override void SetValue( object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture )
		{
			_value = value;
		}
	}
}