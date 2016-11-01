// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.SoftwarePlatform.Migration.Sources
{
	/// <summary>
	///     Source Type.
	/// </summary>
	public enum SourceType
	{
		/// <summary>
		///     Unknown type
		/// </summary>
		Unknown,

		/// <summary>
		///     The application package
		/// </summary>
		AppPackage,

		/// <summary>
		///     The tenant
		/// </summary>
		Tenant,

        /// <summary>
        ///     A graph of data.
        /// </summary>
        DataExport
    }
}