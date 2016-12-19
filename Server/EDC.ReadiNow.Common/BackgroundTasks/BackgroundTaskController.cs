// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Messaging;
using ProtoBuf;
using System;
using System.Text;
using System.Threading;

namespace EDC.ReadiNow.BackgroundTasks
{
    /// <summary>
    /// Controls the background tasks
    /// </summary>
    public class BackgroundTaskController : IBackgroundTaskController, IDisposable
    {
        enum MessageAction { StartAll, StopAll, Stopped, PollRunning, IsRunning, ReportRequest, ReportAnswer };

        const int MaxWaitTimeSec = 20;

	    readonly IBackgroundTaskManager _taskManager;
        int _runningCount;
        bool _stoppingInitiator;
        string _reportAnswer;    

        private IChannel<BackgroundTaskControllerMessage> Channel { get; set; }

        public BackgroundTaskController(IDistributedMemoryManager redisMgr, IBackgroundTaskManager backgroundTaskManager)
        {
            _taskManager = backgroundTaskManager;

            Channel = redisMgr.GetChannel<BackgroundTaskControllerMessage>("BackgroundTaskController");
            Channel.MessageReceived += ChannelMessageReceived;
            Channel.Subscribe();
        }

        /// <summary>
        /// Start all the active managers, resuming any suspended tasks
        /// </summary>
        public void StartAll()
        {
            Publish(MessageAction.StartAll);
        }

        /// <summary>
        /// Stop all the active managers, suspending all suspendable tasks and waiting for the rest to finish.
        /// </summary>
        /// <returns>True if all controllers reported as stopped</returns>
        public bool StopAll()
        {
            _stoppingInitiator = true;
            _runningCount = 0;

            Publish(MessageAction.PollRunning);
            Thread.Sleep(500);
            RecordStopping();

            try
            {
                Publish(MessageAction.StopAll);

                Thread.Sleep(500);

                int i = MaxWaitTimeSec;
                while (i-- > 0)
                {
                    if (_runningCount == 0)
                        break;

					Thread.Sleep( 5000 );
				}

                return i > 0;
            }
            finally
            {
                _runningCount = 0;
                _stoppingInitiator = false;
            }
        }

        /// <summary>
        /// Push all the tasks into the database
        /// </summary>
        public void SuspendToDb()
        {
            _taskManager.SuspendAllTasks();
        }

        /// <summary>
        /// Push all the tasks into the database
        /// </summary>
        public void RestoreFromDb()
        {
            _taskManager.RestoreAllTasks();
        }

        public string GetReport()
        {
            Publish(MessageAction.ReportRequest);
            Thread.Sleep(500);

            return _reportAnswer;
        }

        /// <summary>
        /// If the task manager is active, run the action
        /// </summary>
        private void IfActive(Action<IBackgroundTaskManager> act)
        {
            if (_taskManager.IsActive)
                act(_taskManager);
        }



        private void Publish(MessageAction action, bool publishToOriginator = true, string additionalInfo = null)
        {
            Channel.Publish(new BackgroundTaskControllerMessage() { Action = action.ToString(), AdditionalInfo = additionalInfo }, PublishMethod.Immediate, options: PublishOptions.None, publishToOriginator: publishToOriginator);
        }

        private void ChannelMessageReceived(object sender, MessageEventArgs<BackgroundTaskControllerMessage> e)
        {
            if (e != null)
           {
                var action = Enum.Parse(typeof(MessageAction), e.Message.Action);

                if (action != null)
                {
                    switch ((MessageAction) action)
                    {
                        case MessageAction.StartAll:
                            IfActive(tm => tm.Start());
                            break;

                        case MessageAction.StopAll:
                            IfActive(tm =>
                            {
                                tm.Stop();
                                Publish(MessageAction.Stopped);
                            });
                            break;

                        case MessageAction.Stopped:
                            if (_stoppingInitiator)
                            {
                                _runningCount--;
                                RecordStopping();
                            }
                            break;

                        case MessageAction.PollRunning:
                            IfActive(tm => Publish(MessageAction.IsRunning));
                            break;

                        case MessageAction.IsRunning:
                            if (_stoppingInitiator)
                            {
                                _runningCount++;
                            }
                            break;

                        case MessageAction.ReportRequest:
                            Publish(MessageAction.ReportAnswer, true, GenerateReport());
                            break;

                        case MessageAction.ReportAnswer:
                            _reportAnswer += e.Message.AdditionalInfo + '\n';
                            break;

                    }
                }
                else
                {
                     EventLog.Application.WriteError($"BackgroundTaskController: Unexpected message {e.Message.Action ?? "[null]"}");
                }

            }
        }

        void RecordStopping()
        {
            EventLog.Application.WriteInformation($"BackgroundTaskController: Stopping, waiting for {_runningCount} to stop.");
        }

        public string GenerateReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Server: {Environment.MachineName}, Process: {System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName}, Active: {_taskManager.IsActive}");
            if (_taskManager.IsActive)
                _taskManager.GenerateReport(sb);
            return sb.ToString();
        }

	    public void Dispose( )
	    {
			if ( Channel != null )
			{
				Channel.MessageReceived -= ChannelMessageReceived;
				Channel.Unsubscribe( );
				Channel.Dispose( );
				Channel = null;
			}
	    }
    }


    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class BackgroundTaskControllerMessage
    {
        public string Action;
        public string AdditionalInfo;
    }
}
