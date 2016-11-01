// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Autofac;
using EDC.Exceptions;
using EDC.ReadiNow.CAST.Services;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Marketplace.WebApi.Contracts;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using System;
using System.Net.Http;

namespace EDC.ReadiNow.Marketplace.WebApi.Controllers
{
    /// <summary>
    /// Controller for some customer related marketplace activities.
    /// </summary>
    [RoutePrefix("v1/customer")]
    public class CustomerController : ApiController
    {
        #region Constructor

		/// <summary>
        /// Basic constructor.
		/// </summary>
        public CustomerController()
		{
		    MarketplaceServiceImpl = Factory.Current.Resolve<IMarketplaceService>();
		}

		#endregion

        #region Internal Properties

        /// <summary>
        /// Holds a handle to an instance of the marketplace service.
        /// </summary>
        internal IMarketplaceService MarketplaceServiceImpl { get; private set; }
        
        #endregion

        /// <summary>
        /// Gets customer information, if it exists, based on just an email address.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <returns>The customer information if one was found to match the email.</returns>
        [NoXsrfCheck]
        [Route("")]
        [HttpGet]
        public IHttpActionResult GetCustomer([FromUri] string email = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest();
            }

            var customer = MarketplaceServiceImpl.GetCustomer(email);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(new CustomerInfo
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PasswordHash = customer.Password,
                Arguments = new Dictionary<string, string>
                {
                    { "cast:department", customer.Department },
                    { "cast:company", customer.Company },
                    { "cast:size", customer.Size },
                    { "cast:telephone", customer.Telephone },
                    { "cast:industry", customer.Industry }
                }
            });
        }

        /// <summary>
        /// Registers a customer with the information provided.
        /// </summary>
        /// <param name="info">The registration information.</param>
        /// <returns>Basic http response.</returns>
        [NoXsrfCheck]
        [Route("")]
        [HttpPost]
        public IHttpActionResult RegisterCustomer([FromBody] CustomerInfo info)
        {
            if (info == null)
            {
                return BadRequest();
            }

            var customer = MarketplaceServiceImpl.CreateCustomer(info.Email, info.FirstName, info.LastName);
            customer.Password = info.PasswordHash;
            if (info.Arguments != null)
            {
                foreach (var key in info.Arguments.Keys)
                {
                    var value = info.Arguments[key];
                    switch (key)
                    {
                        case "cast:department":
                            customer.Department = value;
                            break;
                        case "cast:company":
                            customer.Company = value;
                            break;
                        case "cast:size":
                            customer.Size = value;
                            break;
                        case "cast:telephone":
                            customer.Telephone = value;
                            break;
                        case "cast:industry":
                            customer.Industry = value;
                          break;
                    }
                }
            }

            customer.Save();

			if ( customer.Tenants.Count <= 0 )
            {
                // Spec a tenant for the customer for when it is verified
                var tenant = MarketplaceServiceImpl.CreateTenant(customer);
                tenant.Save();

                // Allocate the tenant to a platform for eventual creation
                var platform = MarketplaceServiceImpl.GetPlatform(customer, tenant);
                if (platform != null)
                {
                    platform.ShouldContainTenants.Add(tenant);
                    platform.Save();
                }
            }

            return Ok();
        }

        /// <summary>
        /// Note: this may be called multiple times as the customer loves to reclick the same link ovr and over
        /// Moreover, we are going to poll using this .. from the verify email page until we get a "done" message with a URL
        /// </summary>
        /// <param name="verifyEmail"></param>
        /// <returns></returns>
        public HttpResponseMessage VerifyEmail([FromBody] VerifyEmail verifyEmail)
        {
            if (verifyEmail == null)
            {
                throw new ArgumentNullException("verifyEmail");
            }


            throw new NotImplementedException();

            // Responses?
            // - "tokenerror" unknown/bad token
            // - other failure modes??
            // - "processing" received and processing
            // - "done" done, ready to log in ... here's the URL..
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [Route("order")]
        [HttpPost]
        public HttpResponseMessage<long> Post([FromBody] MarketplaceOrder order)
        {
            /*
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }

            if (order.Customer == null)
            {
                throw new ArgumentNullException("order.Customer");
            }

            using (new RemoteManagementContext())
            {
                // Save the order as an entity for history and for processing
                var history = EntityModel.Create<MarketplaceOrderHistory>();
                history.Message = JsonConvert.SerializeObject(order, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                if (string.IsNullOrEmpty(order.Customer.Email))
                {
                    // just for now. poc.
                    throw new InvalidOperationException("An email is required to identify the customer.");
                }

                // Retrieve the customer or create it
                var customer = MarketplaceServiceImpl.GetCustomer(order.Customer.Email);

                if (!customer.Tenants.Any())
                {
                    // If no tenants specified, "spec" one for creation by the wf on an appropriate platform
                    var tenant = MarketplaceServiceImpl.CreateTenant(customer);

                    var platform = MarketplaceServiceImpl.GetPlatform(customer);
                    if (platform == null)
                    {
                        throw new InvalidOperationException("There are no appropriate platforms to deploy to at this time.");
                    }

                    platform.ShouldContainTenants.Add(tenant);
                    platform.Save();
                }

                // assume all versions of all apps are available on all platforms
                if (order.Purchase != null)
                {
                    history.Purchased = new EntityCollection<MarketplaceProduct>();
                    foreach (var purchase in order.Purchase)
                    {
                        var product = EntityModel.GetByField<MarketplaceProduct>(purchase.SKU, new EntityRef("cast:productSKUField")).FirstOrDefault();
                        if (product != null)
                        {
                            history.Purchased.Add(product);
                        }
                    }
                }

                // Link to the resolved entities
                history.Tenant = customer.Tenants.First();

                // Save the history entity to initiate the workflow
                history.Save();

                // Return the workflow run id? What does Pete want?
                return new HttpResponseMessage<long>(0L);
            }
             */
            return new HttpResponseMessage<long>(0L);
        }
    }
}