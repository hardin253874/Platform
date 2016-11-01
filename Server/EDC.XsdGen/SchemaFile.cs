// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;

namespace EDC.ReadiNow.XsdGen
{
    public class SchemaFile
    {
        public string Path { get; set; }

        public string TempPath { get; set; }

        public string Namespace { get; set; }

        public XmlSchema XmlSchema { get; set; }
    }
}
