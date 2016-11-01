// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Package format
	/// </summary>
	public enum Format
	{
		/// <summary>
		///     undefined
		/// </summary>
		Undefined = 0,

		/// <summary>
		///     Sqlite format
		/// </summary>
		Sqlite = 1,

		/// <summary>
		///     Xml format
		/// </summary>
		Xml = 2,

        /// <summary>
        ///     Xml 2.0 format
        /// </summary>
        XmlVer2 = 3
    }
}