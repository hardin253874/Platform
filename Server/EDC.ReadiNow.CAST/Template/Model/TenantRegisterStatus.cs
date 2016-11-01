// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool on July 20, 2016.
    /// </summary>
	public static class TenantRegisterStatusSchema
	{
		//
		// Tenant Register Status
		//
		public const string TenantRegisterStatusType = "cast:tenantRegisterStatus";

		//
		// Name
		//
		internal static string NameField
        {
            get { return "core:name"; }
        }

		//
		// Active
		//
		internal const string ActiveEnum = "cast:tenantRegisterStatus_Active";

		//
		// On Hold
		//
		internal const string OnHoldEnum = "cast:tenantRegisterStatus_OnHold";

		//
		// Redundant
		//
		internal const string RedundantEnum = "cast:tenantRegisterStatus_Redundant";
	}

	/// <summary>
    /// This enum was auto-generated by a templating tool on July 20, 2016.
    /// </summary>
	public enum TenantRegisterStatusEnumeration : ulong
    {
		Active = 1,
		OnHold = 2,
		Redundant = 3
	}
}