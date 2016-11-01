// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using TenantDiffTool.Core;
using TenantDiffTool.SupportClasses.Diff;

namespace TenantDiffTool.SupportClasses
{
	/// <summary>
	///     Empty source.
	/// </summary>
	public class Empty : ViewModelBase, ISource
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="Empty" /> class.
		/// </summary>
		/// <param name="context">The context.</param>
		public Empty( DatabaseContext context )
		{
			Context = context;
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <returns></returns>
		public IList<Data> GetData( )
		{
			Data = new List<Data>( );

			return Data;
		}

		/// <summary>
		///     Gets the entities.
		/// </summary>
		/// <param name="excludeRelationshipInstances">
		///     if set to <c>true</c> [exclude relationship instances].
		/// </param>
		/// <returns></returns>
		public IList<Entity> GetEntities( bool excludeRelationshipInstances )
		{
			Entities = new List<Entity>( );

			return Entities;
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="excludeRelationshipInstances">
		///     if set to <c>true</c> [exclude relationship instances].
		/// </param>
		/// <returns></returns>
		public IList<Relationship> GetRelationships( bool excludeRelationshipInstances )
		{
			Relationships = new List<Relationship>( );

			return Relationships;
		}

		/// <summary>
		///     Gets or sets the context.
		/// </summary>
		/// <value>
		///     The context.
		/// </value>
		public DatabaseContext Context
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the source string.
		/// </summary>
		/// <returns></returns>
		public string SourceString
		{
			get
			{
				return "empty:";
			}
			set
			{
			}
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose( )
		{
			if ( Context != null )
			{
				Context.Dispose( );
				Context = null;
			}
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <value>
		///     The data.
		/// </value>
		public IList<Data> Data
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the entities.
		/// </summary>
		/// <value>
		///     The entities.
		/// </value>
		public IList<Entity> Entities
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <value>
		///     The relationships.
		/// </value>
		public IList<Relationship> Relationships
		{
			get;
			private set;
		}
	}
}