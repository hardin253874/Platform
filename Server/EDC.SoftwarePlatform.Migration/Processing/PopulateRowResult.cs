// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.ComponentModel;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Populate Row Result
	/// </summary>
	public enum PopulateRowResult
	{
		/// <summary>
		///     Successful
		/// </summary>
		[Description( "Success" )]
		Success = 0,

		/// <summary>
		///     Invalid Data
		/// </summary>
		[Description( "Invalid Data" )]
		InvalidData = 1,

		/// <summary>
		///     Active Reference
		/// </summary>
		[Description( "Active Reference" )]
		ActiveReference = 2,

		/// <summary>
		///     Missing Entity Dependency
		/// </summary>
		[Description( "Missing Entity Dependency" )]
		MissingEntityDependency = 3,

		/// <summary>
		///     The missing field dependency
		/// </summary>
		[Description( "Missing Field Dependency" )]
		MissingFieldDependency = 4,

		/// <summary>
		///     The missing from dependency
		/// </summary>
		[Description( "Missing From Dependency" )]
		MissingFromDependency = 5,

		/// <summary>
		///     The missing type dependency
		/// </summary>
		[Description( "Missing Type Dependency" )]
		MissingTypeDependency = 6,

		/// <summary>
		///     The missing to dependency
		/// </summary>
		[Description( "Missing To Dependency" )]
		MissingToDependency = 7,

		/// <summary>
		///     The missing previous lookup dependency
		/// </summary>
		[Description( "Missing Previous Lookup Dependency" )]
		MissingPreviousLookupDependency = 8,

		/// <summary>
		///     Restriction
		/// </summary>
		[Description( "Restriction" )]
		Restriction = 9,

        /// <summary>
		///     Ignore the row
		/// </summary>
		[Description("Ignore")]
        Ignore = 10
    }
}