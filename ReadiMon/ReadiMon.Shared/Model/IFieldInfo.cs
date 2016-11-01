// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace ReadiMon.Shared.Model
{
	/// <summary>
	///     IFieldInfo interface
	/// </summary>
	public interface IFieldInfo
	{
		/// <summary>
		///     Gets the alias.
		/// </summary>
		/// <value>
		///     The alias.
		/// </value>
		string Alias
		{
			get;
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <value>
		///     The data.
		/// </value>
		object Data
		{
			get;
		}

		/// <summary>
		///     Gets a value indicating whether this <see cref="IFieldInfo" /> is disabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if disabled; otherwise, <c>false</c>.
		/// </value>
		bool Disabled
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the edit view mode.
		/// </summary>
		/// <value>
		///     The edit view mode.
		/// </value>
		EditViewMode EditViewMode
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the entity identifier.
		/// </summary>
		/// <value>
		///     The entity identifier.
		/// </value>
		long EntityId
		{
			get;
		}

		/// <summary>
		///     Gets the field identifier.
		/// </summary>
		/// <value>
		///     The field identifier.
		/// </value>
		long FieldId
		{
			get;
		}

		/// <summary>
		///     Gets the field upgrade identifier.
		/// </summary>
		/// <value>
		///     The field upgrade identifier.
		/// </value>
		Guid FieldUpgradeId
		{
			get;
		}

		/// <summary>
		///     Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		bool IsReadOnly
		{
			get;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		string Name
		{
			get;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [new field].
		/// </summary>
		/// <value>
		///     <c>true</c> if [new field]; otherwise, <c>false</c>.
		/// </value>
		bool NewField
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the name of the table.
		/// </summary>
		/// <value>
		///     The name of the table.
		/// </value>
		string TableName
		{
			get;
		}

		/// <summary>
		///     Gets the tenant identifier.
		/// </summary>
		/// <value>
		///     The tenant identifier.
		/// </value>
		long TenantId
		{
			get;
		}
	}
}