// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Messaging.Redis;
using EDC.ReadiNow.Model;
using ProtoBuf.Meta;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.BackgroundTasks.Handlers
{
    /// <summary>
    /// A base task handler that deals with the serialization and deserialization of the task information
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TaskHandler<T>: ITaskHandler where T: IWorkflowQueuedEvent, new()
    {
        /// <summary>
        /// The key used to fetch the task handler
        /// </summary>
        public string TaskHandlerKey { get; }

        public bool CompressData { get; }



        protected TaskHandler(string handlerKey, bool compressData)
        {
            TaskHandlerKey = handlerKey;
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void HandleTask(BackgroundTask task)
        {
            var value = task.GetData<T>();

            HandleTask(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        protected abstract void HandleTask(T taskData);


        /// <summary>
        /// Convert a T to a background task
        /// </summary>
        public BackgroundTask ToBackgroundTask(T taskData)
        {
            return BackgroundTask.Create(TaskHandlerKey, taskData);
        }


        /// <summary>
        /// The type of entity to hold this kind of suspended task.
        /// </summary>
        protected abstract EntityType SuspendedTaskType { get; }        
        

        public IEntity CreateSuspendedTask(BackgroundTask task)
        {
            var suspendedTask = Entity.Create(SuspendedTaskType);

            var context = task.Context;

            if (context.Identity != null && context.Identity.Id != 0)
            {
                var primaryUser = Entity.Get<UserAccount>(context.Identity.Id);

                if (primaryUser != null)
                    suspendedTask.GetRelationships(SuspendedTask.StIdentity_Field).Add(primaryUser);
            }

            if (context.SecondaryIdentity != null && context.SecondaryIdentity.Id != 0)
            {
                var secondaryUser = Entity.Get<UserAccount>(context.SecondaryIdentity.Id);

                if (secondaryUser != null)
                    suspendedTask.GetRelationships(SuspendedTask.StSecondaryIdentity_Field).Add(secondaryUser);
            }

            suspendedTask.SetField(SuspendedTask.StCulture_Field, task.Context.Culture);
            suspendedTask.SetField(SuspendedTask.StTimezone_Field, task.Context.TimeZone);

            T taskParam = task.GetData<T>();
            AnnotateSuspendedTask(suspendedTask, taskParam);

            return suspendedTask;
        }

        /// <summary>
        /// Annotate a suspend task with the task handler specific info.
        /// </summary>
        /// <param name="suspendTask">The task to annotate</param>
        /// <param name="backgroundTask">The background task to use</param>
        /// <returns></returns>~
        protected abstract void AnnotateSuspendedTask(IEntity suspendedTask, T backgroundTask);


        public IEnumerable<BackgroundTask> RestoreSuspendedTasks()
        {
            var suspended = Entity.GetInstancesOfType(SuspendedTaskType);

            var result = suspended.Select(te =>
            {
                try
                {
                    var taskData = RestoreTaskData(te);
                    var bgTask = ToBackgroundTask(taskData);

                    bgTask.Context.Identity = CreateIdentityInfo(te, SuspendedTask.StIdentity_Field);
                    bgTask.Context.SecondaryIdentity = CreateIdentityInfo(te, SuspendedTask.StSecondaryIdentity_Field);
                    bgTask.Context.Culture = (string)te.GetField(SuspendedTask.StCulture_Field);
                    bgTask.Context.TimeZone = (string)te.GetField(SuspendedTask.StTimezone_Field);

                    return bgTask;
                }
                catch (RestoreException ex)
                {
                    EventLog.Application.WriteInformation($"TaskHandler.RestoreSuspendedTasks: Unable to restore suspended task due to expected error. Task will be ignored: {ex}");
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError($"TaskHandler.RestoreSuspendedTasks: Unable to restore suspended task due to unexpected error: {ex}");
                }

                return null;
            }).ToList();

            Entity.Delete(suspended.Select(e => e.Id));

            return result.Where(r => r != null);
        }

        protected abstract T RestoreTaskData(IEntity suspendedTask);

        private Security.IdentityInfo CreateIdentityInfo(IEntity suspendedTask, IEntity field)
        {
            var identity = suspendedTask.GetRelationships<UserResource>(field).FirstOrDefault();
            return  identity != null ? new Security.IdentityInfo(identity.Id, identity.Name) : null;
        }
    }

    /// <summary>
    /// An expected restoration error 
    /// </summary>
    public class RestoreException: Exception
    {
        public RestoreException(string message) : base(message)
        { }
    }

    public class SuspendFailedException: Exception
    {
        public SuspendFailedException(string message): base(message)
        {

        }

    }
}
