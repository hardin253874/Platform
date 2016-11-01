// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Common.ConfigParser.Containers;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     General Extensions
	/// </summary>
	public static class GeneralExtensions
	{
		/// <summary>
		///     Drops the system tenant only relationships.
		/// </summary>
		/// <param name="relationships">The relationships.</param>
		/// <returns></returns>
		public static IEnumerable<Relationship> DropSystemTenantOnlyRelationships( this IEnumerable<Relationship> relationships )
		{
			if ( relationships != null )
			{
				foreach ( Relationship relationship in relationships )
				{
					if ( relationship.Type.IsSystemTenantOnly( ) || relationship.From.IsSystemTenantOnly( ) || relationship.To.IsSystemTenantOnly( ) )
					{
						continue;
					}

					yield return relationship;
				}
			}
		}

		/// <summary>
		///     Determines whether the entity is for the system tenant only.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public static bool IsSystemTenantOnly( this Entity entity )
		{
			if ( entity == null )
			{
				return false;
			}

			if ( entity.Members.Any( m => m.MemberDefinition.Alias.Namespace == "core" && m.MemberDefinition.Alias.Value == "systemTenantOnly" ) )
			{
				return true;
			}

			if ( entity.Type != null && entity.Type.Entity != null && !( entity.Type.Alias.Namespace == "core" && entity.Type.Alias.Value == "type" ) )
			{
				return IsSystemTenantOnly( entity.Type.Entity );
			}

			return false;
		}
	}
}