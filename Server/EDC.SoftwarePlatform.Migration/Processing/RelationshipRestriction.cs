// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	public class RelationshipRestriction
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RelationshipRestriction" /> class.
		/// </summary>
		/// <param name="restrictionCheck">The restriction check.</param>
		public RelationshipRestriction( Func<RelationshipEntry, bool> restrictionCheck )
		{
			if ( restrictionCheck == null )
			{
				throw new ArgumentNullException( "restrictionCheck" );
			}

			RestrictionCheck = restrictionCheck;
		}

		/// <summary>
		///     Gets or sets the restriction check.
		/// </summary>
		/// <value>
		///     The restriction check.
		/// </value>
		public Func<RelationshipEntry, bool> RestrictionCheck
		{
			get;
			set;
		}

		/// <summary>
		///     Determines whether the specified entry is allowed.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <returns></returns>
		public bool IsAllowed( RelationshipEntry entry )
		{
			return RestrictionCheck( entry );
		}
	}
}