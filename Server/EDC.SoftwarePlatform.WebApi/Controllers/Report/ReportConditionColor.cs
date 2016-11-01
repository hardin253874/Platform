// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class ReportConditionColor.
	/// </summary>
	[DataContract]
	public class ReportConditionColor
	{
		/// <summary>
		///     Gets or sets the alpha channel.
		/// </summary>
		/// <value>The alpha.</value>
		[DataMember( Name = "a", EmitDefaultValue = true, IsRequired = true )]
		public byte Alpha
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the red channel.
		/// </summary>
		/// <value>The red.</value>
		[DataMember( Name = "r", EmitDefaultValue = true, IsRequired = true )]
		public byte Red
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the green channel.
		/// </summary>
		/// <value>The green.</value>
		[DataMember( Name = "g", EmitDefaultValue = true, IsRequired = true )]
		public byte Green
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the blue channel.
		/// </summary>
		/// <value>The blue.</value>
		[DataMember( Name = "b", EmitDefaultValue = true, IsRequired = true )]
		public byte Blue
		{
			get;
			set;
		}
	}
}