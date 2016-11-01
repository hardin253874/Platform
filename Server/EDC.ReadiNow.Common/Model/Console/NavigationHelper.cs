// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Model.Console
{
    /// <summary>
    /// Helper to generate links to object that will remain permanent even after an upgrade of the platform.
    /// </summary>
    public static class NavigationHelper
    {
        private const string resourceViewFormat = "https://{0}/sp/#/{1}/{2}/viewForm";

        /// <summary>
        /// Get a link to view a resource.
        /// </summary>
        /// <param name="resourceRef"></param>
        /// <returns></returns>
        public static Uri GetResourceViewUrl(EntityRef resourceRef)
        {
            var baseAddress = ConfigurationSettings.GetSiteConfigurationSection().SiteSettings.Address;
            var tenantName = RequestContext.GetContext().Tenant.Name;
            return new Uri(string.Format(resourceViewFormat, baseAddress, tenantName, resourceRef.Id));
        }
    }
}
