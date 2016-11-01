// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace ReadiNow.Expressions.Tree
{
    /// <summary>
    /// Represents an operator that is available in the language.
    /// One instance per type of operator per type.
    /// </summary>
    [XmlRoot]
    public class Operator : Function
    {
    }

}
