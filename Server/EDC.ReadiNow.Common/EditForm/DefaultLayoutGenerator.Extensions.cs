// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.EditForm
{
    /// <summary>
    /// Extension methods related to edit form types.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Sets the vertical resize mode for a relationship control on the form.
        /// </summary>
        /// <param name="value">The relation ship control.</param>
        /// <param name="resizeMode">The resize mode as a string. E.g. resizeSpring</param>
        public static void SetRenderingVerticalResizeMode(this RelationshipControlOnForm value, string resizeMode)
        {
            value.SetRelationships("console:renderingVerticalResizeMode", new EntityRelationshipCollection<IEntity> {
                Entity.Get<IEntity>("console:" + resizeMode)
            }, Direction.Forward);
        }

        /// <summary>
        /// Sets the horizontal resize mode for a relationship control on the form.
        /// </summary>
        /// <param name="value">The relation ship control.</param>
        /// <param name="resizeMode">The resize mode as a string. E.g. resizeSpring</param>
        public static void SetRenderingHorizontalResizeMode(this RelationshipControlOnForm value, string resizeMode)
        {
            value.SetRelationships("console:renderingHorizontalResizeMode", new EntityRelationshipCollection<IEntity> {
                Entity.Get<IEntity>("console:" + resizeMode)
            }, Direction.Forward);
        }
    }
}
