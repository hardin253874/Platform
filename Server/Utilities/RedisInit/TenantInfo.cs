// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace RedisInit
{
	/// <summary>
	///     Tenant Info.
	/// </summary>
	public class TenantInfo
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TenantInfo" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="name">The name.</param>
		public TenantInfo( long id, string name )
		{
			Id = id;
			Name = name;
		}

		/// <summary>
		///     Gets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		public long Id
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}
	}
}