// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace EDC.ReadiNow.Test
{
	[AttributeUsage( AttributeTargets.Method )]
	public class RunWithoutTransactionAttribute : Attribute
	{
	}
}
