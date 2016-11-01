// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Model
{
	public static class EntityRefStaticConverters
	{
		/// <summary>
		///     Helper method for turning IDs into entity refs.
		///     Cannot have an implicit cast unfortunately.
		/// </summary>
		public static IEnumerable<EntityRef> AsEntityRefs( this IEnumerable<long> ids )
		{
			return ids.Select( id => new EntityRef( id ) );
		}
	}
}