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
    internal class TypeObjectInfo : ObjectInfo
    {
        #region Properties
        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        /// <value>
        /// The name of the type.
        /// </value>
        public string TypeName { get; set; }


        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly.
        /// </value>
        public string AssemblyName { get; set; }
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
            toolTipInfo.AppendLine(string.Format("Type Id: {0}", Id.ToString()));
            toolTipInfo.AppendLine(string.Format("Name: {0}", Name));
            if (!string.IsNullOrEmpty(SolutionName))
            {
                toolTipInfo.AppendLine(string.Format("Solution Name: {0}", SolutionName));
            }
            else
            {
                toolTipInfo.AppendLine(string.Format("Solution Id: {0}", SolutionId.ToString()));
            }
            toolTipInfo.AppendLine(string.Format("Type Name: {0}", TypeName));
            toolTipInfo.AppendLine(string.Format("Assembly Name: {0}", AssemblyName));

            return toolTipInfo.ToString();
        }
        #endregion
    }
}
