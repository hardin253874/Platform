// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Console
{
	/// <summary>
	///     Session info result data.
	/// </summary>
	[DataContract]
	public class SessionInfoResult
	{
		/// <summary>
		///     The bookmark scheme.
		/// </summary>
		[DataMember( Name = "bookmarkScheme", EmitDefaultValue = false, IsRequired = false )]
		public string BookmarkScheme
		{
			get;
			set;
		}

		/// <summary>
		/// Should the bookmark scheme be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
	    private bool ShouldSerializeBookmarkScheme( )
	    {
			return BookmarkScheme != null;
	    }

		/// <summary>
		///     The platform version.
		/// </summary>
		[DataMember( Name = "platformVersion", EmitDefaultValue = false, IsRequired = false )]
		public string PlatformVersion
		{
			get;
			set;
		}

		/// <summary>
		/// Should the platform version be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializePlatformVersion( )
	    {
			return PlatformVersion != null;
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
		/// Should the name of the branch be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
	    private bool ShouldSerializeBranchName( )
	    {
			return BranchName != null;
	    }
	}
}