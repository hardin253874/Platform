// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Impl.AdoJobStore;

namespace EDC.ReadiNow.Scheduling
{
    /// <summary>
    /// This class is necessary to avoid issues where the standard (base class) does not load the 
    /// classes from the GAC.
    /// See: http://stackoverflow.com/questions/13142718/quartz-net-as-a-windows-service-with-dependencies-in-the-gac
    /// 
    /// Note that this could cause a problem fo upgrades if we ever change the versions number of an assembly.
    /// 
    /// The alternative is add assembly binding information to the appconfig.
    /// </summary>
    public class EdcDelegate: SqlServerDelegate
    {
        
        protected override string GetStorableJobTypeName(Type jobType)
        {
            return jobType.AssemblyQualifiedName;
        }

    }

   
}
