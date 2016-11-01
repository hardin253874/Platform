// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using Quartz;

namespace EDC.ReadiNow.Scheduling
{
    /// <summary>
    /// A schedule Job that was services a scheduled item
    /// </summary>
    public interface IScheduledItemJob
    {
         void Execute(EntityRef scheduledItemRef);
    }
}
