// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace EDC.SoftwarePlatform.WebApi.Test.Infrastructure
{
	/// <summary>
	///     JSON Property Constraint
	/// </summary>
	public class JsonPropertyConstraint : PrefixConstraint
	{
		/// <summary>
		///     The property name
		/// </summary>
		private readonly string _name;

		/// <summary>
		///     The property value
		/// </summary>
		private object _propValue;

		/// <summary>
		///     Initializes a new instance of the <see cref="T:PropertyConstraint" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="baseConstraint">The constraint to apply to the property.</param>
		public JsonPropertyConstraint( string name, Constraint baseConstraint )
			: base( baseConstraint )
		{
			_name = name;
		}

		/// <summary>
		///     Returns the string representation of the constraint.
		/// </summary>
		/// <returns />
		protected override string GetStringRepresentation( )
		{
			return string.Format( "<property {0} {1}>", _name, baseConstraint );
		}

		/// <summary>
		/// Test whether the constraint is satisfied by a given value
		/// </summary>
		/// <param name="propertyValue">The property value.</param>
		/// <returns>
		/// True for success, false for failure
		/// </returns>
		/// <exception cref="System.ArgumentException">name</exception>
		public override bool Matches( object propertyValue )
		{
			actual = propertyValue;

			Guard.ArgumentNotNull( propertyValue, "actual" );

			PropertyInfo property = ( propertyValue as Type ?? propertyValue.GetType( ) ).GetProperty( _name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty );

			if ( property == null )
			{
				throw new ArgumentException( string.Format( "Property {0} was not found", _name ), "name" );
			}

			_propValue = property.GetValue( propertyValue, null );

			Type type = _propValue.GetType( );

			if ( type.Name == "JsonObject" )
			{
				var jsonTypeField = type.GetField( "Type", BindingFlags.NonPublic | BindingFlags.Instance );

				if ( jsonTypeField != null )
				{
					var value = jsonTypeField.GetValue( _propValue ).ToString( );

					switch ( value )
					{
						case "String":
							_propValue = _propValue.ToString( ).TrimStart( '\"' ).TrimEnd( '\"' );
							break;
						case "True":
							_propValue = true;
							break;
						case "False":
							_propValue = false;
							break;
						default:
							_propValue = _propValue.ToString( );
							break;
					}
				}
			}

			return baseConstraint.Matches( _propValue );
		}

		/// <summary>
		///     Write the actual value for a failing constraint test to a
		///     MessageWriter. The default implementation simply writes
		///     the raw value of actual, leaving it to the writer to
		///     perform any formatting.
		/// </summary>
		/// <param name="writer">The writer on which the actual value is displayed</param>
		public override void WriteActualValueTo( MessageWriter writer )
		{
			writer.WriteActualValue( _propValue );
		}

		/// <summary>
		///     Write the constraint description to a MessageWriter
		/// </summary>
		/// <param name="writer">The writer on which the description is displayed</param>
		public override void WriteDescriptionTo( MessageWriter writer )
		{
			writer.WritePredicate( "property " + _name );

			if ( baseConstraint == null )
			{
				return;
			}

			if ( baseConstraint is EqualConstraint )
			{
				writer.WritePredicate( "equal to" );
			}

			baseConstraint.WriteDescriptionTo( writer );
		}
	}
}