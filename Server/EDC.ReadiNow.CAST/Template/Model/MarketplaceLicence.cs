// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.CAST.Template.Model
{
	/// <summary>
    /// This class was auto-generated by a templating tool against the
	/// CAST application file marked as published on '2016-12-07 00:40:35Z'.
    /// </summary>
	public static class MarketplaceLicenceSchema
	{
		//
		// Marketplace Licence
		//
		public const string MarketplaceLicenceType = "cast:marketplaceLicence";
		
		//
		// Name
		//
		internal static string NameField
        {
            get { return "core:name"; }
        }

		//
		// Product Licence
		//
		internal static string LicensedProductLookup
        {
            get { return "cast:productLicence"; }
        }
	}
}