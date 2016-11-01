// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDC.ReadiNow.Metadata.Solutions
{
    /// <summary>
    /// Represents information about registered solutions.
    /// </summary>
    [Serializable]
    public class SolutionInfo
    {
        private Guid _id = Guid.Empty;
        private string _name = string.Empty;
        private string _description = string.Empty;

        /// <summary>
        /// Initializes a new instance of the SolutionInfo class.
        /// </summary>
        public SolutionInfo()
        {

        }

        /// <summary>
        /// Initializes a new instance of the SolutionInfo class.
        /// </summary>
        public SolutionInfo(Guid id, string name, string description)
        {
            this._id = id;
            this._name = name;
            this._description = description;
        }

        /// <summary>
        /// Gets the ID uniquely identifying the solution.
        /// </summary>
        public Guid Id
        {
            get
            {
                return this._id;
            }
        }

        /// <summary>
        /// Gets the name of the solution.
        /// </summary>
        public string Name
        {
            get
            {
                return this._name;
            }
        }

        /// <summary>
        /// Gets the description of the solution.
        /// </summary>
        public string Description
        {
            get
            {
                return this._description;
            }
        }
    }
}
