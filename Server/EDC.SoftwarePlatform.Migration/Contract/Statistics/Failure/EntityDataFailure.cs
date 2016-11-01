// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.SoftwarePlatform.Migration.Contract.Statistics.Failure
{
	/// <summary>
	///     Entity Data migration failure.
	/// </summary>
	public class EntityDataFailure : EntityFailure
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EntityDataFailure" /> class.
		/// </summary>
		/// <param name="entityUpgradeId">The entity upgrade unique identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade unique identifier.</param>
		/// <param name="level">The level.</param>
		public EntityDataFailure( Guid entityUpgradeId, Guid fieldUpgradeId, string dataTable, FailureLevel level )
			: base( entityUpgradeId, level )
		{
			DataTable = dataTable;
			FieldUpgradeId = fieldUpgradeId;
		}

		/// <summary>
		///     Gets the data table.
		/// </summary>
		/// <value>
		///     The data table.
		/// </value>
		public string DataTable
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the field upgrade unique identifier.
		/// </summary>
		/// <value>
		///     The field upgrade unique identifier.
		/// </value>
		public Guid FieldUpgradeId
		{
			get;
			private set;
		}
	}
}