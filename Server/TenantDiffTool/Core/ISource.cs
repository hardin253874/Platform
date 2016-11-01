// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using TenantDiffTool.SupportClasses.Diff;

namespace TenantDiffTool.Core
{
	/// <summary>
	///     ISource interface.
	/// </summary>
	public interface ISource : IDisposable
	{
		/// <summary>
		///     Gets or sets the context.
		/// </summary>
		/// <value>
		///     The context.
		/// </value>
		DatabaseContext Context
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <value>
		///     The data.
		/// </value>
		IList<Data> Data
		{
			get;
		}

		/// <summary>
		///     Gets the entities.
		/// </summary>
		/// <value>
		///     The entities.
		/// </value>
		IList<Entity> Entities
		{
			get;
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <value>
		///     The relationships.
		/// </value>
		IList<Relationship> Relationships
		{
			get;
		}

		/// <summary>
		///     Gets the source string.
		/// </summary>
		/// <returns></returns>
		string SourceString
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <returns></returns>
		IList<Data> GetData( );

		/// <summary>
		///     Gets the entities.
		/// </summary>
		/// <param name="excludeRelationshipInstances">
		///     if set to <c>true</c> [exclude relationship instances].
		/// </param>
		/// <returns></returns>
		IList<Entity> GetEntities( bool excludeRelationshipInstances );

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="excludeRelationshipInstances">
		///     if set to <c>true</c> [exclude relationship instances].
		/// </param>
		/// <returns></returns>
		IList<Relationship> GetRelationships( bool excludeRelationshipInstances );
	}
}