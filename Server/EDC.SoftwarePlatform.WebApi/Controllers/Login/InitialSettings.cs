// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
	/// <summary>
	///     Initial Settings sent back to the client after a successful login.
	/// </summary>
	[DataContract]
	public class InitialSettings
	{
		/// <summary>
		///     Gets or sets the platform version.
		/// </summary>
		/// <value>
		///     The platform version.
		/// </value>
		[DataMember( Name = "platformVersion", EmitDefaultValue = true, IsRequired = false )]
		public string PlatformVersion
		{
			get;
			set;
		}

        /// <summary>
        ///     Gets or sets the minimum version of the client required to interact with this server
        /// </summary>
        /// <value>
        ///     The min client required version version.
        /// </value>
        [DataMember(Name = "requiredClientVersion", EmitDefaultValue = true, IsRequired = false)]
        public string RequiredClientVersion
        {
            get;
            set;
        }

        /// <summary>
        ///     The branch name.
        /// </summary>
        [DataMember(Name = "branchName", EmitDefaultValue = false, IsRequired = false)]
        public string BranchName
        {
            get;
            set;
        }

		/// <summary>
		///		Should the name of the branch be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeBranchName( )
	    {
		    return BranchName != null;
	    }

		/// <summary>
		///     Gets or sets the culture.
		/// </summary>
		/// <value>
		///     The locale.
		/// </value>
		[DataMember( Name = "culture", EmitDefaultValue = true, IsRequired = false )]
		public string Culture
		{
			get;
			set;
		}

        /// <summary>
        ///     Comma separated list of enabled feature switches for the tenant.
        /// </summary>
        [DataMember(Name = "featureSwitches", EmitDefaultValue = false, IsRequired = false)]
        public string FeatureSwitches
        {
            get;
            set;
        }
    }
}