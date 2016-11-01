// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Common.Logging;
using EDC.ReadiNow.Scheduling;
using Quartz;
using Quartz.Impl;
using EDC.ReadiNow.Core;

namespace SchedulerService
{
    public partial class SchedulerService : ServiceBase
    {
        private readonly ILog logger;
        private IScheduler scheduler;
        private string _schedulerInstanceId = null;

        public SchedulerService(string schedulerInstanceId)
        {
            InitializeComponent();

            logger = new EdcLogger();

            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;
            _schedulerInstanceId = schedulerInstanceId;

            logger.Info("Scheduler service: Created");
        }

        internal void InternalStart() { OnStart(null); }

        internal void InternalStop() { OnStop(); }


        protected override void OnStart(string[] args)
        {
            try
            {
                logger.Info("Scheduler service: Starting");

                Factory.BackgroundTaskManager.Start();

                scheduler = SchedulingHelper.Instance;

                scheduler.Start();


                try
                {
                    Thread.Sleep(3000);
                }
                catch (ThreadInterruptedException) 
                {
                }

                logger.Info("Scheduler service: Starting completed");
            }
            catch (Exception e)
            {
                logger.Error("Server initialization failed:" + e.Message, e);
                throw;
            }
        }

        protected override void OnStop()
        {
            logger.Info("Scheduler service: Stopping");

            scheduler.Shutdown(true);

            Factory.BackgroundTaskManager.Stop();
            logger.Info("Scheduler service: Stopping complete");
        }


        protected override void OnPause()
        {
            scheduler.PauseAll();
            Factory.BackgroundTaskManager.Stop();
        }

        protected override void OnContinue()
        {
            Factory.BackgroundTaskManager.Start();
            scheduler.ResumeAll();
        }

    }
}
