// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool on July 20, 2016.
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