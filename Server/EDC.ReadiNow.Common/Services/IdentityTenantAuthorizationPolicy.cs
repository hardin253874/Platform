// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Policy;
using System.IdentityModel.Claims;
using System.Security;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Services
{
    /// <summary>
    /// 
    /// </summary>
    internal class IdentityTenantAuthorizationPolicy : IAuthorizationPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityTenantAuthorizationPolicy"/> class.
        /// </summary>
        /// <param name="identityInfo">The identity info.</param>
        /// <param name="tenantInfo">The tenant info.</param>
        public IdentityTenantAuthorizationPolicy(IdentityInfo identityInfo, TenantInfo tenantInfo)
        {
            if (identityInfo == null)
            {
                throw new ArgumentNullException("identityInfo");
            }

            if (tenantInfo == null)
            {
                throw new ArgumentNullException("tenantInfo");
            }

            Id = Guid.NewGuid().ToString();
            Issuer = ClaimSet.System;

            this.identityInfo = identityInfo;
            this.tenantInfo = tenantInfo;
        }


        /// <summary>
        /// Evaluates whether a user meets the requirements for this authorization policy.
        /// </summary>
        /// <param name="evaluationContext">An <see cref="T:System.IdentityModel.Policy.EvaluationContext"/> that contains the claim set that the authorization policy evaluates.</param>
        /// <param name="state">A <see cref="T:System.Object"/>, passed by reference that represents the custom state for this authorization policy.</param>
        /// <returns>
        /// false if the <see cref="M:System.IdentityModel.Policy.IAuthorizationPolicy.Evaluate(System.IdentityModel.Policy.EvaluationContext,System.Object@)"/> method for this authorization policy must be called if additional claims are added by other authorization policies to <paramref name="evaluationContext"/>; otherwise, true to state no additional evaluation is required by this authorization policy.
        /// </returns>
        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {         
            RequestContext.SetContext(identityInfo, tenantInfo, ServicesHelper.GetCurrentCulture(), ServicesHelper.GetClientTimeZone());
            return true;
        }


        #region Properties
        /// <summary>
        /// Gets a string that identifies this authorization component.
        /// </summary>
        /// <returns>A string that identifies this authorization component.</returns>
        public String Id
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets a claim set that represents the issuer of the authorization policy.
        /// </summary>
        /// <returns>A <see cref="T:System.IdentityModel.Claims.ClaimSet"/> that represents the issuer of the authorization policy.</returns>
        public ClaimSet Issuer
        {
            get;
            private set;
        }
        #endregion


        #region Fields
        private IdentityInfo identityInfo;
        private TenantInfo tenantInfo;
        #endregion
    } 
}
