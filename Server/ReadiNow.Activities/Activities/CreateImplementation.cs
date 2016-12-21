// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Activities;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;
using System.Data.SqlClient;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Core;
using Autofac;

namespace EDC.SoftwarePlatform.Activities
{

    public sealed class CreateImplementation : ActivityImplementationBase, IRunNowActivity
    {
        private static void Log(string message, params object[] parameters)
        {
           EventLog.Application.WriteInformation(message, parameters);
        }

        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var ResourceTypeToCreateKey =   GetArgumentKey("createActivityResourceArgument");
            var CreatedResourceKey =        GetArgumentKey("createActivityCreatedResource");

            IEntity newEntity = null;

            var resTypeRef = (IEntity)inputs[ResourceTypeToCreateKey];
            var resType = resTypeRef?.As<EntityType>();
            if (resType == null)
            {
                throw new WorkflowRunException_Internal("Input resource argument must be a type.", null);
            }

            var activityAs = ActivityInstance.Cast<EntityWithArgsAndExits>();
            Action<IEntity> updateAction = (e) => UpdateArgsHelper.UpdateEntityFromArgs(activityAs, inputs, e);


            DatabaseContext.RunWithRetry(() =>
            {
                newEntity = PerformCreate(resType, updateAction);
            });

            context.SetArgValue(ActivityInstance, CreatedResourceKey, newEntity);
        }

        /// <summary>
        /// Perform a create using the provided update action. To be used for testing. 
        /// </summary>
        /// <param name="resType"></param>
        /// <param name="updateAction"></param>
        public static IEntity PerformCreate(EntityType resType, Action<IEntity> updateAction)
        {
            try
            {
                IEntity newEntity = null;

                SecurityBypassContext.RunAsUser(() =>
                {
                    newEntity = Entity.Create(resType);

                    var provider = Factory.Current.Resolve<IEntityDefaultsDecoratorProvider>();
                    var decorator = provider.GetDefaultsDecorator(resType.Id);
                    // Do per-entity work
                    decorator.SetDefaultValues(newEntity.ToEnumerable());

                });

                 updateAction(newEntity);


                SecurityBypassContext.RunAsUser(() =>
                {
                    newEntity.Save();
                });

                return newEntity;
            }
            catch (ValidationException ex)
            {
                string message = ex.FieldName != null ? string.Format("{0}: {1}", ex.FieldName, ex.Message) : ex.Message;
                throw new WorkflowRunException(message, ex);
            }
            catch (CardinalityViolationException ex)
            {
                throw new WorkflowRunException(ex.Message, ex);
            }
            catch (DuplicateKeyException ex)
            {
                throw new WorkflowRunException(ex.Message, ex);
            }
            catch (InvalidCastException ex)
            {
                throw new WorkflowRunException("Incorrect data type", ex);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                    throw new CreateInvalidReferenceException(ex);
                else
                    throw;
            }
        }

        public class CreateInvalidReferenceException: WorkflowRunException
        {
            public CreateInvalidReferenceException(SqlException ex) : base("Attempted to create an object with a reference to a object that does not exist.", ex)
            { }
        }
    }

}

   

