// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Security
{
	[DataContract]
	public class NewAccessRuleInfo
	{
		/// <summary>
		///     The type the access rule allows access to.
		/// </summary>
		[DataMember( Name = "securableEntityId", IsRequired = true )]
		public long SecurableEntityId
		{
			get;
			set;
		}

		/// <summary>
		///     The user or role the rule grants access for.
		/// </summary>
		[DataMember( Name = "subjectId", IsRequired = true )]
		public long SubjectId
		{
			get;
			set;
		}
	}
}