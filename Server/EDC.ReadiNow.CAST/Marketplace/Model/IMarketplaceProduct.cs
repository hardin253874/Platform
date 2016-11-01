// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Marketplace.Model
{
    /// <summary>
    /// Defines a product that can be purchase on the marketplace. Essentially a collection of apps
    /// or specific versions of apps.
    /// </summary>
    public interface IMarketplaceProduct : IEntity
    {
        /// <summary>
        /// The name of the product.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The stock keeping unit which uniquely identifies the product.
        /// </summary>
        string Sku { get; set; }

        /// <summary>
        /// The applications included in this product.
        /// </summary>
        IEntityCollection<ManagedApp> IncludesApps { get; set; }

        /// <summary>
        /// The specific application versions included in this product.
        /// </summary>
        IEntityCollection<ManagedAppVersion> IncludesAppVersions { get; set; }
    }
}
