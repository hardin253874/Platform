// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool on July 20, 2016.
    /// </summary>
	public static class TenantRegisterPurposeSchema
	{
		//
		// Tenant Register Purpose
		//
		public const string TenantRegisterPurposeType = "cast:tenantRegisterPurpose";

		//
		// Name
		//
		internal static string NameField
        {
            get { return "core:name"; }
        }

		//
		// App Dev
		//
		internal const string AppDevEnum = "cast:tenantRegisterPurpose_AppDev";

		//
		// Charity
		//
		internal const string CharityEnum = "cast:tenantRegisterPurpose_Charity";

		//
		// Consumer
		//
		internal const string ConsumerEnum = "cast:tenantRegisterPurpose_Consumer";

		//
		// Personal
		//
		internal const string PersonalEnum = "cast:tenantRegisterPurpose_Personal";

		//
		// Sandbox
		//
		internal const string SandboxEnum = "cast:tenantRegisterPurpose_Sandbox";
	}

	/// <summary>
    /// This enum was auto-generated by a templating tool on July 20, 2016.
    /// </summary>
	public enum TenantRegisterPurposeEnumeration : ulong
    {
		AppDev = 1,
		Charity = 2,
		Consumer = 3,
		Personal = 4,
		Sandbox = 5
	}
}