// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EDC.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities.Engine.Events;

namespace EDC.SoftwarePlatform.Activities.Tasks
{
    /// <summary>
    /// Updates mail boxes on the provider whenever the mail box is saved.
    /// </summary>
    public class UserTaskTarget : IEntityEventSave
    {
        static EntityRef isCompleteRef = new EntityRef("core", "userTaskIsComplete");
        static EntityRef taskStatusRef = new EntityRef("core", "taskStatus");

        static List<EntityRef> changesWeAreInterestedIn = new List<EntityRef>() { isCompleteRef, taskStatusRef, new EntityRef("core", "percentageCompleted"), new EntityRef("core", "userTaskCompletedOn") };

        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (var entity in entities)
            {
                var task = entity.As<BaseUserTask>();

                if (task.HasChanges(changesWeAreInterestedIn))
                {
                    var justCompleted = task.HasChanges(taskStatusRef.ToEnumerable()) && task.TaskStatus_Enum == TaskStatusEnum_Enumeration.TaskStatusCompleted;

                    if (justCompleted)
                    {
                        task.UserTaskIsComplete = true;
                        task.UserTaskCompletedOn = DateTime.UtcNow;
                        task.PercentageCompleted = 100;
                    }
                    else if (task.HasChanges(changesWeAreInterestedIn))
                    {
                        if (task.TaskStatus_Enum != TaskStatusEnum_Enumeration.TaskStatusCompleted)
                        {
                            if (task.UserTaskIsComplete ?? false)
                                task.UserTaskIsComplete = false;

                            if (task.UserTaskCompletedOn != null)
                                task.UserTaskCompletedOn = null;

                            // ignore percentage complete
                        }
                        else // we are already complete
                        {
                            if (!(task.UserTaskIsComplete ?? false))
                                task.UserTaskIsComplete = true;

                            if (task.PercentageCompleted != 100)
                                task.PercentageCompleted = 100;

                            // Ignore the completed date
                        }
                    }
                }
            }

            return false;
        }

        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
           
        }

      
    }
}