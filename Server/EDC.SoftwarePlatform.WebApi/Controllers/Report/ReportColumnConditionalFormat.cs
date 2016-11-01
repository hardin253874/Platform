// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Report Column Conditional Format class.
	/// </summary>
	[DataContract]
	public class ReportColumnConditionalFormat
	{
		/// <summary>
		///     Gets or sets the style.
		/// </summary>
		/// <value>The style.</value>
		[DataMember( Name = "style", EmitDefaultValue = false, IsRequired = true )]
		public string Style
		{
			get;
			set;
		}

		/// <summary>
		///		Should the style be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeStyle( )
		{
			return Style != null;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [show value].
		/// </summary>
		/// <value><c>true</c> if [show value]; otherwise, <c>false</c>.</value>
		[DataMember( Name = "showval", EmitDefaultValue = false, IsRequired = false )]
		public bool ShowValue
		{
			get;
			set;
		}

		/// <summary>
		///		Should the show value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeShowValue( )
		{
			return ShowValue;
		}

		/// <summary>
		///     Gets or sets the rules.
		/// </summary>
		/// <value>
		///     The rules.
		/// </value>
		[DataMember( Name = "rules", EmitDefaultValue = false, IsRequired = true )]
		public List<ReportConditionalFormatRule> Rules
		{
			get;
			set;
		}

		/// <summary>
		///		Should the rules be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeRules( )
		{
			return Rules != null;
		}
	}
}