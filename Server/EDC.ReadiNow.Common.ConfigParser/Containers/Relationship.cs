// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDC.ReadiNow.Common.ConfigParser.Containers
{
    /// <summary>
    /// Holds the endpoints of a relationship.
    /// </summary>
    public class Relationship
    {
        // The source entity (one endpoint)
        public Entity From { get; set; }

        // The target entity (other endpoint)
        public Entity To { get; set; }

        // The type of the relationship
        public Entity Type { get; set; }

		// The solution entity
		public Entity SolutionEntity { get; set; }

        public override string ToString()
        {
            return string.Format("type: {0}, from: {1}, to: {2}", this.Type, this.From, this.To);
        }
    }
    
}
