// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.CAST.Marketplace.Model;
using EDC.ReadiNow.CAST.Model;

namespace EDC.ReadiNow.CAST.Marketplace.Services
{
    /// <summary>
    /// Describes the services available to the marketplace application.
    /// </summary>
    public interface IMarketplaceService
    {
        /// <summary>
        /// Gets the customer that has the given email address.
        /// </summary>
        /// <param name="email">The email address to use.</param>
        /// <returns>The customer that has the given address.</returns>
        ManagedCustomer GetCustomer(string email);

        /// <summary>
        /// Creates a new customer object (unsaved) with the key details set.
        /// </summary>
        /// <param name="email">The customer email.</param>
        /// <param name="firstName">The first name of the customer.</param>
        /// <param name="lastName">The last name of the customer.</param>
        /// <returns>The newly created customer.</returns>
        ManagedCustomer CreateCustomer(string email, string firstName, string lastName);

        /// <summary>
        /// Gets the platform that a customer should have its tenant(s) and subsequent applications
        /// deployed to.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="tenant">The specific tenant owned by the customer. (Optional.)</param>
        /// <returns>The managed platform that has been selected.</returns>
        ManagedPlatform GetPlatform(ManagedCustomer customer, ManagedTenant tenant = null);

        /// <summary>
        /// Creates a new tenant object (unsaved) for the customer with the name of the tenant derived from 
        /// the details given by the customer.
        /// </summary>
        /// <param name="customer">The customer to create the tenant for.</param>
        /// <returns>The newly created tenant object.</returns>
        ManagedTenant CreateTenant(ManagedCustomer customer);

        /// <summary>
        /// Gets the product that is identified by the given sku.
        /// </summary>
        /// <param name="sku">The stock keeping unit.</param>
        /// <returns>The marketplace product.</returns>
        IMarketplaceProduct GetProduct(string sku);
    }
}
