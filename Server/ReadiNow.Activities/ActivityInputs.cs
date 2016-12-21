// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{
    public class ActivityInputs: Dictionary<ActivityArgument, object>
    {
        public ActivityInputs() : base(EntityIdComparer.Singleton)
        {
            
        }


        public ActivityInputs(IEnumerable<ActivityArgument> args, IDictionary<string, object> inputs )
            : base(EntityIdComparer.Singleton)
        {
            foreach (var arg in args)
            {
                object o = null;
                inputs.TryGetValue(arg.Name, out o);

                if (IsValidArg(arg, o))
                {
                    Add(arg, o);
                }
            }
        }
        
        private bool IsValidArg(ActivityArgument arg, object input)
        {
            // validate resource argument type
            var isResourceArg = arg.IsOfType.Select(t => t.Alias == "core:resourceArgument").Any();
            if (!isResourceArg || input == null) return true;
            {
                var resourceArg = arg.As<ResourceArgument>();
                if (resourceArg?.ConformsToType == null) return true;

                var argConformsToType = arg.As<ResourceArgument>().ConformsToType;

                if (input.GetType() != typeof (Entity)) return true;

                var argumentEntity = input as Entity;

                if (argumentEntity == null || argConformsToType == null || argConformsToType.Alias == "core:enumType" ||
                    argConformsToType.Alias == "core:enumValue") return true;

                bool isValid = ((IEntity)argumentEntity).GetAllTypes().Any(t => t.Id == argConformsToType.Id);

                if (!isValid)
                {
                    throw new WorkflowRunException(
                        $"The selected argument resource is not of the required type for this workflow. Expected type '{argConformsToType.Name}', but received '{Entity.GetName(argumentEntity.Id)}' of type '{Entity.GetName(argumentEntity.TypeIds.First())}.'");
                }
            }

            // more validations to be added later on
            return true;
        }
    }
}
