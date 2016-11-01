// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class ReportConditionalFormatRule.
	/// </summary>
	[DataContract]
	public class ReportConditionalFormatRule
	{
		/// <summary>
		///     Gets or sets the operator.
		/// </summary>
		/// <value>The operator.</value>
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
		///     Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		[DataMember( Name = "val", EmitDefaultValue = false, IsRequired = false )]
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
		/// <value>The values.</value>
		[DataMember( Name = "vals", EmitDefaultValue = false, IsRequired = false )]
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

		/// <summary>
		///     The foreground color
		/// </summary>
		[DataMember( Name = "fgcolor", EmitDefaultValue = false, IsRequired = false )]
		public ReportConditionColor ForegroundColor
		{
			get;
			set;
		}

		/// <summary>
		///		Should the color of the foreground be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeForegroundColor( )
		{
			return ForegroundColor != null;
		}

		/// <summary>
		///     The background color
		/// </summary>
		[DataMember( Name = "bgcolor", EmitDefaultValue = false, IsRequired = false )]
		public ReportConditionColor BackgroundColor
		{
			get;
			set;
		}

		/// <summary>
		///		Should the color of the background be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeBackgroundColor( )
		{
			return BackgroundColor != null;
		}

		/// <summary>
		///     Gets or sets the image entity unique identifier.
		/// </summary>
		/// <value>The image entity unique identifier.</value>
		[DataMember( Name = "imgid", EmitDefaultValue = false, IsRequired = false )]
		public long? ImageEntityId
		{
			get;
			set;
		}

		/// <summary>
		///		Should the image entity identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeImageEntityId( )
		{
			return ImageEntityId != null;
		}

        /// <summary>
        ///     Gets or sets the image entity unique identifier.
        /// </summary>
        /// <value>The image entity unique identifier.</value>
        [DataMember(Name = "cfid", EmitDefaultValue = false, IsRequired = false)]
        public long? CfEntityId
        {
            get;
            set;
        }

		/// <summary>
		///		Should the cf entity identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeCfEntityId( )
		{
			return CfEntityId != null;
		}

		/// <summary>
		///     The percentage bounds
		/// </summary>
		[DataMember( Name = "bounds", EmitDefaultValue = false, IsRequired = false )]
		public ReportPercentageBounds PercentageBounds
		{
			get;
			set;
		}

		/// <summary>
		///		Should the percentage bounds be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializePercentageBounds( )
		{
			return PercentageBounds != null;
		}
	}
}