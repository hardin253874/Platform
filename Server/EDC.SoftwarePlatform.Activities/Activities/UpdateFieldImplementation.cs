// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;
using System.Data.SqlClient;


namespace EDC.SoftwarePlatform.Activities
{
    public sealed class UpdateFieldImplementation : ActivityImplementationBase, IRunNowActivity
    {
        private readonly UpdateArgsHelper _updateArgsHelper = new UpdateArgsHelper();

        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var res = (IEntity)inputs[GetArgumentKey("updateFieldActivityResourceArgument")];

            IEntity entityToUpdate = null;

            try
            {
                if (res != null)
                    entityToUpdate = res.AsWritable();
            }
            catch
            {
                // ignore
            }

            if (entityToUpdate == null)
            {
                throw new WorkflowRunException_Internal("Input resource argument must be a resource", null);
            }

            var activityAs = ActivityInstance.Cast<EntityWithArgsAndExits>();

            Action<IEntity> updateAction = (e) => UpdateArgsHelper.UpdateEntityFromArgs(activityAs, inputs, e);

            PerformUpdate(entityToUpdate, updateAction);
        }


        /// <summary>
        /// Perform a update using the provided update action. To be used for testing. 
        /// </summary>
        /// <param name="entityToUpdate"></param>
        /// <param name="updateAction"></param>
        public static void PerformUpdate(IEntity entityToUpdate, Action<IEntity> updateAction)
        {
            try
            {
                updateAction(entityToUpdate);

                SecurityBypassContext.RunAsUser(() =>
                {
                    entityToUpdate.Save();
                });
            }
            catch (EDC.ReadiNow.Model.ValidationException ex)
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
                    throw new UpdateInvalidReferenceException(ex);
                else
                    throw;
            }
        }


        public class UpdateInvalidReferenceException : WorkflowRunException
        {
            public UpdateInvalidReferenceException(SqlException ex) : base("Attempted to update an object with a reference to a object that does not exist.", ex)
            { }
        }
    }
}
