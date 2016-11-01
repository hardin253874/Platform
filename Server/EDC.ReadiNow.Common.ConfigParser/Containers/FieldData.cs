// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDC.ReadiNow.Common.ConfigParser.Containers
{
    /// <summary>
    /// Used to return data held in a field
    /// </summary>
    public class FieldData
    {
        // The field definition
        public Entity Field { get; set; }

        // The data
        public string Value { get; set; }
    }
}
