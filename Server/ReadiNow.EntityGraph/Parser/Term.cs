// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace ReadiNow.EntityGraph.Parser
{
	/// <summary>
	///     A node in the parse-tree result.
	///     May represent either a field or a relationship.
	/// </summary>
	/// <remarks>
	///     Note: the parser returns Terms, rather than Field and Relationship requests directly, because
	///     its not actually possible to determine if a term is a field or a relationship in the general case until we
	///     have parsed the entire request.
	/// </remarks>
	internal class Term
	{
        /// <summary>
        ///     True if there was an asterisk after this term.
        ///     (Or if the asterisk solely consists of an asterisk).
        /// </summary>
        public bool Asterisk
        {
            get;
            set;
        }

		/// <summary>
		///     Any children of this term. (I.e. relationships or fields that got followed)
		/// </summary>
		public List<Term> Children
		{
			get;
			set;
		}

		/// <summary>
		///     An ID number assigned to this term.
		/// </summary>
		public long Id
		{
			get;
			set;
		}

		/// <summary>
		///     The alias assigned to this term.
		/// </summary>
		public string Identifier
		{
			get;
			set;
		}

        /// <summary>
        ///     Indicates that we're accessing a variable.
        /// </summary>
        public bool VariableAccess
        {
            get;
            set;
        }

        /// <summary>
        ///     The term has a reversal marker before it.
        /// </summary>
        public bool Reverse
        {
            get;
            set;
        }

        /// <summary>
        ///     The term is a meta-data only marker.
        /// </summary>
        public bool MetadataOnly
        {
            get;
            set;
        }

        /// <summary>
        ///     The alias assigned to this term.
        /// </summary>
        public string AliasOrId
        {
            get { return Identifier ?? ( Id == 0 ? null : "#" + Id);  }
        }
	}
}