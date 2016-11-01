// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Internal;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.CAST.Template.Model;

namespace EDC.ReadiNow.CAST.Marketplace.Model
{
    /// <summary>
    /// The managed customer object.
    /// </summary>
    [Serializable]
    [ModelClass(ManagedCustomerSchema.ManagedCustomerType)]
    public class ManagedCustomer : StrongEntity
    {
        #region Entity Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedCustomer" /> class.
        /// </summary>
        public ManagedCustomer() : base(typeof(ManagedCustomer)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedCustomer" /> class.
        /// </summary>
        /// <param name="activationData">The activation data.</param>
        internal ManagedCustomer(IActivationData activationData) : base(activationData) { }

        #endregion

        /// <summary>
        /// The name of the customer entity.
        /// </summary>
        public string Name
        {
            get { return (string)GetField(ManagedCustomerSchema.NameField); }
            set { SetField(ManagedCustomerSchema.NameField, value); }
        }

        /// <summary>
        /// The customer email.
        /// </summary>
        public string Email
        {
            get { return (string)GetField(ManagedCustomerSchema.EmailField); }
            set { SetField(ManagedCustomerSchema.EmailField, value); }
        }

        /// <summary>
        /// The first name of the customer.
        /// </summary>
        public string FirstName
        {
            get { return (string)GetField(ManagedCustomerSchema.FirstNameField); }
            set { SetField(ManagedCustomerSchema.FirstNameField, value); }
        }

        /// <summary>
        /// The last name of the customer.
        /// </summary>
        public string LastName
        {
            get { return (string)GetField(ManagedCustomerSchema.LastNameField); }
            set { SetField(ManagedCustomerSchema.LastNameField, value); }
        }
        
        /// <summary>
        /// The company name of the customer.
        /// </summary>
        public string Company
        {
            get { return (string)GetField(ManagedCustomerSchema.CompanyField); }
            set { SetField(ManagedCustomerSchema.CompanyField, value); }
        }
        
        /// <summary>
        /// The department that the customer operates in.
        /// </summary>
        public string Department
        {
            get { return (string)GetField(ManagedCustomerSchema.DepartmentField); }
            set { SetField(ManagedCustomerSchema.DepartmentField, value); }
        }
        
        /// <summary>
        /// The industry that the customer operates in.
        /// </summary>
        public string Industry
        {
            get { return (string)GetField(ManagedCustomerSchema.IndustryField); }
            set { SetField(ManagedCustomerSchema.IndustryField, value); }
        }
        
        /// <summary>
        /// The password belonging to the customer.
        /// </summary>
        public string Password
        {
            get { return (string)GetField(ManagedCustomerSchema.PasswordField); }
            set { SetField(ManagedCustomerSchema.PasswordField, value); }
        }

        /// <summary>
        /// A flag indicating if the customer has been verified.
        /// </summary>
        public bool Verified
        {
            get { return (bool)GetField(ManagedCustomerSchema.VerifiedField); }
            set { SetField(ManagedCustomerSchema.VerifiedField, value); }
        }

        /// <summary>
        /// An indicator of whom it was that verified this customer.
        /// </summary>
        public string VerifiedBy
        {
            get { return (string)GetField(ManagedCustomerSchema.VerifiedByField); }
            set { SetField(ManagedCustomerSchema.VerifiedByField, value); }
        }
        
        /// <summary>
        /// The company size.
        /// </summary>
        public string Size
        {
            get { return (string)GetField(ManagedCustomerSchema.SizeField); }
            set { SetField(ManagedCustomerSchema.SizeField, value); }
        }

        /// <summary>
        /// Telephone number.
        /// </summary>
        public string Telephone
        {
            get { return (string)GetField(ManagedCustomerSchema.TelephoneField); }
            set { SetField(ManagedCustomerSchema.TelephoneField, value); }
        }
        
        /// <summary>
        /// Tenants that belong to this customer.
        /// </summary>
        public IEntityCollection<ManagedTenant> Tenants
        {
            get { return GetRelationships<ManagedTenant>(ManagedCustomerSchema.HasTenantRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedCustomerSchema.HasTenantRelationship, value.ToEntityRelationshipCollection(), Direction.Forward); }
        }

        /// <summary>
        /// Products that the customer has purchased or is trialling.
        /// </summary>
        public IEntityCollection<MarketplaceProduct> HasPurchased
        {
            get { return GetRelationships<MarketplaceProduct>(ManagedCustomerSchema.HasPurchasedRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedCustomerSchema.HasPurchasedRelationship, value.ToEntityRelationshipCollection(), Direction.Forward); }
        }

        /// <summary>
        /// Applications that the customer has access to.
        /// </summary>
        public IEntityCollection<ManagedApp> CanAccess
        {
            get { return GetRelationships<ManagedApp>(ManagedCustomerSchema.CanAccessRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedCustomerSchema.CanAccessRelationship, value.ToEntityRelationshipCollection(), Direction.Forward); }
        }

        /// <summary>
        /// Payments that the customer has made.
        /// </summary>
        public IEntityCollection<MarketplacePayment> Payments
        {
            get { return GetRelationships<MarketplacePayment>(ManagedCustomerSchema.PaymentsRelationship, Direction.Forward).Entities; }
            set { SetRelationships(ManagedCustomerSchema.PaymentsRelationship, value.ToEntityRelationshipCollection(), Direction.Forward); }
        }
    }
}
