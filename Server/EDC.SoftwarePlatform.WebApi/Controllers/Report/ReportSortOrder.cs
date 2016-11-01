// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class ReportSortOrder.
	/// </summary>
	[DataContract]
	public class ReportSortOrder
	{
		/// <summary>
		///     Gets or sets the column unique identifier.
		/// </summary>
		/// <value>The column unique identifier.</value>
		[DataMember( Name = "colid", EmitDefaultValue = false, IsRequired = true )]
		public string ColumnId
		{
			get;
			set;
		}

		/// <summary>
		///		Should the column identifier be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeColumnId( )
		{
			return ColumnId != null;
		}

		/// <summary>
		///     Gets or sets the order.
		/// </summary>
		/// <value>The order.</value>
		[DataMember( Name = "order", EmitDefaultValue = false, IsRequired = true )]
		public string Order
		{
			get;
			set;
		}

		/// <summary>
		///		Should the order be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeOrder( )
		{
			return Order != null;
		}
	}
}