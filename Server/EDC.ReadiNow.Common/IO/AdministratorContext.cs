// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.IO
{
	/// <summary>
	///     Sets the administrators context for the duration of this objects lifetime on the current thread.
	///     Once disposed, the original calling context is restored.
	/// </summary>
	public class AdministratorContext : ContextBlock
	{
		/// <summary>
		///     Default constructor for the AdministratorContext object.
		/// </summary>
		public AdministratorContext( )
			: base( RequestContext.SetSystemAdministratorContext )
		{
		}

		/// <summary>
		///     Default constructor for the AdministratorContext object.
		/// </summary>
		/// <param name="culture">
		///     String representing the current culture.
		/// </param>
		public AdministratorContext( string culture )
			: base( ( ) => RequestContext.SetSystemAdministratorContext( culture ) )
		{
		}
	}
}