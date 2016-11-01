// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Services.Query
{
	/// <summary>
	///     Additional column information that's not already captured in the data table or the report data view.
	/// </summary>
	[DataContract]
	public class GridResultColumn
	{
		/// <summary>
		///     Extra information if the column is string type.
		/// </summary>
		[DataMember]
		public bool AllowMultiline
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the auto number display pattern.
		/// </summary>
		/// <value>
		///     The auto number display pattern.
		/// </value>
		[DataMember]
		public string AutoNumberDisplayPattern
		{
			get;
			set;
		}

		/// <summary>
		///     Extra information for currency, decimal value, return the decimal places value
		/// </summary>
		[DataMember]
		public int? DecimalPlaces
		{
			get;
			set;
		}

		/// <summary>
		///     The ID of the field being displayed - if this column is an unmodified data field.
		/// </summary>
		[DataMember]
		public EntityRef FieldId
		{
			get;
			set;
		}

		/// <summary>
		///     The id of the column that has the image id.
		/// </summary>
		/// <value>
		///     The image id column id.
		/// </value>
		[DataMember]
		public Guid? ImageIdColumnId
		{
			get;
			set;
		}

		/// <summary>
		///     A resource column, aside from the root one
		/// </summary>
		[DataMember]
		public bool IsRelatedResourceColumn
		{
			get;
			set;
		}

		/// <summary>
		///     If true, indicates that this is a resource expression column and that the column data is XML encoded.
		///     Example: &lt;e id="1234" text="Resource name" /&gt;
		/// </summary>
		[DataMember]
		public bool IsResourceColumn
		{
			get;
			set;
		}

		/// <summary>
		///     The Id of the column in the resource table that provides this column.
		/// </summary>
		[DataMember]
		public Guid QueryColumnId
		{
			get;
			set;
		}

		/// <summary>
		///     The alias of the resource table that provides this column.
		/// </summary>
		[DataMember]
		public string QueryTableAlias
		{
			get;
			set;
		}

		/// <summary>
		///     If this is a relationship field column, this contains the relationship field Id.
		/// </summary>
		[DataMember]
		public EntityRef RelationshipId
		{
			get;
			set;
		}

		/// <summary>
		///     If this is a structure view column, this contains the structure view Id.
		/// </summary>
		[DataMember]
		public EntityRef StructureViewId
		{
			get;
			set;
		}

		/// <summary>
		///     Additional information used when column is displaying an image.
		/// </summary>
		[DataMember]
		public ThumbnailDetails ThumbnailDetails
		{
			get;
			set;
		}
	}
}