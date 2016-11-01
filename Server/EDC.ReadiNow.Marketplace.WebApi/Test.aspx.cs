// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Web.UI;
using EDC.ReadiNow.CAST.Common;
using EDC.ReadiNow.Marketplace.WebApi.Contracts;

namespace EDC.ReadiNow.Marketplace.WebApi
{
    /// <summary>
    /// Test page.
    /// </summary>
    public partial class Test : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Unnamed1_Click(object sender, EventArgs e)
        {
            Order();
        }

        static void Order()
        {
            var request = new RemoteServiceRequester {Server = "syd1dev34.entdata.local"};
            var order = new MarketplaceOrder
            {
                Customer = new CustomerInfo {Email = "abc@foo.com"},
                Purchase = new List<ProductInfo>
                {
                    new ProductInfo {SKU = "p101"},
                    new ProductInfo {SKU = "p102"}
                }
            };
            var response = request.Post("v1/order", order);
            var result = request.Deserialise<string>(response);
        }
    }
}