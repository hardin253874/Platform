// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;

namespace EDC.SoftwarePlatform.WebApiTestControllers.Controllers.Entity
{
	/// <summary>
	///     Test Request class.
	/// </summary>
	[DataContract]
	public class TestRequest
	{
		/// <summary>
		///     Gets or sets my other data.
		/// </summary>
		/// <value>
		///     My other data.
		/// </value>
		[DataMember( Name = "myOtherData" )]
		public string MyOtherData
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets my entity data.
		/// </summary>
		/// <value>
		///     My entity data.
		/// </value>
		[DataMember( Name = "myEntityData" )]
		public EntityNugget MyEntityData
		{
			get;
			set;
		}
	}
}