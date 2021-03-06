// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool against the
	/// CAST application file marked as published on '2016-12-07 00:40:35Z'.
    /// </summary>
	public static class MarketplaceOrderHistorySchema
	{
		//
		// Marketplace Order History
		//
		public const string MarketplaceOrderHistoryType = "cast:marketplaceOrderHistory";
		
		//
		// Name
		//
		internal static string NameField
        {
            get { return "core:name"; }
        }
		
		//
		// Message
		//
		internal static string MessageField
        {
            get { return "cast:marketplaceOrderMessageField"; }
        }

		//
		// Order History Purchased
		//
		internal static string PurchasedRelationship
        {
            get { return "cast:orderHistoryPurchased"; }
        }

		//
		// Order History Tenant
		//
		internal static string TenantLookup
        {
            get { return "cast:orderHistoryTenant"; }
        }
	}
}