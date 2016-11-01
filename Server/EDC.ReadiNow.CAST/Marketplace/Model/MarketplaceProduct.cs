// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Internal;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.CAST.Template.Model;

namespace EDC.ReadiNow.CAST.Marketplace.Model
{
    /// <summary>
    /// A product that may be purchased on the marketplace, bundling applications (or particular versions thereof) together with 
    /// a license.
    /// </summary>
    [Serializable]
    [ModelClass(MarketplaceProductSchema.MarketplaceProductType)]
    public class MarketplaceProduct : StrongEntity, IMarketplaceProduct
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketplaceProduct" /> class.
        /// </summary>
        public MarketplaceProduct() : base(typeof(MarketplaceProduct)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketplaceProduct" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal MarketplaceProduct(IActivationData activationData) : base(activationData) { }

        #endregion

        /// <summary>
        /// The name of the product as it appears on the marketplace.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(MarketplaceProductSchema.NameField); }
            set { SetField(MarketplaceProductSchema.NameField, value); }
        }

        /// <summary>
        /// The "stock keeping unit" which uniquely identifies the product amongst all those available.
        /// </summary>
        public string Sku
        {
            get { return (string)GetField(MarketplaceProductSchema.SkuField); }
            set { SetField(MarketplaceProductSchema.SkuField, value); }
        }
        
        /// <summary>
        /// The applications (all versions thereof) which are included in this product.
        /// </summary>
        public IEntityCollection<ManagedApp> IncludesApps
        {
            get { return GetRelationships<ManagedApp>(MarketplaceProductSchema.IncludesAppRelationship, Direction.Forward).Entities; }
            set { SetRelationships(MarketplaceProductSchema.IncludesAppRelationship, value.ToEntityRelationshipCollection(), Direction.Forward); }
        }

        /// <summary>
        /// The application versions which are included in this product.
        /// </summary>
        public IEntityCollection<ManagedAppVersion> IncludesAppVersions
        {
            get { return GetRelationships<ManagedAppVersion>(MarketplaceProductSchema.IncludesAppVersionRelationship, Direction.Forward).Entities; }
            set { SetRelationships(MarketplaceProductSchema.IncludesAppVersionRelationship, value.ToEntityRelationshipCollection(), Direction.Forward); }
        }

        #region Internals

        internal static string MarketplaceProductPreloadQuery
        {
            get
            {
                return "alias,name,isOfType.{alias,name}," +
                        MarketplaceProductSchema.SkuField + "," +
                        MarketplaceProductSchema.IncludesAppRelationship + ".{" + ManagedApp.ManagedAppPreloadQuery + "}," +
                        MarketplaceProductSchema.IncludesAppVersionRelationship + ".{" + ManagedAppVersion.ManagedAppVersionPreloadQuery + "}";
            }
        }

        #endregion
    }
}
