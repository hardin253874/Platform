// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.IO
{
	/// <summary>
	///     A list of the special machine folders.
	/// </summary>
	[Serializable]
	public enum SpecialMachineFolders
	{
		/// <summary>
		///     ReadiNow install directory.
		/// </summary>
		Install,

		/// <summary>
		///     ReadiNow binary folder.
		/// </summary>
		Bin,

		/// <summary>
		///     ReadiNow solutions folder.
		/// </summary>
		Solutions,

		/// <summary>
		///     ReadiNow log folder.
		/// </summary>
		Log,

		/// <summary>
		///     ReadiNow message queue.
		/// </summary>
		Queue,

		/// <summary>
		///     ReadiNow in message queue.
		/// </summary>
		InQueue,

		/// <summary>
		///     ReadiNow out message queue.
		/// </summary>
		OutQueue,

		/// <summary>
		///     ReadiNow bad message queue.
		/// </summary>
		BadQueue,

        /// <summary>
        ///     Mail drop directory for testing IMap.
        /// </summary>
        MailDrop,

		/// <summary>
		///		The configuration
		/// </summary>
		Configuration
	}

	/// <summary>
	///     A list of the special user folders.
	/// </summary>
	[Serializable]
	public enum SpecialUserFolders
	{
		/// <summary>
		///     ReadiNow solutions folder
		/// </summary>
		Solutions
	}
}