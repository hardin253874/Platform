// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.SoftwarePlatform.Migration.Test.Sql
{
	/// <summary>
	///     The RelationshipResult class.
	/// </summary>
	public class RelationshipResult
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RelationshipResult" /> class.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="fromId">From identifier.</param>
		/// <param name="toId">To identifier.</param>
		public RelationshipResult( long typeId, long fromId, long toId )
		{
			TypeId = typeId;
			FromId = fromId;
			ToId = toId;
		}

		/// <summary>
		///     Gets or sets the type identifier.
		/// </summary>
		/// <value>
		///     The type identifier.
		/// </value>
		public long TypeId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets from identifier.
		/// </summary>
		/// <value>
		///     From identifier.
		/// </value>
		public long FromId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets to identifier.
		/// </summary>
		/// <value>
		///     To identifier.
		/// </value>
		public long ToId
		{
			get;
			set;
		}
	}

	/// <summary>
	///     The ExpectedRelationship class.
	/// </summary>
	public class ExpectedRelationship
	{
		/// <summary>
		///     Gets or sets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		public EntityTypes Type
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the type alias.
		/// </summary>
		/// <value>
		///     The type alias.
		/// </value>
		public string TypeAlias
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets from.
		/// </summary>
		/// <value>
		///     From.
		/// </value>
		public EntityTypes From
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets from alias.
		/// </summary>
		/// <value>
		///     From alias.
		/// </value>
		public string FromAlias
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets to.
		/// </summary>
		/// <value>
		///     To.
		/// </value>
		public EntityTypes To
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets to alias.
		/// </summary>
		/// <value>
		///     To alias.
		/// </value>
		public string ToAlias
		{
			get;
			set;
		}
	}
}