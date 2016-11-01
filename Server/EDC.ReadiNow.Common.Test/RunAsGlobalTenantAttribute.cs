// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.Test
{
	/// <summary>
	///     Runs the applied test case method under the global tenant
	/// </summary>
	public class RunAsGlobalTenantAttribute : RunAsTenantAttribute
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RunAsGlobalTenantAttribute" /> class.
		/// </summary>
		public RunAsGlobalTenantAttribute( )
			: base( SpecialStrings.GlobalTenant )
		{
		}
	}
}