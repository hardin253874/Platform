// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.SoftwarePlatform.Migration.Test.Sql
{
	/// <summary>
	///     The EntityType enumeration.
	/// </summary>
	[Flags]
	public enum EntityTypes
	{
		/// <summary>
		///     The source application
		/// </summary>
		SourceApp = 1,

		/// <summary>
		///     The target application
		/// </summary>
		TargetApp = 2,

		/// <summary>
		///     The type a
		/// </summary>
		TypeA = 4,

		/// <summary>
		///     The type b
		/// </summary>
		TypeB = 8,

		/// <summary>
		///     The relationship
		/// </summary>
		Relationship = 16,

		/// <summary>
		///     The type a instance
		/// </summary>
		TypeAInstance = 32,

		/// <summary>
		///     The type b instance
		/// </summary>
		TypeBInstance = 64,

		/// <summary>
		/// The application dependency
		/// </summary>
		AppDependency = 128
	}
}