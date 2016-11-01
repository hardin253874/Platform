// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using EDC.ReadiNow.CAST.Template.Model;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Marketplace.Model
{
    /// <summary>
    /// The storage item for a link that was sent to someone to verify some piece of information.
    /// </summary>
    [Serializable]
    [ModelClass(VerificationLinkSchema.VerificationLinkType)]
    public class VerificationLink : StrongEntity
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationLink" /> class.
        /// </summary>
        public VerificationLink() : base(typeof(VerificationLink)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationLink" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal VerificationLink(IActivationData activationData) : base(activationData) { }
        
        #endregion

        /// <summary>
        /// The name of the link. Used to store the token that will be used to retrieve and verify the info.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(VerificationLinkSchema.NameField); }
            set { SetField(VerificationLinkSchema.NameField, value); }
        }

        /// <summary>
        /// The email address that this verification token was sent to.
        /// </summary>
        public string MailedTo
        {
            get { return (string)GetField(VerificationLinkSchema.MailedToField); }
            set { SetField(VerificationLinkSchema.MailedToField, value); }
        }

        /// <summary>
        /// The full address used in the verification link sent.
        /// </summary>
        public string URI
        {
            get { return (string)GetField(VerificationLinkSchema.UriField); }
            set { SetField(VerificationLinkSchema.UriField, value); }
        }
        
        /// <summary>
        /// The customer that required verification.
        /// </summary>
        public ManagedCustomer ManagedCustomer
        {
            get { return GetRelationships<ManagedCustomer>(VerificationLinkSchema.ManagedCustomerLookup, Direction.Forward).FirstOrDefault(); }
            set { SetRelationships(VerificationLinkSchema.ManagedCustomerLookup, value == null ? null : new EntityRelationship<ManagedCustomer>(value).ToEntityRelationshipCollection(), Direction.Forward); }
        }
    }
}
