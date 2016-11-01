// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;

namespace EDC.ReadiNow.Security
{
	/// <summary>
	///     Represents the core identity data.
	/// </summary>
	[Serializable]
	[DebuggerStepThrough]
	public class IdentityInfo
	{
		/// <summary>
		///     Initializes a new instance of the IdentityInfo class.
		/// </summary>
		public IdentityInfo( )
		{
			Id = -1;
			Name = string.Empty;
		}

		/// <summary>
		///     Initializes a new instance of the IdentityInfo class.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="name">The name.</param>
		/// <remarks></remarks>
		public IdentityInfo( long id, string name )
		{
			Id = id;
			Name = name;
		}             

        /// <summary>
        ///     Gets the ID associated with the user data.
        /// </summary>
        public long Id
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the username associated with the user data.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

        /// <summary>
		///     Gets the identity provider.
		/// </summary>
		public long IdentityProviderId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the identity provider identifier.
        /// </summary>
        /// <value>The identity provider identifier.</value>
        public string IdentityProviderTypeAlias
        {
            get;
            set;
        }

        /// <summary>
		///     Gets the identity provider user id.
		/// </summary>
		public long IdentityProviderUserId
        {
            get;
            set;
        }
    }
}