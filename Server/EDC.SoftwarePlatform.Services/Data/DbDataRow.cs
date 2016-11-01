// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.Services.Data
{
	/// <summary>
	///     Defines a row of data within a data table.
	/// </summary>
	[DataContract]
	public class DbDataRow
	{
		private Dictionary<int, DbDataField> _fieldbyOrdinal = new Dictionary<int, DbDataField>( );
		private DbDataFieldDictionary _fields = new DbDataFieldDictionary( );

		/// <summary>
		///     Gets or sets the fields contained within the row.
		/// </summary>
		[DataMember]
		public DbDataFieldDictionary Fields
		{
			get
			{
				return _fields;
			}

			set
			{
				_fields = value;
			}
		}

		/// <summary>
		///     Gets or sets the fields contained within the row.
		/// </summary>
		[DataMember]
		public Dictionary<int, DbDataField> FieldsByOrdinal
		{
			get
			{
				return _fieldbyOrdinal;
			}

			set
			{
				_fieldbyOrdinal = value;
			}
		}
	}
}