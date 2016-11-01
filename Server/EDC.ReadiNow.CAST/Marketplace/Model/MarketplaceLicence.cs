// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.CAST.Template.Model;

namespace EDC.ReadiNow.CAST.Marketplace.Model
{
    /// <summary>
    /// Describes a license component associated with any product available on the marketplace.
    /// </summary>
    [Serializable]
    [ModelClass(MarketplaceLicenceSchema.MarketplaceLicenceType)]
    public class MarketplaceLicence : StrongEntity
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketplaceLicence" /> class.
        /// </summary>
        public MarketplaceLicence() : base(typeof(MarketplaceLicence)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketplaceLicence" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal MarketplaceLicence(IActivationData activationData) : base(activationData) { }

        #endregion
    }
}
