// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool against the
	/// CAST application file marked as published on '2016-11-02 06:20:34Z'.
    /// </summary>
	public static class ManagedUserRolesSchema
	{
		//
		// Managed User Roles
		//
		public const string ManagedUserRolesType = "cast:managedUserRole";

		//
		// Name
		//
		internal static string NameField
        {
            get { return "core:name"; }
        }

		//
		// Administrators
		//
		internal const string AdministratorsEnum = "cast:managedUserRoleAdministrators";

		//
		// CAST Managers
		//
		internal const string CastManagersEnum = "cast:managedUserRoleCastManagers";

		//
		// Customer Approvers
		//
		internal const string CustomerApproversEnum = "cast:managedUserRoleCustomerApprovers";

		//
		// Everyone
		//
		internal const string EveryoneEnum = "cast:managedUserRoleEveryone";

		//
		// Self Serve
		//
		internal const string SelfServeEnum = "cast:managedUserRoleSelfServe";
	}

	/// <summary>
    /// This enum was auto-generated by a templating tool against the
	/// CAST application file marked as published on '2016-11-02 06:20:34Z'.
    /// </summary>
	public enum ManagedUserRolesEnumeration : ulong
    {
		Administrators = 1,
		CastManagers = 3,
		CustomerApprovers = 4,
		Everyone = 0,
		SelfServe = 2
	}
}