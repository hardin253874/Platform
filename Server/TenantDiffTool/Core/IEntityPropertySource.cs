// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.ComponentModel;

namespace TenantDiffTool.Core
{
	/// <summary>
	///     IPropertySource interface
	/// </summary>
	public interface IEntityPropertySource
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
		///     Gets the entity field properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		void GetEntityFieldProperties( PropertyDescriptorCollection props, IDictionary<string, object> state );

		/// <summary>
		///     Gets the entity properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		void GetEntityProperties( PropertyDescriptorCollection props, IDictionary<string, object> state );

		/// <summary>
		///     Gets the entity relationship properties.
		/// </summary>
		/// <param name="props">The props.</param>
		/// <param name="state">The state.</param>
		void GetEntityRelationshipProperties( PropertyDescriptorCollection props, IDictionary<string, object> state );
	}
}