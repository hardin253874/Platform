// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Database;

namespace EDC.ReadiNow.Model.FieldTypes
{
	/// <summary>
	///     Extend the generated GuidField class with behaviors.
	/// </summary>
    public class GuidFieldHelper : IFieldHelper
	{
		private GuidField _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal GuidFieldHelper(GuidField fieldEntity)
        {
            _field = fieldEntity;
        }

		/// <summary>
		///     Converts a configuration XML field value string to an instance of its .Net type.
		/// </summary>
		public object ConvertXmlStringToObject( string xml )
		{
			return Guid.Parse( xml );
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
				return string.Format( "This field is required." );
			}

			return null;
		}

		/// <summary>
		///     Converts the type of to database.
		/// </summary>
		public DatabaseType ConvertToDatabaseType( )
		{
            return DatabaseType.GuidType;
		}

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public DataType ConvertToDataType()
        {
            return DataType.Guid;
        }
	}
}
