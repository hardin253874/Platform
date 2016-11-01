// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool on July 20, 2016.
    /// </summary>
	public static class ManagedCustomerSchema
	{
		//
		// Managed Customer
		//
		public const string ManagedCustomerType = "cast:managedCustomer";
		
		//
		// Name
		//
		internal static string NameField
        {
            get { return "core:name"; }
        }
		
		//
		// Company
		//
		internal static string CompanyField
        {
            get { return "cast:customerCompanyField"; }
        }
		
		//
		// Department
		//
		internal static string DepartmentField
        {
            get { return "cast:customerDepartmentField"; }
        }
		
		//
		// Email
		//
		internal static string EmailField
        {
            get { return "cast:customerEmailField"; }
        }
		
		//
		// First Name
		//
		internal static string FirstNameField
        {
            get { return "cast:customerFirstNameField"; }
        }
		
		//
		// Industry
		//
		internal static string IndustryField
        {
            get { return "cast:customerIndustryField"; }
        }
		
		//
		// Last Name
		//
		internal static string LastNameField
        {
            get { return "cast:customerLastNameField"; }
        }
		
		//
		// Password
		//
		internal static string PasswordField
        {
            get { return "cast:customerPasswordField"; }
        }
		
		//
		// Size
		//
		internal static string SizeField
        {
            get { return "cast:customerSizeField"; }
        }
		
		//
		// Telephone
		//
		internal static string TelephoneField
        {
            get { return "cast:customerTelephoneField"; }
        }
		
		//
		// Verified
		//
		internal static string VerifiedField
        {
            get { return "cast:customerVerifiedField"; }
        }
		
		//
		// Verified By
		//
		internal static string VerifiedByField
        {
            get { return "cast:customerVerifiedByField"; }
        }

		//
		// Customer Can Access App
		//
		internal static string CanAccessRelationship
        {
            get { return "cast:customerCanAccessApp"; }
        }

		//
		// Customer Has Purchased Product
		//
		internal static string HasPurchasedRelationship
        {
            get { return "cast:customerHasPurchasedProduct"; }
        }

		//
		// Customer Has Tenants
		//
		internal static string HasTenantRelationship
        {
            get { return "cast:customerHasTenants"; }
        }

		//
		// Customer Payments
		//
		internal static string PaymentsRelationship
        {
            get { return "cast:customerPayments"; }
        }

		//
		// Verification Link - Managed Customer
		//
		internal static string VerificationLinkRelationship
        {
            get { return "cast:verificationLinkCustomer"; }
        }
	}
}