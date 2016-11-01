// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogViewer.Common
{
    /// <summary>
    /// 
    /// </summary>
    internal class ResourceObjectInfo : ObjectInfo
    {
        #region Properties
        /// <summary>
        /// Gets or sets the tenant id.
        /// </summary>
        /// <value>
        /// The tenant id.
        /// </value>
        public Guid TenantId { get; set; }


        /// <summary>
        /// Gets or sets the name of the tenant.
        /// </summary>
        /// <value>
        /// The name of the tenant.
        /// </value>
        public string TenantName { get; set; }


        /// <summary>
        /// Gets or sets the type id.
        /// </summary>
        /// <value>
        /// The type id.
        /// </value>
        public Guid TypeId { get; set; }


        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        /// <value>
        /// The name of the type.
        /// </value>
        public string TypeName { get; set; }
        #endregion


        #region Public Methods
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder toolTipInfo = new StringBuilder();
            toolTipInfo.AppendLine(string.Format("Resource Id: {0}", Id.ToString()));
            toolTipInfo.AppendLine(string.Format("Name: {0}", Name));
            if (!string.IsNullOrEmpty(TypeName))
            {
                toolTipInfo.AppendLine(string.Format("Type Name: {0}", TypeName));
            }
            else
            {
                toolTipInfo.AppendLine(string.Format("Type Id: {0}", TypeId.ToString()));
            }
            if (!string.IsNullOrEmpty(SolutionName))
            {
                toolTipInfo.AppendLine(string.Format("Solution Name: {0}", SolutionName));
            }
            else
            {
                toolTipInfo.AppendLine(string.Format("Solution Id: {0}", SolutionId.ToString()));
            }
            if (!string.IsNullOrEmpty(TenantName))
            {
                toolTipInfo.AppendLine(string.Format("Tenant Name: {0}", TenantName));
            }
            else
            {
                toolTipInfo.AppendLine(string.Format("Tenant Id: {0}", TenantId.ToString()));
            }

            return toolTipInfo.ToString();
        }
        #endregion
    }
}
