// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;

/////
// ReSharper disable CheckNamespace
/////

namespace EDC.ReadiNow.Model.FieldTypes
{
	/// <summary>
	///     Extend the generated AliasField class with behaviors.
	/// </summary>
    public class AliasFieldHelper : IFieldHelper
	{
        private AliasField _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal AliasFieldHelper(AliasField fieldEntity)
        {
            _field = fieldEntity;
        }

		/// <summary>
		///     Converts a configuration XML field value string to an instance of its .Net type.
		/// </summary>
		public object ConvertXmlStringToObject( string xml )
		{
			return xml;
		}

		/// <summary>
		///     Ensures that constraints for this field are satisfied.
		/// </summary>
		/// <returns>
		///     Null if the value is OK, otherwise an error message.
		/// </returns>
		public string ValidateFieldValue( object value )
		{
			var sValue = ( string ) value;

			if ( _field.IsRequired == true && string.IsNullOrEmpty( sValue ) )
			{
				return "This field is required.";
			}

			return null;
		}

		/// <summary>
		///     Converts the type of to database.
		/// </summary>
		public DatabaseType ConvertToDatabaseType( )
		{
		    return DatabaseType.StringType;
		}

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public DataType ConvertToDataType()
        {
            return DataType.String;
        }
	}
}

/////
// ReSharper restore CheckNamespace
/////