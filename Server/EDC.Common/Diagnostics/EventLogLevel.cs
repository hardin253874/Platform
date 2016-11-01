// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.Core;

namespace EDC.Diagnostics
{
	/// <summary>
	///     Defines the log levels used by the application event log.
	/// </summary>
	[DataContract( Namespace = Constants.DataContractNamespace )]
	public enum EventLogLevel
	{
		/// <summary>
		///     A trace entry, which indicates a minor operation or event.
		/// </summary>
		[EnumMember]
		Trace,

		/// <summary>
		///     An information entry, which indicates a significant, successful operation or event.
		/// </summary>
		[EnumMember]
		Information,

		/// <summary>
		///     A warning entry, which indicates a problem that is not immediately significant, but that may signify conditions that could cause future problems.
		/// </summary>
		[EnumMember]
		Warning,

		/// <summary>
		///     An error entry, which indicates a significant problem the user should know about; usually a loss of functionality or data.
		/// </summary>
		[EnumMember]
		Error
	}
}