// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using TenantDiffTool.Core;
using TenantDiffTool.SupportClasses.Diff;

namespace TenantDiffTool.SupportClasses
{
	/// <summary>
	///     File class.
	/// </summary>
	public abstract class File : ViewModelBase, ISource, IEntityPropertySource
	{
		/// <summary>
		///     Path.
		/// </summary>
		private string _path;

		/// <summary>
		///     Initializes a new instance of the <see cref="File" /> class.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="context">The context.</param>
		protected File( string path, DatabaseContext context )
		{
			Path = path;
			Context = context;
		}

		/// <summary>
		///     Gets the path.
		/// </summary>
		/// <value>
		///     The path.
		/// </value>
		public string Path
		{
			get
			{
				return _path;
			}
			set
			{
				if ( _path != value )
				{
					_path = value;
					RaisePropertyChanged( "Path" );
				}
			}
		}

		public abstract void GetEntityProperties( PropertyDescriptorCollection props, IDictionary<string, object> state );

		/// <summary>
		///     Gets the entity field properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		public abstract void GetEntityFieldProperties( PropertyDescriptorCollection props, IDictionary<string, object> state );

		/// <summary>
		///     Gets the entity relationship properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		public abstract void GetEntityRelationshipProperties( PropertyDescriptorCollection props, IDictionary<string, object> state );

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
		///     Gets the data.
		/// </summary>
		/// <returns></returns>
		public abstract IList<Data> GetData( );

		/// <summary>
		///     Gets the entities.
		/// </summary>
		/// <param name="excludeRelationshipInstances">
		///     if set to <c>true</c> [exclude relationship instances].
		/// </param>
		/// <returns></returns>
		public abstract IList<Entity> GetEntities( bool excludeRelationshipInstances );

		/// <summary>
		///     Gets the relationships.
		/// </summary>
		/// <param name="excludeRelationshipInstances">
		///     if set to <c>true</c> [exclude relationship instances].
		/// </param>
		/// <returns></returns>
		public abstract IList<Relationship> GetRelationships( bool excludeRelationshipInstances );

		/// <summary>
		///     Gets the source string.
		/// </summary>
		public string SourceString
		{
			get
			{
				return $"file://{Path}";
			}
			set
			{
			}
		}

		/// <summary>
		///     Clean up
		/// </summary>
		public virtual void Dispose( )
		{
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
			protected set;
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
			protected set;
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
			protected set;
		}

		/// <summary>
		///     Refers to upgrade identifier lookup.
		/// </summary>
		/// <param name="val">The value.</param>
		/// <param name="id">The identifier.</param>
		protected void ReferToUpgradeIdLookup( ref string val, Guid id )
		{
			if ( string.IsNullOrEmpty( val ) )
			{
				string tempVal;
				if ( UpgradeIdCache.Instance.TryGetValue( id, out tempVal ) )
				{
					val = tempVal;
				}
			}
		}
	}
}