// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ReadiNow.Expressions.Tree
{
    /// <summary>
    /// Helper context for Xml representation of expression trees.
    /// </summary>
    public class ExpressionXmlContext
    {
        private class Pair
        {
            public XElement Element;

            public int Id;
        }

        private int _nextRef;

        private readonly Dictionary<object, Pair> _visited = new Dictionary<object, Pair>( );

        /// <summary>
        /// Check an object to see if it has already been visited, and if not then generate Xml for it.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="xElementCallback">Callback to generate Xml.</param>
        /// <returns></returns>
        public XElement CheckElement( object obj, Func<XElement> xElementCallback )
        {
            Pair reference;
            if ( _visited.TryGetValue( obj, out reference ) )
            {
                if ( reference.Id == -1 )
                {
                    reference.Id = _nextRef++;
                    reference.Element?.Add( new XAttribute( "xid", reference.Id ) );
                }
                return new XElement( "Ref",
                    new XAttribute( "xid", reference.Id ) );
            }

            reference = new Pair { Id = -1 };
            _visited.Add( obj, reference );
            XElement res = xElementCallback( ); // note: same element may be accesed during recursion, so must be placed in dict prior to call.
            reference.Element = res;
            if ( reference.Id != -1)
                reference.Element.Add( new XAttribute( "xid", reference.Id ) );
            return res;
        }


        /// <summary>
        /// Helper function to generate an Xml element that contains a list of items.
        /// </summary>
        /// <param name="listName">Name of the Xml element to contain the list.</param>
        /// <param name="list">List to read values from.</param>
        /// <param name="nodeCallback">Callback to convert list items to Xml.</param>
        public XElement CreateList<T>( string listName, IEnumerable<T> list, Func<T, XElement> nodeCallback )
        {
            if ( listName == null )
                throw new ArgumentNullException( nameof( listName ) );
            if ( nodeCallback == null )
                throw new ArgumentNullException( nameof( nodeCallback ) );

            if (list == null)
                return new XElement( listName, new XAttribute("null", true) );

            var nodeArray = list.Select( nodeCallback ).Cast<object>().ToArray( );
            return new XElement( listName, nodeArray );
        }

    }
}
