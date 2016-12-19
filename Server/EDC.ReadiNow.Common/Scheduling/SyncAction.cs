// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Threading;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using System;
using System.Security.AccessControl;

namespace EDC.ReadiNow.Scheduling
{
    /// <summary>
    /// This class is really only used during testing. It's only here because it must be in a gaced assembly.
    /// </summary>
    public class SyncAction : ItemBase
    {
        protected override bool RunAsOwner
        {
            get
            {
                return true;
            }
        }

        static string GetName(EntityRef scheduledItemRef)
        {
            return "Global\\EDC.Scheduling.SyncAction-" + scheduledItemRef.Id;
        }


        public override void Execute(EntityRef scheduledItemRef)
        {
            var resource = Entity.Create<Resource>();
            resource.Name = scheduledItemRef.Id.ToString();
            resource.Save();
        }
    }
}