// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using EDC.ReadiNow.Common.ConfigParser.Containers;

namespace EDC.ReadiNow.Common.ConfigParser
{
    public class BuildException : Exception
    {
        public BuildException(string message)
            : base(message)
        {
            ErrorCode = "ZZ0000";
        }

        public BuildException(string message, Alias alias)
            : this(message)
        {
            if (alias != null)
            {
                this.File = alias.File;
                this.Line = alias.LineNumber;
                this.Column = alias.ColumnNumber;
            }
        }

        public BuildException(string message, Entity entity)
            : this(message)
        {
            if (entity != null && entity.Alias != null)
            {
                this.File = entity.Alias.File;
                this.Line = entity.Alias.LineNumber;
                this.Column = entity.Alias.ColumnNumber;
            }
        }

        public BuildException(string message, EntityRef entRef)
            : this(message)
        {
            if (entRef != null && entRef.Alias != null)
            {
                this.File = entRef.Alias.File;
                this.Line = entRef.Alias.LineNumber;
                this.Column = entRef.Alias.ColumnNumber;
            }
        }

        public BuildException(string message, XElement element)
            : this(message)
        {
            IXmlLineInfo elemInfo = (IXmlLineInfo)element;
            if (elemInfo.HasLineInfo())
            {
                this.Line = elemInfo.LineNumber;
                this.Column = elemInfo.LinePosition;
            }
            this.File = element.Document.BaseUri;
        }

        public string File { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string ErrorCode {get; set; }

        public string FormatMessage()
        {
            if (File == null)
            {
                return "Config error: " + Message;
            }
            else
            {
                string loc = FormatLocation(File, Line, Column);

                return string.Format("{0}: error E0: {1}", loc, Message);
            }
        }

        public static string FormatLocation(string file, int line, int column)
        {
            if (file.StartsWith("file:///"))
                file = file.Substring("file:///".Length);
            file = file.Replace("/", @"\");

            return string.Format("{0}({1},{2})", file, line, column);
        }

    }
}
