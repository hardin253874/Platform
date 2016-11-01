// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.Services.ExportData
{
	[DataContract]
	public class ExportDataInfo
	{
		/// <summary>
		///     Gets and sets the Filestream.
		/// </summary>
		[DataMember]
		public byte[ ] FileStream
		{
			get;
			set;
		}
	}
}