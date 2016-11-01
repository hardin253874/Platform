// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;
using EDC.ReadiNow.Resources;

/////
// ReSharper disable CheckNamespace
/////

namespace EDC.ReadiNow.Model.FieldTypes
{
	/// <summary>
	///     Extend the generated BoolField class with behaviors.
	/// </summary>
    public class BoolFieldHelper : IFieldHelper
	{
		private BoolField _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal BoolFieldHelper(BoolField fieldEntity)
        {
            _field = fieldEntity;
        }

		/// <summary>
		///     Converts a configuration XML field value string to an instance of its .Net type.
		/// </summary>
		public object ConvertXmlStringToObject( string xml )
		{
			return bool.Parse( xml );
		}

		/// <summary>
		///     Ensures that constraints for this field are satisfied.
		/// </summary>
		/// <returns>
		///     Null if the value is OK, otherwise an error message.
		/// </returns>
		public string ValidateFieldValue( object value )
		{
			if ( _field.IsRequired == true && value == null )
			{
				return string.Format( GlobalStrings.MandatoryMessageTextFormat, _field.Name );
			}

			return null;
		}


		/// <summary>
		///     Converts the type of to database.
		/// </summary>
		public DatabaseType ConvertToDatabaseType( )
		{
            return DatabaseType.BoolType;
        }

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public DataType ConvertToDataType()
        {
            return DataType.Bool;
        }
	}
}

/////
// ReSharper restore CheckNamespace
/////