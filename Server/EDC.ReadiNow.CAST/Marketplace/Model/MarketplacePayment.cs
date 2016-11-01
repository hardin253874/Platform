// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.CAST.Template.Model;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Marketplace.Model
{
    /// <summary>
    /// A payment that has been made from the marketplace.
    /// </summary>
    [Serializable]
    [ModelClass(MarketplacePaymentSchema.MarketplacePaymentType)]
    public class MarketplacePayment : StrongEntity
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketplacePayment" /> class.
        /// </summary>
        public MarketplacePayment() : base(typeof(MarketplacePayment)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketplacePayment" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal MarketplacePayment(IActivationData activationData) : base(activationData) { }

        #endregion
    }
}
