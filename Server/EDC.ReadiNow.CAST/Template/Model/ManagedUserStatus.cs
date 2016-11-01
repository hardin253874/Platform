// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool on July 20, 2016.
    /// </summary>
	public static class ManagedUserStatusSchema
	{
		//
		// Managed User Status
		//
		public const string ManagedUserStatusType = "cast:managedUserStatus";

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
		internal const string ActiveEnum = "cast:managedUserStatus_Active";

		//
		// Disabled
		//
		internal const string DisabledEnum = "cast:managedUserStatus_Disabled";

		//
		// Expired
		//
		internal const string ExpiredEnum = "cast:managedUserStatus_Expired";

		//
		// Locked
		//
		internal const string LockedEnum = "cast:managedUserStatus_Locked";

		//
		// Unknown
		//
		internal const string UnknownEnum = "cast:managedUserStatus_Unknown";
	}

	/// <summary>
    /// This enum was auto-generated by a templating tool on July 20, 2016.
    /// </summary>
	public enum ManagedUserStatusEnumeration : ulong
    {
		Active = 2,
		Disabled = 3,
		Expired = 5,
		Locked = 4,
		Unknown = 1
	}
}