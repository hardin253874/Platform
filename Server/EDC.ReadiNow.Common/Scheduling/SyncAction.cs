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
        static string GetName(EntityRef scheduledItemRef)
        {
            return "Global\\EDC.Scheduling.SyncAction-" + scheduledItemRef.Id;
        }

        static public EventWaitHandle CreateEventHandle(EntityRef scheduledItemRef)
        {
            var handleName = GetName(scheduledItemRef);

            var ewhSecurity = new EventWaitHandleSecurity();

            ewhSecurity.AddAccessRule(
             new EventWaitHandleAccessRule(
              "Everyone",
              EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify,
              AccessControlType.Allow));

            EventLog.Application.WriteInformation("Creating eventHandle: {0}", handleName);

            bool created;
            return new EventWaitHandle(false, EventResetMode.ManualReset, handleName, out created, ewhSecurity);
        }

        public EventWaitHandle GetEventHandle(EntityRef scheduledItemRef)
        {
            var handleName = GetName(scheduledItemRef);
            EventLog.Application.WriteInformation("Getting event: {0}", handleName);
            try
            {
                return EventWaitHandle.OpenExisting(handleName);
            }
            catch(Exception ex)
            {
                EventLog.Application.WriteTrace("Failed to get handle: {0}  Exception:{1}", handleName, ex);                
            }

            return null;
        }

        public override void Execute(EntityRef scheduledItemRef)
        {
            var waitHandle = GetEventHandle(scheduledItemRef);
            if (waitHandle != null)
            {
                waitHandle.Set();
                EventLog.Application.WriteInformation("Tripping ItemExecuted Event");
            }            
        }
    }
}