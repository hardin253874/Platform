// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool against the
	/// CAST application file marked as published on '2016-11-02 06:20:34Z'.
    /// </summary>
	public static class TenantRequesterSchema
	{
		//
		// Tenant Requester
		//
		public const string TenantRequesterType = "cast:tenantRequester";
		
		//
		// Name
		//
		internal static string NameField
        {
            get { return "core:name"; }
        }

		//
		// Tenant Register - Requester
		//
		internal static string TenantRegisterRelationship
        {
            get { return "cast:tenantRegisterRequester"; }
        }
	}
}