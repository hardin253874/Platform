// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace EDC.SoftwarePlatform.Services.Query
{
	[DataContract]
	public class GridRequest
	{
		// TODO: this isn't actually being used at the moment, but it causes the Conditions contract to be made available for discovery on the client side,
		// where it is currently being used. This needs to be fixed!
		/// <summary>
		///     Any server-side filters to be applied.
		/// </summary>
		[DataMember]
		public Conditions Conditions
		{
			get;
			set;
		}

		/// <summary>
		///     Number of rows to return
		/// </summary>
		[DataMember]
		public int Count
		{
			get;
			set;
		}

		/// <summary>
		///     First row to return
		/// </summary>
		[DataMember]
		public int Index
		{
			get;
			set;
		}
	}
}