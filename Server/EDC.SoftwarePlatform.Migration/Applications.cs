// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.SoftwarePlatform.Migration
{
	/// <summary>
	///     Application constants.
	/// </summary>
	public static class Applications
	{
		/// <summary>
		///     The core application identifier.
		/// </summary>
		public static readonly Guid CoreApplicationId = new Guid( "7062aade-2e72-4a71-a7fa-a412d20d6f01" );

		/// <summary>
		///     The Console application identifier
		/// </summary>
		public static readonly Guid ConsoleApplicationId = new Guid( "34ff4d95-70c6-4ae8-8f6f-38d88546d4c4" );

		/// <summary>
		///     The CoreData application identifier
		/// </summary>
		public static readonly Guid CoreDataApplicationId = new Guid( "abf12077-6fa5-43da-b608-b8b7514d07bb" );

		/// <summary>
		///     The Shared application identifier
		/// </summary>
		public static readonly Guid SharedApplicationId = new Guid( "50380499-2857-474d-92bf-6007303855f1" );

		/// <summary>
		///     The Test application identifier
		/// </summary>
		public static readonly Guid TestApplicationId = new Guid( "5f7eb596-1f47-409d-a4d8-33c2e16b079f" );
	}
}