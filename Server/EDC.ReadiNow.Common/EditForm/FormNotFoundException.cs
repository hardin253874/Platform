// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.EditForm
{
    /// <summary>
    /// Exception that is thrown if a form cannot be found.
    /// </summary>
    public class FormNotFoundException : WebArgumentNotFoundException
    {
    }
}
