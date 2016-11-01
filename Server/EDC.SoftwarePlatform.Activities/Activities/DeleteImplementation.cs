// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// The implementation of the Delete activity usable from Workflows.
    /// </summary>
    public sealed class DeleteImplementation : ActivityImplementationBase, IRunNowActivity
    {
        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var resourceToDeleteKey = GetArgumentKey("deleteActivityResourceArgument");
            
            var res = (IEntity) inputs[resourceToDeleteKey];

            if (res == null)
            {
                throw new WorkflowRunException_Internal("No record provided.", null);
            }

            SecurityBypassContext.RunAsUser(() =>
            {
                Entity.Delete(res.Id);
            });

            context.RemoveReference(res.Id);
        }
    }
}
