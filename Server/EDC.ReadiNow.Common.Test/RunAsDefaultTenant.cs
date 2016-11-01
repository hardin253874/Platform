// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Test
{
	/// <summary>
	///     Runs the applied test case method under the default (EDC) tenant.
	/// </summary>
	public class RunAsDefaultTenant : RunAsTenantAttribute
	{
		/// <summary>
		///     The default tenant name
		/// </summary>
		public const string DefaultTenantName = "EDC";

		/// <summary>
		///     Initializes a new instance of the <see cref="RunAsDefaultTenant" /> class.
		/// </summary>
		public RunAsDefaultTenant( )
			: base( DefaultTenantName )
		{
		}
	}
}