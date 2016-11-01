// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Services.Query
{
	/// <summary>
	///     Additional information used by the report when a column
	///     is displaying an image.
	/// </summary>
	[DataContract]
	public class ThumbnailDetails
	{
		/// <summary>
		///     Gets or sets the height of the thumbnail.
		/// </summary>
		/// <value>
		///     The height.
		/// </value>
		[DataMember]
		public int Height
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the thumbnail scale id.
		/// </summary>
		/// <value>
		///     The scale id.
		/// </value>
		[DataMember]
		public EntityRef ScaleId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the thumbnail size id.
		/// </summary>
		/// <value>
		///     The size id.
		/// </value>
		[DataMember]
		public EntityRef SizeId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the width of the thumbnail.
		/// </summary>
		/// <value>
		///     The width.
		/// </value>
		[DataMember]
		public int Width
		{
			get;
			set;
		}
	}
}