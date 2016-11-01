// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.SoftwarePlatform.Migration.Contract.Statistics.Failure
{
	/// <summary>
	///     Entity migration failure.
	/// </summary>
	public class EntityFailure : MigrationFailure
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EntityFailure" /> class.
		/// </summary>
		/// <param name="entityUpgradeId">The entity upgrade unique identifier.</param>
		/// <param name="level">The level.</param>
		public EntityFailure( Guid entityUpgradeId, FailureLevel level )
			: base( level )
		{
			EntityUpgradeId = entityUpgradeId;
		}

		/// <summary>
		///     Gets the entity upgrade unique identifier.
		/// </summary>
		/// <value>
		///     The entity upgrade unique identifier.
		/// </value>
		public Guid EntityUpgradeId
		{
			get;
			private set;
		}
	}
}