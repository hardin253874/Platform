// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Model.Client
{
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum DataState
    {
		[EnumMember( Value = "unchanged" )]
		Unchanged,

		[EnumMember( Value = "create" )]
		Create,

		[EnumMember( Value = "update" )]
		Update,

		[EnumMember( Value = "delete" )]
		Delete
    }
}
