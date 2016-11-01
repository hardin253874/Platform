// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Core.Cache.Providers;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Core.FeatureSwitch
{
    /// <summary>
    /// An implementation of IFeatureSwitch that stores the settings in the entity model and relies on Entity caching for performance.
    /// </summary>
    public class EntityFeatureSwitch : IFeatureSwitch
    {

        /// <summary>
        /// Get a feature switch value, Only true if both the global and tenant setting is set. 
        /// </summary>
		/// <param name="featureName"></param>
        /// <returns>True if the switch is set on both the tenant and globally</returns>
        public bool Get(string featureName)
        {

            bool result = false;

            ActInScope( () =>
            {

                var fsEntity = GetFeatureEntity();
                var feature = StringToFeatures(fsEntity.FsSwitches).FirstOrDefault(f => f.Name == featureName);

                if (feature != null)
                    result = feature.IsSet;
            });

            return result;
        }

        /// <summary>
        /// Set a set of feature switchs
        /// </summary>
        /// <param name="featureSwitchList">List of features.</param>
        /// <param name="value">Value to set</param>
        /// <param name="scope">The scope to set the value</param>
        public void Set(string featureSwitchList)
        {
            if (string.IsNullOrEmpty(featureSwitchList))
                return;

            var features = featureSwitchList.Split(',').Select(f =>
            {
                var split = f.Split('=');
                bool value = true;          // defaulting to true

                if (split.Length >= 2)
                {
                    var trimmed = split[1].Trim();

                    try
                    {
                        value = trimmed == string.Empty ? true : value = Boolean.Parse(split[1]);
                    }
                    catch (FormatException)
                    {
                        throw new ArgumentException($"Unable to parse value for {split[0]}");
                    }
                }
                return new FeatureInfo
                {
                    Name = split[0],
                    IsSet = value
                };
            }).ToList();

            foreach (var feature in features)
            {
                Set(feature.Name, feature.IsSet);
            }

            CacheManager.ClearCaches();

            TenantHelper.Invalidate( );
        }

        /// <summary>
        /// Set a feature switch
        /// </summary>
        /// <param name="featureName">Name of the feature.</param>
        /// <param name="value">Value to set</param>
        /// <param name="scope">The scope to set the value</param>
        public void Set(string featureName, bool value)
        {
            TestFsName(featureName);

            ActInScope( () =>
            {
                var fsEntity = GetFeatureEntity(true);
                var features = StringToFeatures(fsEntity.FsSwitches);
                var feature = features.FirstOrDefault(f => f.Name == featureName);

                if (feature != null)
                {
                    feature.IsSet = value;
                }
                else
                {
                    features.Add(new FeatureInfo { Name = featureName, Description = string.Empty, IsSet = value });
                }

                fsEntity.FsSwitches = FeaturesToString(features);
                fsEntity.Save();
            });
        }

        void TestFsName(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Feature switch name must not be empty");

            if (name.Contains(","))
                throw new ArgumentException("Feature switch name cannot contain a comma");

            if (name.Contains("|"))
                throw new ArgumentException("Feature switch name cannot contain a pipe symble (|)");
        }
        

        public void ActInScope(Action act)
        {
            var tenantInfo = RequestContext.GetContext().Tenant;

            using (new TenantAdministratorContext(tenantInfo.Id))
            {
                act();
            }
        }

        /// <summary>
        /// List the name and description of the features
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FeatureInfo> List()
        {
            IEnumerable<FeatureInfo> result = null;

            ActInScope( () => result = StringToFeatures(GetFeatureEntity().FsSwitches));

            return result;
        }

        List<FeatureInfo> StringToFeatures(string s)
        {
            return s.Split('|').Select(f =>
            {
                var split = f.Split(',');
                return new FeatureInfo {
                    Name = split[0],
                    Description = split.Length >= 2 ? split[1] : string.Empty,
                    IsSet = split.Length >= 3 ? Boolean.Parse(split[2]) : false };
            }).ToList();
        }


        string FeaturesToString(IEnumerable<FeatureInfo> features)
        {
            return String.Join("|", features.Select(f => string.Format("{0},{1},{2}", f.Name, f.Description, f.IsSet)).ToArray());
        }



        /// <summary>
        /// Get a comma separated list of activated features for the current tenant.
        /// </summary>
        /// <returns></returns>
        public string GetFeatureListString()
        {
            var onFeatures = OnFeatures();

            return String.Join(",", onFeatures);
        }

        IEnumerable<string> OnFeatures()
        {
            return  List().Where(f => f.IsSet).Select(f => f.Name).ToList();
        }

        FeatureSwitchType GetFeatureEntity(bool writable = false)
        {
            return Entity.Get<FeatureSwitchType>(new EntityRef("core:featureSwitch"), writable); // Cant use well known aliases here as we don't know which tenant
        }

    }
}
