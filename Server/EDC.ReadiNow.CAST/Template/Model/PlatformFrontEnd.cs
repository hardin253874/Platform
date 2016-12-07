// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool against the
	/// CAST application file marked as published on '2016-11-02 06:20:34Z'.
    /// </summary>
	public static class PlatformFrontEndSchema
	{
		//
		// Platform Front End
		//
		public const string PlatformFrontEndType = "cast:platformFrontEnd";
		
		//
		// Name
		//
		internal static string NameField
        {
            get { return "core:name"; }
        }
		
		//
		// Domain
		//
		internal static string DomainField
        {
            get { return "cast:platformFrontEndDomain"; }
        }
		
		//
		// Host
		//
		internal static string HostField
        {
            get { return "cast:platformFrontEndHost"; }
        }
		
		//
		// Last Contact
		//
		internal static string LastContactField
        {
            get { return "cast:platformFrontEndLastContact"; }
        }

		//
		// Managed Platform - Front End History
		//
		internal static string ManagedPlatformLookup
        {
            get { return "cast:platformFrontEndHistory"; }
        }
	}
}