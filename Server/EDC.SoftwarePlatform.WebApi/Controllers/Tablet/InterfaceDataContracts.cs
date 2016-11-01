// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;

//
// This file contains all of the data definitions used by the mobile web services for both tablet and smart phone.
//
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Tablet
{

    #region Edit Form Definition (and optional data with relationships)

    [DataContract]
    public class Selection
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [DataMember(Name = "id", EmitDefaultValue = false, IsRequired = true)]
        public long Id { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeId( )
	    {
			return Id != 0;
	    }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        [DataMember(Name = "title", EmitDefaultValue = false, IsRequired = true)]
        public string Title { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTitle( )
	    {
			return Title != null;
	    }
    }

    /// <summary>
    /// Defines the control that is to be displayed on the screen.
    /// </summary>
    /// <remarks>
    /// This control must exist on the mobile device application. In the event it does not exist it is removed.
    /// </remarks>
    [DataContract]
    public class Control
    {
        /// <summary>
        /// Gets or sets the id of the control instance
        /// </summary>
        /// <value>The identifier relative to the associated screen.</value>
        /// <remarks>
        /// This identifier is used to associated the data elements received or sent to the host with it's owning control.
        /// </remarks>
        [DataMember(Name = "id", EmitDefaultValue = false, IsRequired = true)]
        public long Id { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeId( )
	    {
			return Id != 0;
	    }

        /// <summary>
        /// Gets or sets the title that is displayed with the control.
        /// </summary>
        /// <value>The localised title string.</value>
        /// <remarks></remarks>
        [DataMember(Name = "title", EmitDefaultValue = false, IsRequired = true)]
        public string Title { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTitle( )
	    {
			return Title != null;
	    }

        [DataMember(Name = "hint", EmitDefaultValue = false, IsRequired = false)]
        public string Hint { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeHint( )
	    {
			return Hint != null;
	    }

        /// <summary>
        /// Gets or sets the type of the control.
        /// </summary>
        /// <value>The control type.</value>
        /// <remarks>
        /// This is the class name of the Objective-C control. If this control is not found in the 
        /// application then the entire control definition is ignored.
        /// </remarks>
        /// <example>
        /// GITextControl
        /// </example>
        [DataMember(Name = "type", EmitDefaultValue = false, IsRequired = true)]
        public string Type { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeType( )
	    {
			return Type != null;
	    }

        /// <summary>
        /// Gets or sets a value indicating whether this field is required.
        /// </summary>
        /// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
        /// <remarks>This mandates that a given field is required to be populated</remarks>
        [DataMember(Name = "reqd", EmitDefaultValue = false, IsRequired = false)]
        public bool Required { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeRequired( )
	    {
			return true;
	    }

        [DataMember(Name = "ro", EmitDefaultValue = false, IsRequired = false)]
        public bool ReadOnly { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeReadOnly( )
	    {
			return true;
	    }

        [DataMember(Name = "defval", EmitDefaultValue = false, IsRequired = false)]
        public string DefaultValue { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDefaultValue( )
	    {
			return DefaultValue != null;
	    }


        
        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        /// <value>The minimum allowable value.</value>
        /// <remarks>
        /// This specifies a minimum value in context to it's data type. 
        /// For example if the associated value is a string then it's length, 
        /// if it's a numeric then it's the value specified as a string (i.e. int.ToString()), 
        /// if it's a date then the specification contains the date string in UTC format, 
        /// a time then it's the time string specified based on a 24-hour clock and so on. 
        /// </remarks>
        [DataMember(Name = "min", EmitDefaultValue = false, IsRequired = false)]
        public string Minimum { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeMinimum( )
	    {
			return Minimum != null;
	    }

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>The maximum allowable value.</value>
        /// <remarks>
        /// This specifies a maximum value in context to the associated data type. 
        /// For example if the associated value is a string then it's length, 
        /// if it's a numeric then it's the value specified as a string (i.e. int.ToString()), 
        /// if it's a date then the specification contains the date string in UTC format, 
        /// a time then it's the time string specified based on a 24-hour clock and so on. 
        /// </remarks>
        [DataMember(Name = "max", EmitDefaultValue = false, IsRequired = false)]
        public string Maximum { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeMaximum( )
	    {
			return Maximum != null;
	    }

        /// <summary>
        /// Gets or sets the regular expression.
        /// </summary>
        /// <value>The regular expression to be applied for the validation of a string.</value>
        /// <remarks>
        /// This is only applicable for a string data type. If this is specified for a non-string type it is ignored.
        /// </remarks>
        [DataMember(Name = "regex", EmitDefaultValue = false, IsRequired = false)]
        public string Expression { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeExpression( )
	    {
			return Expression != null;
	    }

        /// <summary>
        /// Gets or sets the number of decimal places.
        /// </summary>
        /// <value>The number of decimal places for the type.</value>
        /// <remarks>This is only applicable for a decimal data type. If this is specified for a non-decimal type it is ignored.</remarks>
        [DataMember(Name = "places", EmitDefaultValue = false, IsRequired = false)]
        public string DecimalPlaces { get; set; }

        /// <summary>
        /// Gets or sets the selections.
        /// </summary>
        /// <value>The selections.</value>
        [DataMember(Name = "selections", EmitDefaultValue = false, IsRequired = false)]
        public List<Selection> Selections { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeSelections( )
	    {
			return Selections != null;
	    }

        [DataMember(Name = "defselid", EmitDefaultValue = false, IsRequired = false)]
        public long DefaultSelectionId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDefaultSelectionId( )
	    {
			return DefaultSelectionId != 0;
	    }

        [DataMember(Name = "defselval", EmitDefaultValue = false, IsRequired = false)]
        public string DefaultSelectionValue { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDefaultSelectionValue( )
	    {
			return DefaultSelectionValue != null;
	    }

        [DataMember(Name = "dir", EmitDefaultValue = false, IsRequired = false)]
        public string Direction { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDirection( )
	    {
			return Direction != null;
	    }

        /// <summary>
        /// Gets or sets the selection report identifier.
        /// </summary>
        /// <value>The selection report.</value>
        /// <remarks>
        /// This is used to call into the multiple relationship report for inline one to many selections.
        /// </remarks>
        [DataMember(Name = "selrep", EmitDefaultValue = false, IsRequired = false)]
        public Relationship SelectionReport { get; set; } // Used for multiple relationship pop-up picker

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeSelectionReport( )
	    {
			return SelectionReport != null;
	    }

    }

    /// <summary>
    /// Defines a section within a edit page.
    /// </summary>
    /// <remarks>
    /// The style of this section is defined by the 'grouped' element in the <see cref="EditFormDefinition"/> data contract.
    /// </remarks>
    [DataContract]
    public class FormSection
    {
        /// <summary>
        /// Gets or sets the heading for the section.
        /// </summary>
        /// <value>The localised heading string.</value>
        /// <remarks></remarks>
        [DataMember(Name = "title", EmitDefaultValue = false, IsRequired = true)]
        public string Title { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTitle( )
	    {
			return Title != null;
	    }

        /// <summary>
        /// Gets or sets the collection of controls to be displayed in the section.
        /// </summary>
        /// <value>The control elements.</value>
        /// <remarks>
        /// This array is order specific, the first element is displayed at the top of the stacked list on the mobile screen.
        /// </remarks>
        [DataMember(Name = "controls", EmitDefaultValue = false, IsRequired = true)]
        public List<Control> Controls { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeControls( )
	    {
			return Controls != null;
	    }

    }

    [DataContract]
    public class DataColumn
    {
        [DataMember(Name = "cid", EmitDefaultValue = false, IsRequired = true)]
        public long ControlId { get; set; } // Control ID

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeControlId( )
	    {
			return ControlId != 0;
	    }

        [DataMember(Name = "value", EmitDefaultValue = false, IsRequired = false)]
        public string Value { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeValue( )
	    {
			return Value != null;
	    }

        [DataMember(Name = "vid", EmitDefaultValue = false, IsRequired = false)]
        public long ValueId { get; set; } // Value ID (Used for pickers primarily).

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeValueId( )
	    {
			return true;
	    }

        [DataMember(Name = "values", EmitDefaultValue = false, IsRequired = false)]
        public List<Selection> MultipleValue { get; set; } // One - to - many relationships (inline)

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeMultipleValue( )
	    {
			return MultipleValue != null;
	    }

    }

    [DataContract]
    public class Relationship
    {
        /// <summary>
        /// Gets or sets the heading for the section.
        /// </summary>
        /// <value>The localised heading string.</value>
        /// <remarks></remarks>
        [DataMember(Name = "title", EmitDefaultValue = false, IsRequired = true)]
        public string Title { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTitle( )
	    {
			return Title != null;
	    }

        [DataMember(Name = "rid", EmitDefaultValue = false, IsRequired = true)]
        public long ReportId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeReportId( )
	    {
			return ReportId != 0;
	    }

        [DataMember(Name = "relid", EmitDefaultValue = false, IsRequired = true)]
        public long RelationshipId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeRelationshipId( )
	    {
			return RelationshipId != 0;
	    }

        [DataMember(Name = "dir", EmitDefaultValue = false, IsRequired = true)]
        public string Direction { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDirection( )
	    {
			return Direction != null;
	    }

        [DataMember(Name = "prid", EmitDefaultValue = false, IsRequired = true)]
        public long PickerReportId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializePickerReportId( )
	    {
			return PickerReportId != 0;
	    }

        [DataMember(Name = "prelid", EmitDefaultValue = false, IsRequired = true)]
        public long PickerRelationshipId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializePickerRelationshipId( )
	    {
			return PickerRelationshipId != 0;
	    }

        [DataMember(Name = "rdefid", EmitDefaultValue = false, IsRequired = false)]
        public long DefaultEntityId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDefaultEntityId( )
	    {
			return DefaultEntityId != 0;
	    }
        
        [DataMember(Name = "rdefval", EmitDefaultValue = false, IsRequired = false)]
        public string DefaultDisplayValue { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDefaultDisplayValue( )
	    {
			return DefaultDisplayValue != null;
	    }

        [DataMember(Name = "ro", EmitDefaultValue = false, IsRequired = false)]
        public bool ReadOnly { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeReadOnly( )
	    {
			return ReadOnly;
	    }
    }

    /// <summary>
    /// Defines the structure and makeup of a mobile screen.
    /// </summary>
    /// <remarks>
    /// </remarks>
    [DataContract]
    public class EditFormDefinition
    {
        /// <summary>
        /// Gets or sets the form identifier.
        /// </summary>
        /// <value>The form entity identifier.</value>
        [DataMember(Name = "frmid", EmitDefaultValue = false, IsRequired = true)]
        public long FormId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeFormId( )
	    {
			return true;
	    }

        /// <summary>
        /// Gets or sets the form identifier.
        /// </summary>
        /// <value>The form entity identifier.</value>
        [DataMember(Name = "typeid", EmitDefaultValue = false, IsRequired = false)]
        public long TypeId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTypeId( )
	    {
			return TypeId != 0;
	    }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The localised title string.</value>
        /// <remarks>
        /// This value is displayed at the top of the screen and _if_ the screen is pushed onto 
        /// the navigation stack is used for the breadcrumb text.
        /// </remarks>
        [DataMember(Name = "title", EmitDefaultValue = false, IsRequired = true)]
        public string Title { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTitle( )
	    {
			return Title != null;
	    }

        /// <summary>
        /// Gets or sets the sections.
        /// </summary>
        /// <value>The collection sections to be displayed.</value>
        /// <remarks>
        /// The array of sections is order dependent.
        /// </remarks>
        [DataMember(Name = "sections", EmitDefaultValue = false, IsRequired = true)]
        public List<FormSection> Sections { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeSections( )
	    {
			return Sections != null;
	    }

        /// <summary>
        /// Gets or sets the data columns.
        /// </summary>
        /// <value>The data columns.</value>
        [DataMember(Name = "data", EmitDefaultValue = false, IsRequired = false)]
        public List<DataColumn> DataColumns { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDataColumns( )
	    {
			return DataColumns != null;
	    }

        [DataMember(Name = "relations", EmitDefaultValue = false, IsRequired = false)]
        public List<Relationship> Relationships { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeRelationships( )
	    {
			return Relationships != null;
	    }
    }

    #endregion Edit Form Definition (and optional data with relationships)

    #region Edit Form Data Request (Should only contain deltas for the entity)

    [DataContract]
    public class EditDataField
    {
        [DataMember(Name = "id", EmitDefaultValue = false, IsRequired = true)]
        public long Id { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeId( )
	    {
			return Id != 0;
	    }

        [DataMember(Name = "type", EmitDefaultValue = false, IsRequired = true)]
        public string Type { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeType( )
	    {
			return Type != null;
	    }

        [DataMember(Name = "value", EmitDefaultValue = false, IsRequired = false)]
        public string Value { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeValue( )
	    {
			return Value != null;
	    }
    }

    [DataContract]
    public class EditDataRelationship
    {
        [DataMember(Name = "relid", EmitDefaultValue = false, IsRequired = true)]
        public long RelationshipId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeRelationshipId( )
	    {
			return RelationshipId != 0;
	    }

        [DataMember(Name = "dir", EmitDefaultValue = false, IsRequired = true)]
        public string Direction { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDirection( )
	    {
			return Direction != null;
	    }

        [DataMember(Name = "releid", EmitDefaultValue = false, IsRequired = false)]
        public List<long> RelatedEntityId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeRelatedEntityId( )
	    {
			return RelatedEntityId != null;
	    }
    }

    [DataContract]
    public class EditDataDefinition
    {
        /// <summary>
        /// Gets or sets the form identifier.
        /// </summary>
        /// <value>The form entity identifier.</value>
        [DataMember(Name = "frmid", EmitDefaultValue = false, IsRequired = true)]
        public long FormId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeFormId( )
	    {
			return FormId != 0;
	    }

        /// <summary>
        /// Gets or sets the report id.
        /// </summary>
        /// <value>The report id.</value>
        [DataMember(Name = "rptid", EmitDefaultValue = false, IsRequired = false)]
        public long ReportId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeReportId( )
	    {
			return ReportId != 0;
	    }

        [DataMember(Name = "fdata", EmitDefaultValue = false, IsRequired = false)]
        public List<EditDataField> FieldData { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeFieldData( )
	    {
			return FieldData != null;
	    }

        [DataMember(Name = "rdata", EmitDefaultValue = false, IsRequired = false)]
        public List<EditDataRelationship> RelationshipData { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeRelationshipData( )
	    {
			return RelationshipData != null;
	    }
    }

    #endregion Edit Form Data Response (Should only contain deltas for the entity)

    #region Report Data Definition

    [DataContract]
    public class ReportColumn
    {
        [DataMember(Name = "guid", EmitDefaultValue = false, IsRequired = false)]
        public string Guid { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeGuid( )
	    {
			return Guid != null;
	    }

        [DataMember(Name = "title", EmitDefaultValue = false, IsRequired = true)]
        public string Title { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTitle( )
	    {
			return Title != null;
	    }

        [DataMember(Name = "type", EmitDefaultValue = false, IsRequired = true)]
        public string DataType { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDataType( )
	    {
			return DataType != null;
	    }

        [DataMember(Name = "tid", EmitDefaultValue = false, IsRequired = false)]
        public long EntityTypeId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeEntityTypeId( )
	    {
			return EntityTypeId != 0;
	    }
    }

    [DataContract]
    public class ReportSortOrder
    {
        [DataMember(Name = "guid", EmitDefaultValue = false, IsRequired = true)]
        public string Guid { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeGuid( )
	    {
		    return Guid != null;
	    }

        [DataMember(Name = "order", EmitDefaultValue = false, IsRequired = true)]
        public string Order { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeOrder( )
	    {
			return Order != null;
	    }
    }

    [DataContract]
    public class ReportConditionalFormat
    {
        /// <summary>
        /// The alpha channel
        /// </summary>
        [DataMember(Name = "a", EmitDefaultValue = true, IsRequired = true)]
        public byte Alpha;
        /// <summary>
        /// The red channel
        /// </summary>
        [DataMember(Name = "r", EmitDefaultValue = true, IsRequired = true)]
        public byte Red;
        /// <summary>
        /// The green channel
        /// </summary>
        [DataMember(Name = "g", EmitDefaultValue = true, IsRequired = true)]
        public byte Green;
        /// <summary>
        /// The blue channel
        /// </summary>
        [DataMember(Name = "b", EmitDefaultValue = true, IsRequired = true)]
        public byte Blue;
    }

    [DataContract]
    public class ReportDataItem
    {
        [DataMember(Name = "value", EmitDefaultValue = false, IsRequired = true)]
        public string Value { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeValue( )
	    {
			return Value != null;
	    }

        [DataMember(Name = "eid", EmitDefaultValue = false, IsRequired = false)]
        public long EntityId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeEntityId( )
	    {
			return EntityId != 0;
	    }

        /// <summary>
        /// Gets or sets the color of the foreground.
        /// </summary>
        /// <remarks>
        /// The format for the color is ARGB - (Alpha, Red, Green Blue) a byte for each value.
        /// </remarks>
        /// <value>The color of the foreground.</value>
        [DataMember(Name = "fc", EmitDefaultValue = false, IsRequired = false)]
        public ReportConditionalFormat ForegroundColor { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeForegroundColor( )
	    {
			return ForegroundColor != null;
	    }

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <remarks>
        /// The format for the color is ARGB - (Alpha, Red, Green Blue) a byte for each value.
        /// </remarks>
        /// <value>The color of the background.</value>
        [DataMember(Name = "bc", EmitDefaultValue = false, IsRequired = false)]
        public ReportConditionalFormat BackgroundColor { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeBackgroundColor( )
	    {
			return BackgroundColor != null;
	    }
    }

    [DataContract]
    public class ReportData
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        /// <remarks></remarks>
        [DataMember(Name = "id", EmitDefaultValue = false, Order = 0, IsRequired = true)]
        public long Id { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeId( )
	    {
			return Id != 0;
	    }

        /// <summary>
        /// Gets or sets the line heading data.
        /// </summary>
        /// <value>The line heading data.</value>
        /// <remarks></remarks>
        [DataMember(Name = "item", EmitDefaultValue = false, Order = 1, IsRequired = true)]
        public List<ReportDataItem> LineItems { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeLineItems( )
	    {
			return LineItems != null;
	    }

    }

    [DataContract]
    public class ReportItemType
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        /// <remarks></remarks>
        [DataMember(Name = "id", EmitDefaultValue = false, IsRequired = true)]
        public long Id { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeId( )
	    {
			return Id != 0;
	    }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        /// <remarks></remarks>
        [DataMember(Name = "name", EmitDefaultValue = false, IsRequired = true)]
        public string Name { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    public bool ShouldSerializNamee( )
	    {
			return Name != null;
	    }
    }

    /// <summary>
    /// Class AnalyserOperatorType
    /// </summary>
    [DataContract]
    public class AnalyserOperatorType
    {
        /// <summary>
        /// Gets or sets the type (Correlates to the type used by the reporting engine).
        /// </summary>
        /// <value>The type.</value>
        [DataMember(Name = "type", EmitDefaultValue = false, IsRequired = true)]
        public string Type { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeType( )
	    {
			return Type != null;
	    }

        /// <summary>
        /// Gets or sets the name that is displayed for the condition type.
        /// </summary>
        /// <value>The name.</value>
        [DataMember(Name = "name", EmitDefaultValue = false, IsRequired = true)]
        public string Name { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeName( )
	    {
			return Name != null;
	    }
    }

    /// <summary>
    /// Class AnalyserSelection
    /// </summary>
    public class AnalyserSelection
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [DataMember(Name = "id", EmitDefaultValue = false, IsRequired = true)]
        public long Id { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeId( )
	    {
			return Id != 0;
	    }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        [DataMember(Name = "title", EmitDefaultValue = false, IsRequired = true)]
        public string Title { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTitle( )
	    {
			return Title != null;
	    }
    }

    /// <summary>
    /// Class AnalyserColumn
    /// </summary>
    [DataContract]
    public class AnalyserColumn
    {
        /// <summary>
        /// Gets or sets the expression id.
        /// </summary>
        /// <value>The expression id.</value>
        [DataMember(Name = "expid", EmitDefaultValue = false, IsRequired = true)]
        public string ExpressionId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeExpressionId( )
	    {
			return ExpressionId != null;
	    }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember(Name = "name", EmitDefaultValue = false, IsRequired = true)]
        public string Name { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeName( )
	    {
			return Name != null;
	    }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        [DataMember(Name = "type", EmitDefaultValue = false, IsRequired = true)]
        public string Type { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeType( )
	    {
			return Type != null;
	    }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [DataMember(Name = "value", EmitDefaultValue = false, IsRequired = false)]
        public string Value { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeValue( )
	    {
			return Value != null;
	    }

        /// <summary>
        /// Gets or sets the available selections.
        /// </summary>
        /// <value>The available selections.</value>
        [DataMember(Name = "selections", EmitDefaultValue = false, IsRequired = false)]
        public List<AnalyserSelection> AvailableSelections { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeAvailableSelections( )
	    {
			return AvailableSelections != null;
	    }

        /// <summary>
        /// Gets or sets the type report used for the analyser.
        /// </summary>
        /// <value>The type report.</value>
        [DataMember(Name = "rtype", EmitDefaultValue = false, IsRequired = false)]
        public long? TypeReport { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTypeReport( )
	    {
			return TypeReport != null;
	    }

        /// <summary>
        /// Gets or sets the type report used for the analyser.
        /// </summary>
        /// <value>The type report.</value>
        [DataMember(Name = "ttype", EmitDefaultValue = false, IsRequired = false)]
        public long? TargetType { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTargetType( )
	    {
			return TargetType != null;
	    }

        /// <summary>
        /// Gets or sets the applied operator.
        /// </summary>
        /// <value>The applied operator.</value>
        [DataMember(Name = "aoper", EmitDefaultValue = false, IsRequired = false)]
        public AnalyserOperatorType AppliedOperator { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeAppliedOperator( )
	    {
			return AppliedOperator != null;
	    }

        /// <summary>
        /// Gets or sets the default operator.
        /// </summary>
        /// <value>The default operator.</value>
        [DataMember(Name = "doper", EmitDefaultValue = false, IsRequired = false)]
        public AnalyserOperatorType DefaultOperator { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeDefaultOperator( )
	    {
			return DefaultOperator != null;
	    }

        /// <summary>
        /// Gets or sets the available operators.
        /// </summary>
        /// <value>The available operators.</value>
        [DataMember(Name = "opers", EmitDefaultValue = false, IsRequired = true)]
        public List<AnalyserOperatorType> AvailableOperators { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeAvailableOperators( )
	    {
			return AvailableOperators != null;
	    }
    }

    /// <summary>
    /// Class AnalyserClause
    /// </summary>
    [DataContract]
    public class AnalyserClause
    {
        /// <summary>
        /// Gets or sets the expression id.
        /// </summary>
        /// <value>The expression id.</value>
        [DataMember(Name = "expid", EmitDefaultValue = false, IsRequired = true)]
        public string ExpressionId { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeExpressionId( )
	    {
			return ExpressionId != null;
	    }
        /// <summary>
        /// Gets or sets the applied operator.
        /// </summary>
        /// <value>The applied operator.</value>
        [DataMember(Name = "oper", EmitDefaultValue = false, IsRequired = true)]
        public string Operator { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeOperator( )
	    {
			return Operator != null;
	    }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [DataMember(Name = "value", EmitDefaultValue = false, IsRequired = false)]
        public string Value { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeValue( )
	    {
			return Value != null;
	    }

        /// <summary>
        /// Gets or sets the entity identifiers.
        /// </summary>
        /// <value>The entity identifiers.</value>
        [DataMember(Name = "entids", EmitDefaultValue = false, IsRequired = false)]
        public List<long> EntityIdentifiers { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializentityIdentifierse( )
	    {
			return EntityIdentifiers != null;
	    }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    [DataContract]
    public class ReportDataDefinition
    {
        [DataMember(Name = "title", EmitDefaultValue = false, IsRequired = true)]
        public string ReportTitle { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeReportTitle( )
	    {
			return ReportTitle != null;
	    }

        [DataMember(Name = "style", EmitDefaultValue = false, IsRequired = true)]
        public string ReportStyle { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeReportStyle( )
	    {
			return ReportStyle != null;
	    }

        [DataMember(Name = "cols", EmitDefaultValue = false, IsRequired = true)]
        public List<ReportColumn> Columns { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeColumns( )
	    {
			return Columns != null;
	    }

        [DataMember(Name = "order", EmitDefaultValue = false, IsRequired = false)]
        public List<ReportSortOrder> SortOrder { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeSortOrder( )
	    {
			return SortOrder != null;
	    }

        [DataMember(Name = "data", EmitDefaultValue = false, IsRequired = false)]
        public List<ReportData> ReportDataRows { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeReportDataRows( )
	    {
			return ReportDataRows != null;
	    }

        [DataMember(Name = "types", EmitDefaultValue = false, IsRequired = false)]
        public List<ReportItemType> ReportTypes { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeReportTypes( )
	    {
			return ReportTypes != null;
	    }

        [DataMember(Name = "acols", EmitDefaultValue = false, IsRequired = false)]
        public List<AnalyserColumn> AnalyserColumns { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeAnalyserColumns( )
	    {
			return AnalyserColumns != null;
	    }

    }

    #endregion

    #region Navigation Definition

    [DataContract]
    public class NavigationItem
    {
        /// <summary>
        /// Gets or sets the id of the control instance
        /// </summary>
        /// <value>The identifier relative to the associated screen.</value>
        /// <remarks>
        /// This identifier is used to associated the data elements received or sent to the host with it's owning control.
        /// </remarks>
        [DataMember(Name = "id", EmitDefaultValue = false, IsRequired = true)]
        public long Id { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeId( )
	    {
			return Id != 0;
	    }

        /// <summary>
        /// Gets or sets the type of the control.
        /// </summary>
        /// <value>The control type.</value>
        /// <remarks>
        /// This is the class name of the Objective-C control. If this control is not found in the 
        /// application then the entire control definition is ignored.
        /// </remarks>
        /// <example>
        /// GITextControl
        /// </example>
        [DataMember(Name = "type", EmitDefaultValue = false, IsRequired = true)]
        public string Type { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeType( )
	    {
			return Type != null;
	    }

        /// <summary>
        /// Gets or sets the title that is displayed with the control.
        /// </summary>
        /// <value>The localised title string.</value>
        /// <remarks></remarks>
        [DataMember(Name = "title", EmitDefaultValue = false, IsRequired = true)]
        public string Title { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTitle( )
	    {
			return Title != null;
	    }
    }

    [DataContract]
    public class NavigationSection
    {
        /// <summary>
        /// Gets or sets the heading for the section.
        /// </summary>
        /// <value>The localised heading string.</value>
        /// <remarks></remarks>
        [DataMember(Name = "hdr", EmitDefaultValue = false, Order = 0, IsRequired = true)]
        public string Heading { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeHeading( )
	    {
			return Heading != null;
	    }

        /// <summary>
        /// Gets or sets the collection of controls to be displayed in the section.
        /// </summary>
        /// <value>The control elements.</value>
        /// <remarks>
        /// This array is order specific, the first element is displayed at the top of the stacked list on the mobile screen.
        /// </remarks>
        [DataMember(Name = "item", EmitDefaultValue = false, Order = 1, IsRequired = true)]
        public List<NavigationItem> Controls { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeControls( )
	    {
			return Controls != null;
	    }

        /// <summary>
        /// Gets or sets the footer text for the bottom of the section. This will only appear if the associated form's section definition is set to grouped.
        /// </summary>
        /// <value>The localised footer string.</value>
        /// <remarks></remarks>
        [DataMember(Name = "ftr", EmitDefaultValue = false, Order = 2, IsRequired = false)]
        public string Footer { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeFooter( )
	    {
			return Footer != null;
	    }
    }

    [DataContract]
    public class NavigationDataDefinition
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The localised title string.</value>
        /// <remarks>
        /// This value is displayed at the top of the screen and _if_ the screen is pushed onto 
        /// the navigation stack is used for the breadcrumb text.
        /// </remarks>
        [DataMember(Name = "title", EmitDefaultValue = false, IsRequired = true)]
        public string Title { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTitle( )
	    {
			return Title != null;
	    }

        /// <summary>
        /// Gets or sets the sections.
        /// </summary>
        /// <value>The collection sections to be displayed.</value>
        /// <remarks>
        /// The array of sections is order dependent.
        /// </remarks>
        [DataMember(Name = "sections", EmitDefaultValue = false, IsRequired = true)]
        public List<NavigationSection> Sections { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeSections( )
	    {
			return Sections != null;
	    }
    }

    #endregion

    #region Document/Attachments

    [DataContract]
    public class DocumentDetail
    {
        [DataMember(Name = "name", EmitDefaultValue = false, IsRequired = true)]
        public string Name { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeName( )
	    {
			return Name != null;
	    }

        [DataMember(Name = "ext", EmitDefaultValue = false, IsRequired = true)]
        public string Extension { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeExtension( )
	    {
			return Extension != null;
	    }

        [DataMember(Name = "size", EmitDefaultValue = false, IsRequired = true)]
        public long Size { get; set; }

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeSize( )
	    {
			return Size != 0;
	    }
    }

    #endregion Document/Attachments

}
