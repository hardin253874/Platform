// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.Services.ApplicationManager
{
	/// <summary>
	///     Holds information about an installed application.
	/// </summary>
	[DataContract]
	public class InstalledApplication : AvailableApplication
	{
		/// <summary>
		///     The identifier of the entity that represents the solution where installed.
		/// </summary>
		[DataMember( Name = "sid" )]
		public long SolutionEntityId
		{
			get;
			set;
		}

		/// <summary>
		///     The version of the solution.
		/// </summary>
		[DataMember( Name = "solutionVersion" )]
		public string SolutionVersion
		{
			get;
			set;
		}
	}
}