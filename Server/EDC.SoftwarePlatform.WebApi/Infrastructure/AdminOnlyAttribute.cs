// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Admin Only Attribute class.
	/// </summary>
	/// <seealso cref="System.Web.Http.Filters.ActionFilterAttribute" />
	public class AdminOnlyAttribute : ActionFilterAttribute
	{
		/// <summary>
		///     Occurs before the action method is invoked.
		/// </summary>
		/// <param name="actionContext">The action context.</param>
		public override void OnActionExecuting( HttpActionContext actionContext )
		{
			if ( !IsAdmin( ) )
			{
				var response = actionContext.Request.CreateErrorResponse( HttpStatusCode.Forbidden, "Insufficient privileges." );
				actionContext.Response = response;
			}
		}

		/// <summary>
		///     Determines whether this instance is admin.
		/// </summary>
		/// <returns>
		///     <c>true</c> if this instance is admin; otherwise, <c>false</c>.
		/// </returns>
		private bool IsAdmin( )
		{
			long userId = RequestContext.UserId;

			var userRoleRepository = Factory.Current.Resolve<IUserRoleRepository>( );

			bool userIsAdmin = false;

			if ( userRoleRepository != null )
			{
				var subjectIds = new HashSet<long>( userRoleRepository.GetUserRoles( userId ) )
				{
					userId
				};

				userIsAdmin = subjectIds.Contains( WellKnownAliases.CurrentTenant.AdministratorRole );
			}

			return userIsAdmin;
		}
	}
}