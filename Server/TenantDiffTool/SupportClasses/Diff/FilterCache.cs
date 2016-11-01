// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.ComponentModel;

namespace TenantDiffTool.SupportClasses.Diff
{
	public class FilterCache
	{
		/// <summary>
		///     Attributes.
		/// </summary>
		public Attribute[ ] Attributes;

		/// <summary>
		///     Filtered properties.
		/// </summary>
		public PropertyDescriptorCollection FilteredProperties;

		/// <summary>
		///     Determines whether the specified other is valid.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns>
		///     <c>true</c> if the specified other is valid; otherwise, <c>false</c>.
		/// </returns>
		public bool IsValid( Attribute[ ] other )
		{
			if ( other == null || Attributes == null )
			{
				return false;
			}

			if ( Attributes.Length != other.Length )
			{
				return false;
			}

			for ( int i = 0; i < other.Length; i++ )
			{
				if ( !Attributes[ i ].Match( other[ i ] ) )
				{
					return false;
				}
			}

			return true;
		}
	}
}