// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;

namespace EDC.ReadiNow.Model.FieldTypes
{
	/// <summary>
	///     Extend the generated XmlField class with behaviors.
	/// </summary>
    public class XmlFieldHelper : IFieldHelper
	{
		private XmlField _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal XmlFieldHelper(XmlField fieldEntity)
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

            if (_field.IsRequired == true && string.IsNullOrEmpty(sValue))
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
            return DatabaseType.XmlType;
        }

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public DataType ConvertToDataType()
        {
            return DataType.Xml;
        }
	}
}
