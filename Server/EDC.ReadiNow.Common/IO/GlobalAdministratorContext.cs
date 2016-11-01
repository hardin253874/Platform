// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.IO
{
	/// <summary>
	///     Sets the global administrator context.
	/// </summary>
	public class GlobalAdministratorContext : AdministratorContext
	{
		/// <summary>
		///     Default constructor for the AdministratorContext object.
		/// </summary>
		public GlobalAdministratorContext( )
		{
		}

		/// <summary>
		///     Default constructor for the AdministratorContext object.
		/// </summary>
		/// <param name="culture">
		///     String representing the current culture.
		/// </param>
		public GlobalAdministratorContext( string culture )
			: base( culture )
		{
		}
	}
}