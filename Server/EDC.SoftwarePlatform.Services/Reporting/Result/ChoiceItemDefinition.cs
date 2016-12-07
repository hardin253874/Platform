// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using ReadiNow.Reporting.Definitions;

namespace ReadiNow.Reporting.Result
{
    public class ChoiceItemDefinition
    {
        public long EntityIdentifier { get; set; }

        public string DisplayName { get; set; }

        public string BackgroundColor { get; set; }

        public string ForegroundColor { get; set; }

        public long? ImageEntityId { get; set; }
    }
}
