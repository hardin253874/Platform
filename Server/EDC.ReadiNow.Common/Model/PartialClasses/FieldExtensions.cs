// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.ReadiNow.Diagnostics;

namespace EDC.ReadiNow.Model.PartialClasses
{
	/// <summary>
	///     Class representing the FieldExtensions type.
	/// </summary>
	public static class FieldExtensions
	{
		/// <summary>
		///     Determines whether the field is application to the specified entity.
		/// </summary>
		/// <param name="field">The field.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="throwIfNotApplicable">if set to <c>true</c> [throw if not applicable].</param>
		/// <returns></returns>
		public static bool IsApplicableToEntity( this Field field, IEntity entity, bool throwIfNotApplicable = false )
		{
			bool valid = true;

			try
			{
				var fieldOnType = field?.FieldIsOnType;

				if ( fieldOnType != null && entity != null && entity.TypeIds != null )
				{
					var entityTypeIds = entity.TypeIds.ToList( );

					if ( entity.IsTemporaryId )
					{
						IEntityFieldValues fields;
						IDictionary<long, IChangeTracker<IMutableIdKey>> fwdRels;
						IDictionary<long, IChangeTracker<IMutableIdKey>> revRels;
						entity.GetChanges( out fields, out fwdRels, out revRels );

						IChangeTracker<IMutableIdKey> inherits;
						if (fwdRels != null && fwdRels.TryGetValue( WellKnownAliases.CurrentTenant.Inherits, out inherits ) )
						{
							if ( inherits != null && inherits.Count > 0 )
							{
								foreach ( IMutableIdKey key in inherits )
								{
									entityTypeIds.Add( key.Key );
								}
							}
						}
					}

					if ( entityTypeIds.Any( ) )
					{
						bool found = false;

						foreach ( long entityTypeId in entityTypeIds )
						{
							if ( PerTenantEntityTypeCache.Instance.IsDerivedFrom( entityTypeId, fieldOnType.Id ) )
							{
								found = true;
								break;
							}
						}

						valid = found;
					}
				}
			}
			catch ( Exception exc )
			{
				EventLog.Application.WriteError( exc.ToString( ) );
			}

			if ( throwIfNotApplicable && !valid )
			{
				var entityType = new EntityRef( entity.TypeIds.First( ) ).Entity.As<EntityType>( );

				throw new FieldException( $"Field '{field.Name}' is not applicable to entity of type '{entityType.Name}'." );
			}

			return valid;
		}
	}
}