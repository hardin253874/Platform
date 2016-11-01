// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;
using EDC.ReadiNow.Resources;

namespace EDC.ReadiNow.Model.FieldTypes
{
	/// <summary>
	///     Extend the generated IntField class with behaviors.
	/// </summary>
    public class IntFieldHelper : IFieldHelper
	{
		private IntField _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal IntFieldHelper(IntField fieldEntity)
        {
            _field = fieldEntity;
        }

		// Under no circumstances should any logic that applies to the 'base' be placed in these methods.
		// It won't get called if the user is holding onto an instance of a concrete type.

		/// <summary>
		///     Converts a config XML field value string to an instance of its .Net type.
		/// </summary>
		public object ConvertXmlStringToObject( string xml )
		{
			return int.Parse( xml );
		}

		/// <summary>
		///     Ensures that constraints for this field are satisfied.
		/// </summary>
		/// <returns>
		///     Null if the value is OK, otherwise an error message.
		/// </returns>
		public string ValidateFieldValue( object value )
		{
			if ( value == null )
			{
				if ( _field.IsRequired == true )
				{
					return string.Format( GlobalStrings.MandatoryMessageTextFormat, _field.Name );
				}

				return null;
			}

			//TODO: Add correct handling of default value
			var iValue = ( int ) value;

			if ( _field.MaxInt != null && iValue > _field.MaxInt )
			{
				return string.Format( GlobalStrings.MaximumValueMessageTextFormat, _field.Name, _field.MaxInt );
			}

			if ( _field.MinInt != null && iValue < _field.MinInt )
			{
				return string.Format( GlobalStrings.MinimumValueMessageTextFormat, _field.Name, _field.MinInt );
			}

			return null;
		}

		/// <summary>
		///     Converts the type of to database.
		/// </summary>
		public DatabaseType ConvertToDatabaseType( )
		{
		    return DatabaseType.Int32Type;
		}

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public DataType ConvertToDataType()
        {
            return DataType.Int32;
        }
	}
}
