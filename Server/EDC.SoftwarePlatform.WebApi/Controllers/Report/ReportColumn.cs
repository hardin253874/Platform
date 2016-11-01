// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class ReportColumn.
	/// </summary>
	[DataContract]
	public class ReportColumn
	{
		/// <summary>
		///     Gets or sets the ordinal.
		/// </summary>
		/// <value>The ordinal.</value>
		[DataMember( Name = "ord", EmitDefaultValue = true, IsRequired = true )]
		public long Ordinal
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		[DataMember( Name = "title", EmitDefaultValue = false, IsRequired = true )]
		public string Title
		{
			get;
			set;
		}

		/// <summary>
		///		Should the title be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeTitle( )
		{
			return Title != null;
		}

		/// <summary>
		///     Gets or sets the type.
		/// </summary>
		/// <value>The type.</value>
		[DataMember( Name = "type", EmitDefaultValue = false, IsRequired = true )]
		public string Type
		{
			get;
			set;
		}

		/// <summary>
		///		Should the type be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeType( )
		{
			return Type != null;
		}

		/// <summary>
		///     Gets or sets the type of the analyser.
		/// </summary>
		/// <value>The type of the analyser.</value>
		[DataMember( Name = "oprtype", EmitDefaultValue = false, IsRequired = false )]
		public string OperatorType
		{
			get;
			set;
		}

		/// <summary>
		///		Should the type of the operator be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeOperatorType( )
		{
			return OperatorType != null; 
		}

		/// <summary>
		///		Gets or sets a value indicating whether this instance is hidden.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is hidden; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "hide", EmitDefaultValue = false, IsRequired = false )]
		public bool IsHidden
		{
			get;
			set;
		}

		/// <summary>
		/// Should the is hidden value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeIsHidden( )
		{
			return IsHidden; 
		}

		/// <summary>
		///		Gets or sets a value indicating whether this instance is aggregate column.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is aggregate column; otherwise, <c>false</c>.
		/// </value>
        [DataMember(Name = "aggcol", EmitDefaultValue = false, IsRequired = false)]
        public bool IsAggregateColumn
        {
            get;
            set;
        }

		/// <summary>
		///		Should the is aggregate column value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeIsAggregateColumn( )
		{
			return IsAggregateColumn;
		}

		/// <summary>
		///     Gets or sets the field unique identifier.
		/// </summary>
		/// <value>The field unique identifier.</value>
		[DataMember( Name = "fid", EmitDefaultValue = false, IsRequired = false )]
		public long FieldId
		{
			get;
			set;
		}

		/// <summary>
		///		Should the field identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeFieldId( )
		{
			return FieldId != 0;
		}

        /// <summary>
		///     Gets or sets the relationship typeId
		/// </summary>
		/// <value>The relationship type id.</value>
		[DataMember(Name = "rid", EmitDefaultValue = false, IsRequired = false)]
        public long RelationshipTypeId
        {
            get;
            set;
        }

        /// <summary>
        ///		Should the relationship id be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeRelationshipTypeId()
        {
            return RelationshipTypeId != 0;
        }

        /// <summary>
        ///     Gets or sets the whether then relationship is reverse
        /// </summary>
        /// <value>true if reverse</value>
        [DataMember(Name = "rev", EmitDefaultValue = false, IsRequired = false)]
        public bool RelationshipIsReverse
        {
            get;
            set;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeRelationshipIsReverse()
        {
            return RelationshipIsReverse;
        }


        /// <summary>
        ///     Gets or sets the type unique identifier.
        /// </summary>
        /// <value>The type unique identifier.</value>
        [DataMember( Name = "tid", EmitDefaultValue = false, IsRequired = false )]
		public long TypeId
		{
			get;
			set;
		}

		/// <summary>
		///		Should the type identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeTypeId( )
		{
			return TypeId != 0;
		}

		/// <summary>
		///     Gets or sets the minimum length.
		/// </summary>
		/// <value>The minimum length.</value>
		[DataMember( Name = "minlen", EmitDefaultValue = false, IsRequired = false )]
		public long MinimumLength
		{
			get;
			set;
		}

		/// <summary>
		///		Should the minimum length be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeMinimumLength( )
		{
			return MinimumLength != 0;
		}

		/// <summary>
		///     Gets or sets the maximum length.
		/// </summary>
		/// <value>The maximum length.</value>
		[DataMember( Name = "maxlen", EmitDefaultValue = false, IsRequired = false )]
		public long MaximumLength
		{
			get;
			set;
		}

		/// <summary>
		///		Should the maximum length be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeMaximumLength( )
		{
			return MaximumLength != 0;
		}

		/// <summary>
		///     Gets or sets the regular expression.
		/// </summary>
		/// <value>The regular expression.</value>
		[DataMember( Name = "regex", EmitDefaultValue = false, IsRequired = false )]
		public string RegularExpression
		{
			get;
			set;
		}

		/// <summary>
		///		Should the regular expression be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeRegularExpression( )
		{
			return RegularExpression != null;
		}

		/// <summary>
		///     Gets or sets the regular expression error.
		/// </summary>
		/// <value>The regular expression error.</value>
		[DataMember( Name = "regexerr", EmitDefaultValue = false, IsRequired = false )]
		public string RegularExpressionError
		{
			get;
			set;
		}

		/// <summary>
		///		Should the regular expression error be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeRegularExpressionError( )
		{
			return RegularExpressionError != null;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is multi line.
		/// </summary>
		/// <value><c>true</c> if this instance is multi line; otherwise, <c>false</c>.</value>
		[DataMember( Name = "mline", EmitDefaultValue = false, IsRequired = false )]
		public bool IsMultiLine
		{
			get;
			set;
		}

		/// <summary>
		///		Should the is multi line value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeIsMultiLine( )
		{
			return IsMultiLine;
		}

		/// <summary>
		///     Gets or sets the default value.
		/// </summary>
		/// <value>The default value.</value>
		[DataMember( Name = "defval", EmitDefaultValue = false, IsRequired = false )]
		public string DefaultValue
		{
			get;
			set;
		}

		/// <summary>
		///		Should the default value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeDefaultValue( )
		{
			return DefaultValue != null;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is required.
		/// </summary>
		/// <value><c>true</c> if this instance is required; otherwise, <c>false</c>.</value>
		[DataMember( Name = "reqd", EmitDefaultValue = false, IsRequired = false )]
		public bool IsRequired
		{
			get;
			set;
		}

		/// <summary>
		///		Should the is required value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeIsRequired( )
		{
			return IsRequired;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is read only.
		/// </summary>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		[DataMember( Name = "ro", EmitDefaultValue = false, IsRequired = false )]
		public bool IsReadOnly
		{
			get;
			set;
		}

		/// <summary>
		///		Should the is read only value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeIsReadOnly( )
		{
			return IsReadOnly;
		}

		/// <summary>
		///     Gets or sets the decimal places.
		/// </summary>
		/// <value>The decimal places.</value>
		[DataMember( Name = "mindec", EmitDefaultValue = false, IsRequired = false )]
		public decimal MinimumDecimal
		{
			get;
			set;
		}

		/// <summary>
		///		Should the minimum decimal be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeMinimumDecimal( )
		{
			return MinimumDecimal != 0;
		}

		/// <summary>
		///     Gets or sets the maximum decimal.
		/// </summary>
		/// <value>The maximum decimal.</value>
		[DataMember( Name = "maxdec", EmitDefaultValue = false, IsRequired = false )]
		public decimal MaximumDecimal
		{
			get;
			set;
		}

		/// <summary>
		///		Should the maximum decimal be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeMaximumDecimal( )
		{
			return MaximumDecimal != 0;
		}

		/// <summary>
		///     Gets or sets the decimal places.
		/// </summary>
		/// <value>The decimal places.</value>
		[DataMember( Name = "places", EmitDefaultValue = false, IsRequired = false )]
		public long? DecimalPlaces
		{
			get;
			set;
		}

		/// <summary>
		///		Should the decimal places be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeDecimalPlaces( )
		{
			return DecimalPlaces.HasValue;
		}

		/// <summary>
		///     Gets or sets the minimum date.
		/// </summary>
		/// <value>The minimum date.</value>
		[DataMember( Name = "mindate", EmitDefaultValue = false, IsRequired = false )]
		public DateTime? MinimumDate
		{
			get;
			set;
		}

		/// <summary>
		///		Should the minimum date be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeMinimumDate( )
		{
			return MinimumDate.HasValue;
		}

		/// <summary>
		///     Gets or sets the maximum date.
		/// </summary>
		/// <value>The maximum date.</value>
		[DataMember( Name = "maxdate", EmitDefaultValue = false, IsRequired = false )]
		public DateTime? MaximumDate
		{
			get;
			set;
		}

		/// <summary>
		///		Should the maximum date be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeMaximumDate( )
		{
			return MaximumDate.HasValue;
		}

		/// <summary>
		///     Gets or sets the date time format.
		/// </summary>
		/// <value>The date time format.</value>
		[DataMember( Name = "dtfmt", EmitDefaultValue = false, IsRequired = false )]
		public string DateTimeFormat
		{
			get;
			set;
		}

		/// <summary>
		///		Should the date time format be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeDateTimeFormat( )
		{
			return DateTimeFormat != null;
		}

		/// <summary>
		///     Gets or sets the automatic number display pattern for an Auto Number Field.
		/// </summary>
		/// <value>The automatic number display pattern.</value>
		[DataMember( Name = "anpat", EmitDefaultValue = false, IsRequired = false )]
		public string AutoNumberDisplayPattern
		{
			get;
			set;
		}

		/// <summary>
		///		Should the automatic number display pattern be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeAutoNumberDisplayPattern( )
		{
			return AutoNumberDisplayPattern != null;
		}

		/// <summary>
		///     Gets or sets the column prefix.
		/// </summary>
		/// <value>The column prefix.</value>
		[DataMember( Name = "prefix", EmitDefaultValue = false, IsRequired = false )]
		public string ColumnPrefix
		{
			get;
			set;
		}

		/// <summary>
		///		Should the column prefix be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeColumnPrefix( )
		{
			return ColumnPrefix != null;
		}

		/// <summary>
		///     Gets or sets the column suffix.
		/// </summary>
		/// <value>The column suffix.</value>
		[DataMember( Name = "suffix", EmitDefaultValue = false, IsRequired = false )]
		public string ColumnSuffix
		{
			get;
			set;
		}

		/// <summary>
		///		Should the column suffix be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeColumnSuffix( )
		{
			return ColumnSuffix != null;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this report column has conditional formatting.
		/// </summary>
		/// <value><c>true</c> if this report column has conditional formatting; otherwise, <c>false</c>.</value>
		[DataMember( Name = "condfmt", EmitDefaultValue = false, IsRequired = false )]
		public bool HasConditionalFormatting
		{
			get;
			set;
		}

		/// <summary>
		///		Should the has conditional formatting value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeHasConditionalFormatting( )
		{
			return HasConditionalFormatting;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this column is the entity name field for the primary entity for the report.
		/// </summary>
		/// <value><c>true</c> if this column is the entity name field; otherwise, <c>false</c>.</value>
		[DataMember( Name = "entityname", EmitDefaultValue = false, IsRequired = false )]
		public bool EntityNameField
		{
			get;
			set;
		}

		/// <summary>
		///		Should the entity name field be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeEntityNameField( )
		{
			return EntityNameField;
		}

		/// <summary>
		///     Gets or sets a value indicating the cardinality for relationship types.
		/// </summary>
		/// <value>
		///     The cardinality.
		/// </value>
		[DataMember( Name = "card", EmitDefaultValue = false, IsRequired = false )]
		public string Cardinality
		{
			get;
			set;
		}

		/// <summary>
		///		Should the cardinality be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeCardinality( )
		{
			return Cardinality != null;
		}

        /// <summary>
        /// Gets or sets any error message associated with the column.
        /// </summary>
        [DataMember( Name = "colerr", EmitDefaultValue = false, IsRequired = false )]
        public string ColumnError
        {
            get;
            set;
        }

		/// <summary>
		///		Should the column error be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeColumnError( )
		{
			return ColumnError != null;
		}
	}
}