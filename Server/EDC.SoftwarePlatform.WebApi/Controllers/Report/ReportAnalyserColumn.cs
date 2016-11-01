// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class ReportAnalyserColumn.
	/// </summary>
	[DataContract]
	public class ReportAnalyserColumn
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
		[DataMember( Name = "anltype", EmitDefaultValue = false, IsRequired = false )]
		public string AnalyserType
		{
			get;
			set;
		}

		/// <summary>
		///		Should the type of the analyser be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeAnalyserType( )
		{
			return AnalyserType != null;
		}

        /// <summary>
        ///     Gets or sets the condition parameter picker id.
        /// </summary>
        /// <value>The condition parameter picker id.</value>
        [DataMember(Name = "respickerid", EmitDefaultValue = false, IsRequired = false)]
        public long ConditionParameterPickerId
        {
            get;
            set;
        }

        /// <summary>
        ///		Should the condition parameter picker id be serialized.
        /// </summary>
        /// <returns></returns>
        [UsedImplicitly]
        private bool ShouldSerializeConditionParameterPickerId()
        {
            return ConditionParameterPickerId != 0;
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
        ///     Gets or sets the filtered unique identifier.
        /// </summary>
        /// <value>The filtered unique identifier.</value>
        [DataMember(Name = "filtereids", EmitDefaultValue = false, IsRequired = false)]
        public long[] FilteredEntityIds
        {
            get;
            set;
        }

		/// <summary>
		///		Should the filtered entity ids be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeFilteredEntityIds( )
		{
			return FilteredEntityIds != null;
		}

		/// <summary>
		///     Gets or sets the default operator.
		/// </summary>
		/// <value>The default operator.</value>
		[DataMember( Name = "oper", EmitDefaultValue = false, IsRequired = false )]
		public string Operator
		{
			get;
			set;
		}

		/// <summary>
		///		Should the operator be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeOperator( )
		{
			return Operator != null;
		}

		/// <summary>
		///     Gets or sets the default operator.
		/// </summary>
		/// <value>The default operator.</value>
		[DataMember( Name = "doper", EmitDefaultValue = false, IsRequired = true )]
		public string DefaultOperator
		{
			get;
			set;
		}

		/// <summary>
		///		Should the default operator be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeDefaultOperator( )
		{
			return DefaultOperator != null;
		}

		/// <summary>
		///     Gets or sets the report column that is referenced by this analyser condition.
		/// </summary>
		/// <value>The report column.</value>
		[DataMember( Name = "rcolid", EmitDefaultValue = false, IsRequired = false )]
		public long ReportColumnId
		{
			get;
			set;
		}

		/// <summary>
		///		Should the report column identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeReportColumnId( )
		{
			return ReportColumnId != 0;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is condition locked.
		/// </summary>
		/// <value><c>true</c> if this instance is condition locked; otherwise, <c>false</c>.</value>
		[DataMember( Name = "locked", EmitDefaultValue = false, IsRequired = false )]
		public bool IsConditionLocked
		{
			get;
			set;
		}

		/// <summary>
		///		Should the is condition locked value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeIsConditionLocked( )
		{
			return IsConditionLocked;
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		[DataMember( Name = "value", EmitDefaultValue = false, IsRequired = false )]
		public string Value
		{
			get;
			set;
		}

		/// <summary>
		///		Should the value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeValue( )
		{
			return Value != null;
		}

		/// <summary>
		///     Gets or sets the values.
		/// </summary>
		/// <remarks>
		///     This is only appropriate for Choice and inline relationship types.
		/// </remarks>
		/// <value>The values.</value>
		[DataMember( Name = "values", EmitDefaultValue = false, IsRequired = false )]
		public Dictionary<long, string> Values
		{
			get;
			set;
		}

		/// <summary>
		///		Should the values be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeValues( )
		{
			return Values != null;
		}
	}
}