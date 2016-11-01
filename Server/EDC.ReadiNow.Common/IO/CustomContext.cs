// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.IO
{
	/// <summary>
	///     Sets the request context for the duration of this objects lifetime on the current thread.
	///     Once disposed, the original calling context is restored.
	/// </summary>
	public class CustomContext : ContextBlock
	{
		/// <summary>
		///     Initializes a new instance of the CustomContext class.
		/// </summary>
		/// <param name="identity">
		///     Identity info.
		/// </param>
		/// <param name="tenant">
		///     Tenant info.
		/// </param>
		/// <param name="culture">
		///     Culture name.
		/// </param>
		public CustomContext( IdentityInfo identity, TenantInfo tenant, string culture )
			: base( ( ) => RequestContext.SetContext( identity, tenant, culture ) )
		{
		}

		/// <summary>
		///     Initializes a new instance of the CustomContext class.
		/// </summary>
		/// <param name="context">
		///     Context.
		/// </param>
		public CustomContext( RequestContext context )
			: base( ( ) => RequestContext.SetContext( context ) )
		{
		}

		/// <summary>
		///     Initializes a new instance of the CustomContext class.
		/// </summary>
		/// <param name="contextData">
		///     Context data.
		/// </param>
		public CustomContext( RequestContextData contextData )
			: base( ( ) => RequestContext.SetContext( contextData ) )
		{
		}

		/// <summary>
		///     Initializes a new instance of the CustomContext class.
		/// </summary>
		/// <param name="identity">
		///     Identity info.
		/// </param>
		/// <param name="tenant">
		///     Tenant info.
		/// </param>
		/// <param name="culture">
		///     Culture name.
		/// </param>
		public static CustomContext SetContext( IdentityInfo identity, TenantInfo tenant, string culture )
		{
			return new CustomContext( identity, tenant, culture );
		}

		/// <summary>
		///     Initializes a new instance of the CustomContext class.
		/// </summary>
		/// <param name="context">
		///     Context.
		/// </param>
		public static CustomContext SetContext( RequestContext context )
		{
			return new CustomContext( context );
		}

		/// <summary>
		///     Initializes a new instance of the CustomContext class.
		/// </summary>
		/// <param name="contextData">
		///     Context data.
		/// </param>
		public static CustomContext SetContext( RequestContextData contextData )
		{
			return new CustomContext( contextData );
		}
	}
}