// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Core.FeatureSwitch
{

    /// <summary>
    /// An interface to provide feature switch storage and retrieval.
    /// </summary>
    public interface IFeatureSwitch
    {
        /// <summary>
        /// List name and description of the features
        /// </summary>
        IEnumerable<FeatureInfo> List();

         /// <summary>
        /// Set a feature switch
        /// </summary>
        /// <param name="featureSwitchAlias">Alias of the feature switch</param>
        /// <param name="value">Value to set</param>
        /// <param name="scope">The scope to set the value</param>
        void Set(string featureSwitchAlias, bool value);

        /// <summary>
        /// Set a a set of feature switch. A comma separated list of features with optional '=value'. Defaults to true. 
        /// If the value is ommitted it is assumed to be true
        /// </summary>
        /// <param name="featureSwitchList">Alias of the feature switch</param>
        void Set(string featureSwitchList);

        /// <summary>
        /// Get a feature switch value, Only true if both the global and tenant setting is set. 
        /// </summary>
        /// <param name="featureSwitchAlias"></param>
        /// <returns>True if the switch is set on both the tenant and globally</returns>
        bool Get(string featureSwitchAlias);

        /// <summary>
        /// Get a comma separated list of activated features for the current tenant.
        /// </summary>
        /// <returns></returns>
        string GetFeatureListString();
    }




    public class FeatureInfo
    {
        public string Name;
        public string Description;
        public bool IsSet;
    }


}
