// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities.EmailListener
{
    /// <summary>
    /// An action that can be performed on an email
    /// </summary>
    interface IEmailAction
    {
        /// <summary>
        /// Executed before the message is saved
        /// </summary>
        /// <param name="message"></param>
        /// <param name="postSaveAction">if not null an action run after the save. This happens even if the save is cancelled.</param>
        /// <returns>True if the save is to be cancelled</returns>
        bool BeforeSave(ReceivedEmailMessage message, out Action postSaveAction);
    }

    public class CompleteUserTaskEmailAction: IEmailAction
    {
        public bool BeforeSave(ReceivedEmailMessage message, out Action postSaveAction)
        {
            postSaveAction = null;

            string sequenceString;
            long tenantId;
            long sequenceId;

            try
            {
                UserTaskHelper.SequenceIdGenerator.SplitSequenceId(message.EmSubject, out sequenceString, out tenantId, out sequenceId);

            }
            catch (SequenceIdGenerator.InvalidSequenceId)
            {
              return false; // no sequence, so ignore the message
            }

            var task = UserTaskHelper.GetTaskFromEmbededSequenceId(sequenceString);

            if (task == null)
            {
                SendNoTaskReply(message);
            }
            else if (task.UserTaskCompletedOn != null)
            {
                SendTaskCompletedReply(message);
            }
            else
            {
                // try to complete the message
                var completionOptions = task.WorkflowRunForTask.PendingActivity.ForwardTransitions;
                    
                var completionState = ExtractCompletionOption(completionOptions, message.EmBody);

                if (completionState == null)
                {
                    SendInvalidStateReply(message);
                }
                else
                {
                    // update the task after the save
                    var taskId = task.Id;

                    postSaveAction = () =>
                        {
                            UserTaskHelper.ProcessApproval(task, completionState, (writableTask) =>
                            {
                                writableTask.RelatedMessages.Add(message.Cast<EmailMessage>());
                            });
                        };
                }
            }

            return false;
        }


        private void SendTaskCompletedReply(ReceivedEmailMessage message)
        {
            // TODO: add send
        }

        private void SendNoTaskReply(ReceivedEmailMessage message)
        {
            // TODO: add send
        }

        private void SendInvalidStateReply(ReceivedEmailMessage message)
        {
            // TODO: Add send
        }


        /// <summary>
        /// Given a reply message containing a completion state for a user task, find the matching state.
        /// </summary>
        /// <param name="completionOptions"></param>
        /// <param name="message"></param>
        /// <returns>The state, or null if the state was not found</returns>
        TransitionStart ExtractCompletionOption(IEnumerable<TransitionStart> completionOptions, string message)
        {
            var approvalAlternation =
                completionOptions.Select(o => Regex.Escape(o.Name))
                               .Aggregate((phrase, next) => phrase + '|' + next);
            // turn into "Approve|Reject|..."
            var approvalRegex = new Regex(string.Format("^\\s*({0})", approvalAlternation),
                                          RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var match = approvalRegex.Match(message);

            TransitionStart completionState = null;

            if (match.Success)
            {
                completionState =
                    completionOptions.First(
                        o => String.Equals(o.Name, match.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
            }

            return completionState;
        }

    }
}
