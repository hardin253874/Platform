// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool against the
	/// CAST application file marked as published on '2016-12-07 00:40:35Z'.
    /// </summary>
	public static class ManagedPlatformSchema
	{
		//
		// Managed Platform
		//
		public const string ManagedPlatformType = "cast:managedPlatform";
		
		//
		// Name
		//
		internal static string NameField
        {
            get { return "core:name"; }
        }
		
		//
		// DatabaseId
		//
		internal static string DatabaseIdField
        {
            get { return "cast:platformDatabaseId"; }
        }
		
		//
		// Last Contact
		//
		internal static string LastContactField
        {
            get { return "cast:platformLastContact"; }
        }

		//
		// Managed Platform - Database History
		//
		internal static string DatabaseHistoryRelationship
        {
            get { return "cast:platformDatabaseHistory"; }
        }

		//
		// Managed Platform - Front End History
		//
		internal static string FrontEndHistoryRelationship
        {
            get { return "cast:platformFrontEndHistory"; }
        }

		//
		// Platform Contains Tenants
		//
		internal static string ContainsTenantRelationship
        {
            get { return "cast:platformContainsTenants"; }
        }

		//
		// Platform Has App Versions
		//
		internal static string HasAppVersionsRelationship
        {
            get { return "cast:platformHasAppVersions"; }
        }
	}
}