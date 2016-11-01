// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using EDC.Common;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using EDC.SoftwarePlatform.WebApi.Infrastructure;
using EDC.Exceptions;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Security
{
	[RoutePrefix( "data/v1/accessrule" )]
	public class AccessRulesController : ApiController
	{
		/// <summary>
		///     Create a new <see cref="AccessRulesController" />.
		/// </summary>
		public AccessRulesController( )
			: this( new AccessRuleFactory( ), new AccessRuleDisplayReportFactory( ) )
		{
			// Do nothing
		}

		/// <summary>
		///     Create a new <see cref="AccessRulesController" /> with the given
		///     <see cref="IAccessRuleFactory" /> for creating new access rules.
		/// </summary>
		/// <param name="accessRuleFactory">
		///     The <see cref="IAccessRuleFactory" /> to use. This cannot be null.
		/// </param>
		/// <param name="accessRuleReportFactory">
		///     The <see cref="IAccessRuleReportFactory" /> to use. This cannot be null.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     No argument can be null.
		/// </exception>
		internal AccessRulesController( IAccessRuleFactory accessRuleFactory, IAccessRuleReportFactory accessRuleReportFactory )
		{
			if ( accessRuleFactory == null )
			{
				throw new ArgumentNullException( "accessRuleFactory" );
			}
			if ( accessRuleReportFactory == null )
			{
				throw new ArgumentNullException( "accessRuleReportFactory" );
			}

			RuleFactory = accessRuleFactory;
			ReportFactory = accessRuleReportFactory;
		}

		/// <summary>
		///     An <see cref="IAccessRuleFactory" /> used to create access rules.
		/// </summary>
		public IAccessRuleFactory RuleFactory { get; }

		/// <summary>
		///     An <see cref="IAccessRuleReportFactory" /> used to create access rules.
		/// </summary>
		public IAccessRuleReportFactory ReportFactory { get; }

		/// <summary>
		///     Create a new access rule.
		/// </summary>
		/// <param name="accessRuleInfo">
		///     The details of the new rule. This cannot be null and must contain a
		///     valid type and subject (user or role).
		/// </param>
		/// <returns>
		///     The ID of the <see cref="ReadiNow.Model.AccessRule" /> object for the new rule.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="accessRuleInfo" /> cannot be null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="accessRuleInfo" /> must contain a valid
		///     <see cref="NewAccessRuleInfo.SecurableEntityId">type</see> and
		///     <see cref="NewAccessRuleInfo.SubjectId">user or role</see>.
		/// </exception>
		[Route("")]
        [HttpPost]
		public HttpResponseMessage<long> Post( [FromBody] NewAccessRuleInfo accessRuleInfo )
		{
			if ( accessRuleInfo == null )
			{
				throw new WebArgumentNullException( "accessRuleInfo" );
			}

		    SecurableEntity securableEntity;
		    Subject subject;
		    ReadiNow.Model.Report accessRuleReport;
		    AccessRule accessRule;
		    ISet<long> allowedTypes;

			securableEntity = ReadiNow.Model.Entity.Get<SecurableEntity>( accessRuleInfo.SecurableEntityId );
			if ( securableEntity == null )
			{
				throw new WebArgumentException( @"SecurableEntityId is not a type", "accessRuleInfo" );
			}

            // Restrict to the types visible in the UI. If modifying this, also change the code in accessControlRepository.
		    allowedTypes = new[] {"console:customEditForm", "core:workflow", "core:reportTemplate", "core:fileType", "core:documentType", "core:imageFileType"}
                .Select(ReadiNow.Model.Entity.GetId)
                .ToSet();
		    if (!(securableEntity.Is<ManagedType>()
                || allowedTypes.Contains(securableEntity.Id)))
		    {
                throw new WebArgumentException(@"SecurableEntityId is not a supported type", "accessRuleInfo");		        
		    }

			subject = ReadiNow.Model.Entity.Get<Subject>( accessRuleInfo.SubjectId, true );
			if ( subject == null )
			{
                throw new WebArgumentException(@"SubjectId is not a user or role", "accessRuleInfo");
			}

			accessRuleReport = ReportFactory.GetDisplayReportForSecurableEntity( securableEntity );
			if ( accessRuleReport == null )
			{
				throw new InvalidOperationException( "No report found for target securable entity" );
			}

            accessRule = new AccessRuleFactory().AddAllowByQuery(
				subject,
				securableEntity.As<SecurableEntity>( ),
				new[ ]
				{
					Permissions.Read
				},
				accessRuleReport,
				false // Disabled by default
				);

			return new HttpResponseMessage<long>( accessRule.Id );
		}
	}
}