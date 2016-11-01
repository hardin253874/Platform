// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Properties that are global to a workflow context.
    /// </summary>
    public class ContextProperties
    {
        Dictionary<string, object> _properties = new Dictionary<string, object>();

        protected ContextProperties ParentProperties { get; set; }

        public ContextProperties()
        {
        }

        public ContextProperties(Dictionary<string, object> other)
        {
            _properties = new Dictionary<string, object>(other);

        }

        public object Find(string name)
        {
            object result;
            _properties.TryGetValue(name, out result);

            if (result == null && ParentProperties != null)
                return ParentProperties.Find(name);
            else
                return result;
        }

        public void Add(string name, object value)
        {
            _properties.Add(name, value);
        }

        public ContextProperties CreateChildContext()
        {
            var result = new ContextProperties();

            result.ParentProperties = this;

            return result;
        }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>(_properties);
        }
    }
}
