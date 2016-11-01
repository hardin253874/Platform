﻿// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Model
{
	/// <summary>
	///     DataDecimalInfo class.
	/// </summary>
	public class DataDecimalInfo : FieldInfo<decimal>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DataDecimalInfo" /> class.
		/// </summary>
		/// <param name="databaseManager">The database manager.</param>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="fieldUpgradeId">The field upgrade identifier.</param>
		/// <param name="name">The name.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="data">The data.</param>
		/// <param name="description">The description.</param>
		public DataDecimalInfo( DatabaseManager databaseManager, long entityId, long tenantId, long fieldId, Guid fieldUpgradeId, string name, string alias, decimal data, string description )
			: base( databaseManager, entityId, tenantId, fieldId, fieldUpgradeId, name, alias, data, description )
		{
		}

		/// <summary>
		///     Gets the name of the table.
		/// </summary>
		/// <value>
		///     The name of the table.
		/// </value>
		public override string TableName
		{
			get
			{
				return "Data_Decimal";
			}
		}
	}
}