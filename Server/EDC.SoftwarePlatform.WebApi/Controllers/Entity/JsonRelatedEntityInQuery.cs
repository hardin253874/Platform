// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Json Related Entity In Query class.
	/// </summary>
	[DataContract]
	public class JsonRelatedEntityInQuery
	{
		/// <summary>
		///     Gets or sets the related.
		/// </summary>
		/// <value>
		///     The related.
		/// </value>
		[DataMember( Name = "rel" )]
		public JsonEntityInQuery Related
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="JsonRelatedEntityInQuery" /> is forward.
		/// </summary>
		/// <value>
		///     <c>true</c> if forward; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "forward" )]
		public bool Forward
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [must exist].
		/// </summary>
		/// <value>
		///     <c>true</c> if [must exist]; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "mustExist" )]
		public bool MustExist
		{
			get;
			set;
		}
	}
}