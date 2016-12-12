// Copyright 2011-2016 Global Software Innovation Pty Ltd
using DocumentFormat.OpenXml;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// An XML document token or tag. That is, either an element being opened, or an element being closed.
    /// </summary>
    class Token
    {
        public Token() { }

        public Token(OpenXmlElement sourceNode, bool isClose)
        {
            SourceNode = sourceNode;
            IsClose = isClose;
        }


        /// <summary>
        /// The XML element that is being represented.
        /// </summary>
        public OpenXmlElement SourceNode { get; }


        /// <summary>
        /// True if this is a close-tag.
        /// </summary>
        public bool IsClose { get; }


        /// <summary>
        /// True if this is an open-tag.
        /// </summary>
        public bool IsOpen
        {
            get { return !IsClose; }
        }

        /// <summary>
        /// Returns a string representation for debugging purposes.
        /// </summary>
        public override string ToString()
        {
            return (IsClose ? "</" : "<") + SourceNode.GetType().Name + ">";
        }

        #region Equality Tests

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Token tOther = (Token)obj;

            return SourceNode.Equals(tOther.SourceNode) && IsClose.Equals(tOther.IsClose);
        }


        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
			unchecked
			{
				int hash = 17;

				if ( SourceNode != null )
				{
					hash = hash * 92821 + SourceNode.GetHashCode( );
				}

				hash = hash * 92821 + IsClose.GetHashCode( );

				return hash;
			}
        }

        /// <summary>
        /// Override equality operator to ensure sensible behavior.
        /// </summary>
        public static bool operator ==(Token c1, Token c2)
        {
            if (ReferenceEquals(c1, null) && ReferenceEquals(c2, null))
                return true;
            if (ReferenceEquals(c1, null) || ReferenceEquals(c2, null))
                return false;
            return c1.Equals(c2);
        }

        /// <summary>
        /// Override inequality operator to ensure sensible behavior.
        /// </summary>
        public static bool operator !=(Token c1, Token c2)
        {
            if (ReferenceEquals(c1, null) && ReferenceEquals(c2, null))
                return false;
            if (ReferenceEquals(c1, null) || ReferenceEquals(c2, null))
                return true;
            return !c1.Equals(c2);
        }
        #endregion

    }
}
