// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Migration.Contract.Statistics
{
	public enum AppLibraryAction
	{
		Unknown = 0,
		Delete = 1,
		Deploy = 2,
		Export = 3,
		Import = 4,
		Publish = 5,
		Repair = 6,
		Stage = 7,
		Transform = 8,
		Upgrade = 9,
        ChangeAccess = 10,
		Remove = 11,
		Convert = 12,
        InstallGlobal = 13,
        BootstrapImport = 14
    }
}
