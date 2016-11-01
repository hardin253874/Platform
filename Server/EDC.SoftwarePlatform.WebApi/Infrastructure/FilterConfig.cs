// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Web.Http.Filters;
using System.Web.Mvc;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	public static class FilterConfig
	{
		/// <summary>
		///     Registers the global MVC filters.
		/// </summary>
		/// <param name="filters">The filters.</param>
		public static void RegisterGlobalFilters( GlobalFilterCollection filters )
		{
			filters.Add( new HandleErrorAttribute( ) );
		}

		/// <summary>
		///     Registers the global HTTP API filters.
		/// </summary>
		/// <param name="filters">The filters.</param>
		public static void RegisterHttpFilters( HttpFilterCollection filters )
		{
			filters.Add( new ContextFilter( ) );
			filters.Add( new SecuredAttribute( ) );
			filters.Add( new ExceptionFilter( ) );
		}
	}
}