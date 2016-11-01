// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Marketplace.WebApi.Contracts
{
    /// <summary>
    /// Information about an order that has come from the marketplace.
    /// </summary>
    [DataContract]
    public class MarketplaceOrder
    {
        /// <summary>
        /// Customer information pertaining to the order.
        /// </summary>
        [DataMember(Name = "customer")]
        public CustomerInfo Customer { get; set; }

        /// <summary>
        /// Product information pertaining to the order.
        /// </summary>
        [DataMember(Name = "purchase")]
        public IList<ProductInfo> Purchase { get; set; }
    }
}