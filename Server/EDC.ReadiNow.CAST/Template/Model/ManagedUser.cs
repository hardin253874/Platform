// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool against the
	/// CAST application file marked as published on '2016-12-07 00:40:35Z'.
    /// </summary>
	public static class ManagedUserSchema
	{
		//
		// Managed User
		//
		public const string ManagedUserType = "cast:managedUser";
		
		//
		// Name
		//
		internal static string NameField
        {
            get { return "core:name"; }
        }
		
		//
		// Remote ID
		//
		internal static string RemoteIdField
        {
            get { return "cast:userRemoteId"; }
        }

		//
		// Managed Tenant - Users
		//
		internal static string ManagedTenantLookup
        {
            get { return "cast:userInTenant"; }
        }

		//
		// Managed User Status
		//
		internal static string ManagedUserStatusEnum
        {
            get { return "cast:userStatus"; }
        }

		//
		// Users - Roles
		//
		internal static string RolesRelationship
        {
            get { return "cast:userHasRoles"; }
        }
	}
}