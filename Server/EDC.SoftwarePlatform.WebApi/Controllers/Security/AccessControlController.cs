// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System.Collections.Generic;
using System.Web.Http;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl.Diagnostics;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Security;
using EDC.Exceptions;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Security
{
    /// <summary>
    /// Web controller for misc Access Control requests.
    /// </summary>
    [RoutePrefix( "data/v1/accesscontrol" )]
    public class AccessControlController : ApiController
    {
        ITypeAccessReasonService TypeAccessReasonService { get; }
        IEntityRepository EntityRepository { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AccessControlController()
        {
            TypeAccessReasonService = Factory.Current.Resolve<ITypeAccessReasonService>( );
            EntityRepository = Factory.EntityRepository;
        }

        /// <summary>
        ///     Returns the types of records that can be (directly or indirectly) accessed by a role, and the reasons for the access.
        /// </summary>
        /// <param name="subjectAliasOrId">
        ///     The ID or the alias of the subject to check..
        /// </param>
        /// <param name="advanced">
        ///     Pass "true" to include advanced types. Namely managedType and resource. Otherwise only rules for "definition" gets shown.
        /// </param>
        /// <returns>
        ///     The ID of the <see cref="ReadiNow.Model.Role" /> or user account.
        /// </returns>
        [Route( "typeaccessreport/{subjectAliasOrId}" )]
        [HttpGet]
        public List<TypeAccessReason> Get( string subjectAliasOrId, [FromUri] string advanced = null )
        {
            EntityRef subjectRef = WebApiHelpers.GetId( subjectAliasOrId );

            // Demand read access to the admin role, as a proxy for determining who can use this service
            EntityRepository.Get( "core:administratorRole" );

            // Ensure that the requested ID is a role or user.
            Subject subject = EntityRepository.Get<Subject>( subjectRef.Id );
            if ( subject == null )
                throw new WebArgumentNotFoundException( "subject" );

            using ( new SecurityBypassContext( ) )
            {
                var settings = new TypeAccessReasonSettings( advanced == "true" );

                // Get list of reasons
                IReadOnlyList<AccessReason> reasons = TypeAccessReasonService.GetTypeAccessReasons( subjectRef.Id, settings);

                // Format result
                List<TypeAccessReason> result = new List<TypeAccessReason>( );
                foreach ( AccessReason reason in reasons )
                {
                    TypeAccessReason formattedReason = FormatAccessReason( reason );
                    result.Add( formattedReason );
                }

                return result;
            }
        }

        /// <summary>
        /// Convert access reason to web response contract.
        /// </summary>
        private TypeAccessReason FormatAccessReason( AccessReason reason )
        {
            // This could probably be better, but it'll largely come from cache
            long nameField = WellKnownAliases.CurrentTenant.Name;
            string subjectName = EntityRepository.Get( reason.SubjectId )?.GetField<string>( nameField );
            string typeName = EntityRepository.Get( reason.TypeId )?.GetField<string>( nameField );

            return new TypeAccessReason
            {
                SubjectName = subjectName,
                TypeName = typeName,
                Permissions = reason.PermissionsText,
                Reason = reason.Description,
                Scope = reason.AccessRuleScope.ToString()
            };
        }
    }
}