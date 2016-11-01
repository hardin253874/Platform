// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Services.Console
{
    /// <summary>
    /// Holds information about the central target of an action definition.
    /// </summary>
    public class ActionTargetInfo
    {
        /// <summary>
        /// The entity considered as the target of the action.
        /// </summary>
        public IEntity Entity { get; set; }

        /// <summary>
        /// The name being used for the target in this context.
        /// </summary>
        public string Name { get; set; }
    }
}
