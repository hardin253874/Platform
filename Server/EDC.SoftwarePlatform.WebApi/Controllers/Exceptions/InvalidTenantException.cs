using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Exceptions
{
    /// <summary>
    /// Thrown when an invalid tenant was provided
    /// </summary>
    public class InvalidTenantException: Exception
    {
        public InvalidTenantException(string tenantName): base($"The provided tenant was invalid: '{tenantName}'")
        {
        }

        public InvalidTenantException() : base($"No tenant was provided")
        {
        }
    }
}