// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class ReportResult.
	/// </summary>
	[DataContract]
	public class ReportResult
	{
		/// <summary>
		///     Gets or sets the metadata.
		/// </summary>
		/// <value>The metadata.</value>
		[DataMember( Name = "meta", EmitDefaultValue = false, IsRequired = false )]
		public ReportMetadata Metadata
		{
			get;
			set;
		}

		/// <summary>
		///		Should the metadata be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeMetadata( )
	    {
			return Metadata != null;
	    }

		/// <summary>
		///     Gets or sets the grid data.
		/// </summary>
		/// <value>The grid data.</value>
		[DataMember( Name = "gdata", EmitDefaultValue = false, IsRequired = false )]
		public List<DataRow> GridData
		{
			get;
			set;
		}

		/// <summary>
		///		Should the grid data be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeGridData( )
	    {
			return GridData != null;
	    }
	}
}