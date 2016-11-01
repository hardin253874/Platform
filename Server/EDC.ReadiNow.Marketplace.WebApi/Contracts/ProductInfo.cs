// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.ReadiNow.Marketplace.WebApi.Contracts
{
    /// <summary>
    /// Designed to hold information that described a product from the marketplace.
    /// </summary>
    [DataContract]
    public class ProductInfo
    {
        /// <summary>
        /// Is this a trial product.
        /// </summary>
        [DataMember(Name = "trial")]
        public bool Trial { get; set; }

        /// <summary>
        /// The "stock keeping unit" used to uniquely identify this product.
        /// </summary>
        [DataMember(Name = "sku")]
        public string SKU { get; set; }
    }
}