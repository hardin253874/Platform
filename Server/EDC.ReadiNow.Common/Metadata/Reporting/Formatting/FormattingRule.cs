// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Reporting.Formatting
{
    /// <summary>
    /// Defines the base class for report column formatting rules.
    /// </summary>
    // type: formattingRule
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [KnownType(typeof(BarFormattingRule))]
    [KnownType(typeof(ColorFormattingRule))]
    [KnownType(typeof(IconFormattingRule))]
    [KnownType(typeof(ImageFormattingRule))]    
    public abstract class FormattingRule
    {
    }
}
