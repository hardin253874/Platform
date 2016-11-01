// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using Autofac;
using EDC.ReadiNow.CAST.Marketplace.Model;
using EDC.ReadiNow.CAST.Model;
using EDC.ReadiNow.CAST.Template.Model;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.CAST.Marketplace.Services
{
    /// <summary>
    /// Implements the services available to the marketplace application.
    /// </summary>
    public class MarketplaceService : IMarketplaceService
    {
        private ICastEntityHelper CastEntityHelper { get; set; }

        private IEntityRepository EntityRepository { get; set; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public MarketplaceService()
        {
            CastEntityHelper = Factory.Current.Resolve<ICastEntityHelper>();
            EntityRepository = Factory.Current.Resolve<IEntityRepository>();
        }

        /// <summary>
        /// Gets the customer that has the given email address.
        /// </summary>
        /// <param name="email">The email address to use.</param>
        /// <returns>The customer that has the given address.</returns>
        public ManagedCustomer GetCustomer(string email)
        {
            return default(ManagedCustomer);
            //if (string.IsNullOrEmpty(email))
            //{
            //    throw new ArgumentNullException("email");
            //}

            //if (!email.Contains('@'))
            //{
            //    throw new ArgumentException("The email address is not correctly structured.");
            //}

            //return EntityModel.GetByField<ManagedCustomer>(email, "cast:customerEmailField").FirstOrDefault();
        }
        
        /// <summary>
        /// Creates a new customer object (unsaved) with the key details set.
        /// </summary>
        /// <param name="email">The customer email.</param>
        /// <param name="firstName">The first name of the customer.</param>
        /// <param name="lastName">The last name of the customer.</param>
        /// <returns>The newly created customer.</returns>
        public ManagedCustomer CreateCustomer(string email, string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email");
            }

            var name = email;
            if (!string.IsNullOrEmpty(lastName))
            {
                name = lastName;
                if (!string.IsNullOrEmpty(firstName))
                {
                    name = firstName + " " + lastName;
                }
            }

            var managedCustomer = EntityRepository.Create<ManagedCustomer>();
            managedCustomer.Email = email;
            managedCustomer.FirstName = firstName;
            managedCustomer.LastName = lastName;
            managedCustomer.Name = name;

            //managedCustomer.Save();
            return managedCustomer;
        }

        /// <summary>
        /// Gets the platform that a customer should have its tenant(s) and subsequent applications
        /// deployed to.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="tenant">The specific tenant owned by the customer. (Optional.)</param>
        /// <returns>The managed platform that has been selected.</returns>
        public ManagedPlatform GetPlatform(ManagedCustomer customer, ManagedTenant tenant = null)
        {
            return default(ManagedPlatform);
            //         ManagedPlatform platform = null;

            //         // TODO: Maybe compare timezones first, then round-trip time or entity load?

            //         var platforms = Entity.GetInstancesOfType<ManagedPlatform>().ToList();
            //if ( platforms.Count > 0 )
            //         {
            //             platform = platforms.Where(p => p.LastContact != null)
            //                                 .OrderByDescending(p => p.LastContact)
            //                                 .FirstOrDefault() ?? platforms.First();
            //         }

            //         // writeable? bah.
            //         return platform != null ? platform.AsWritable<ManagedPlatform>() : null;
        }

        /// <summary>
        /// Creates a new tenant object (unsaved) for the customer with the name of the tenant derived from 
        /// the details given by the customer.
        /// </summary>
        /// <param name="customer">The customer to create the tenant for.</param>
        /// <returns>The newly created tenant object.</returns>
        public ManagedTenant CreateTenant(ManagedCustomer customer)
        {
            return default(ManagedTenant);
            //if (customer == null)
            //{
            //    throw new ArgumentNullException("customer");
            //}

            //var managedTenant = EntityRepository.Create<ManagedTenant>();
            //managedTenant.Customer = customer;

            //// I don't know if this is even necessary and it will most likely change.
            //// Whatever logic should be applied... stick it here.
            //var rgx = new Regex("[^A-Za-z0-9]");

            //var baseName = rgx.Replace(customer.Name, "").ToUpper();

            //// Use company if given.
            //if (!string.IsNullOrEmpty(customer.Company))
            //{
            //    var cleanCompany = rgx.Replace(customer.Company, "").ToUpper();
            //    if (!string.IsNullOrEmpty(cleanCompany))
            //    {
            //        baseName = cleanCompany;
            //    }
            //}

            //var tenantName = baseName;
            //var rnd = new Random();
            //while (EntityModel.GetByName<ManagedTenant>(tenantName).Any())
            //{
            //    tenantName = baseName + "_" + rnd.Next(100, 1000);
            //}

            //managedTenant.Name = tenantName;

            //return managedTenant;
        }

        /// <summary>
        /// Gets the product that is identified by the given sku.
        /// </summary>
        /// <param name="sku">The stock keeping unit.</param>
        /// <returns>The marketplace product.</returns>
        public IMarketplaceProduct GetProduct(string sku)
        {
            using (Profiler.Measure("MarketplaceService.GetProduct"))
            {
                if (string.IsNullOrEmpty(sku))
                {
                    throw new ArgumentNullException("sku");
                }

                return CastEntityHelper.GetEntityByField<MarketplaceProduct>(new EntityRef(MarketplaceProductSchema.SkuField), sku);
            }
        }
    }
}
