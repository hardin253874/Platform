// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.EntityRequests;

namespace EDC.ReadiNow.EditForm
{
    /// <summary>
    /// Private classes and interfaces for DefaultLayoutGenerator.
    /// </summary>
    internal partial class DefaultLayoutGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        private interface INamedControl
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            string Name { get; set; }

            /// <summary>
            /// Gets or sets the control.
            /// </summary>
            /// <value>
            /// The control.
            /// </value>
            IEntity Control { get; set; }
        }


        /// <summary>
        /// 
        /// </summary>
        private class NamedRelationshipControl : INamedControl
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }


            /// <summary>
            /// Gets or sets the control.
            /// </summary>
            /// <value>
            /// The control.
            /// </value>
            public IEntity Control { get; set; }
        }


        /// <summary>
        /// 
        /// </summary>
        private class NamedFieldControl : INamedControl
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }


            /// <summary>
            /// Gets or sets the control.
            /// </summary>
            /// <value>
            /// The control.
            /// </value>
            public IEntity Control { get; set; }
        }


        private class RelationshipControls
        {
            #region Constructor
            /// <summary>
            /// Initializes a new instance of the <see cref="RelationshipControls" /> class.
            /// </summary>
            public RelationshipControls()
            {
                ForwardRelationshipControls = new List<NamedRelationshipControl>();
                ForwardTabRelationshipControls = new List<NamedRelationshipControl>();
                ReverseRelationshipControls = new List<NamedRelationshipControl>();
                ReverseTabRelationshipControls = new List<NamedRelationshipControl>();
                StructureViewRelationshipControls = new List<NamedRelationshipControl>();
            }
            #endregion


            #region Properties
            /// <summary>
            /// Gets or sets the forward relationship controls.
            /// </summary>
            /// <value>
            /// The forward relationship controls.
            /// </value>
            public List<NamedRelationshipControl> ForwardRelationshipControls { get; private set; }


            /// <summary>
            /// Gets or sets the forward tab relationship controls.
            /// </summary>
            /// <value>
            /// The forward tab relationship controls.
            /// </value>
            public List<NamedRelationshipControl> ForwardTabRelationshipControls { get; private set; }


            /// <summary>
            /// Gets or sets the reverse relationship controls.
            /// </summary>
            /// <value>
            /// The reverse relationship controls.
            /// </value>
            public List<NamedRelationshipControl> ReverseRelationshipControls { get; private set; }


            /// <summary>
            /// Gets or sets the reverse tab relationship controls.
            /// </summary>
            /// <value>
            /// The reverse tab relationship controls.
            /// </value>
            public List<NamedRelationshipControl> ReverseTabRelationshipControls { get; private set; }


            /// <summary>
            /// Gets or sets the structure view relationship controls.
            /// </summary>
            /// <value>
            /// The structure view relationship controls.
            /// </value>
            public List<NamedRelationshipControl> StructureViewRelationshipControls { get; private set; }
            #endregion
        }
    }
}

