// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Internal;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.CAST.Template.Model;

namespace EDC.ReadiNow.CAST.Marketplace.Model
{
    /// <summary>
    /// An order for an application(s) that has come through from the marketplace.
    /// </summary>
    [Serializable]
    [ModelClass(MarketplaceOrderHistorySchema.MarketplaceOrderHistoryType)]
    public class MarketplaceOrderHistory : StrongEntity
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketplaceOrderHistory" /> class.
        /// </summary>
        public MarketplaceOrderHistory() : base(typeof(MarketplaceOrderHistory)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketplaceOrderHistory" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal MarketplaceOrderHistory(IActivationData activationData) : base(activationData) { }

        #endregion

        /// <summary>
        /// The original raw message request that was initially received to create the marketplace order.
        /// </summary>
        public string Message
        {
            get { return (string)GetField(MarketplaceOrderHistorySchema.MessageField); }
            set { SetField(MarketplaceOrderHistorySchema.MessageField, value); }
        }

        /// <summary>
        /// The tenant that the order related to.
        /// </summary>
        public IManagedTenant Tenant
        {
            get { return GetRelationships<ManagedTenant>(MarketplaceOrderHistorySchema.TenantLookup, Direction.Forward).FirstOrDefault(); }
            set { SetRelationships(MarketplaceOrderHistorySchema.TenantLookup, value == null ? null : new EntityRelationship<ManagedTenant>((ManagedTenant)value).ToEntityRelationshipCollection(), Direction.Forward); }
        }

        /// <summary>
        /// The products that were purchased as part of the order.
        /// </summary>
        public IEntityCollection<MarketplaceProduct> Purchased
        {
            get { return GetRelationships<MarketplaceProduct>(MarketplaceOrderHistorySchema.PurchasedRelationship, Direction.Forward).Entities; }
            set { SetRelationships(MarketplaceOrderHistorySchema.PurchasedRelationship, value.ToEntityRelationshipCollection(), Direction.Forward); }
        }
    }
}
