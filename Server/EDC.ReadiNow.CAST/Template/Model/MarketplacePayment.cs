// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool against the
	/// CAST application file marked as published on '2016-12-07 00:40:35Z'.
    /// </summary>
	public static class MarketplacePaymentSchema
	{
		//
		// Marketplace Payment
		//
		public const string MarketplacePaymentType = "cast:marketplacePayment";
		
		//
		// Name
		//
		internal static string NameField
        {
            get { return "core:name"; }
        }
		
		//
		// Amount
		//
		internal static string AmountField
        {
            get { return ""; }
        }
		
		//
		// Payment Date
		//
		internal static string PaymentDateField
        {
            get { return ""; }
        }

		//
		// Customer Payments
		//
		internal static string PayerLookup
        {
            get { return "cast:customerPayments"; }
        }
	}
}