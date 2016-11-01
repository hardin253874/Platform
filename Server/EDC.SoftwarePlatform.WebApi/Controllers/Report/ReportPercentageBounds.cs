// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class ReportPercentageBounds.
	/// </summary>
	[DataContract]
	public class ReportPercentageBounds
	{
		/// <summary>
		///     Gets or sets the lower bounds.
		/// </summary>
		/// <value>The lower bounds.</value>
		[DataMember( Name = "lower", EmitDefaultValue = true, IsRequired = true )]
		public object LowerBounds
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the upper bounds.
		/// </summary>
		/// <value>The upper bounds.</value>
		[DataMember( Name = "upper", EmitDefaultValue = true, IsRequired = true )]
		public object UpperBounds
		{
			get;
			set;
		}
	}
}