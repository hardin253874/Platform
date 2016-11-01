// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SchedulerService
{
    static class Program
    {
        private const int ERROR_BAD_ARGUMENTS = 160;

        private static EventWaitHandle _waitHandle;
        private static SchedulerService _service;
        static void Main(string[] args)
        {
            bool runConsole = false;
            string instanceId = null;
            var errorMessages = new List<string>();

            foreach (string arg in args)
            {
                if (arg.ToLowerInvariant().Equals("-console"))
                {
                    runConsole = true;
                }
                else if (arg.StartsWith("-instanceId:"))
                {
                    var split = arg.Split(':');
                    if (split.Count() == 2)
                    {
                        instanceId = split[1];
                    }
                    else
                    {
                        errorMessages.Add("Missing instanceId or instanceId contains a ':'.");
                    }
                }
                else
                {
                    if (arg.ToLowerInvariant().Equals("-help"))
                        errorMessages.Add(string.Empty);
                }
            }

			if ( errorMessages.Count <= 0 )
            {
                _service = new SchedulerService(instanceId ?? System.Environment.MachineName + "_SchedulerService");
                if (runConsole)
                {
                    _waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                    Console.WriteLine("Starting Workflow Service in Console Mode");
                    Console.WriteLine("Press Ctrl+C to exit Console Mode");
                    Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress);
                    _service.InternalStart();
                    WaitHandle.WaitAll(new WaitHandle[] {_waitHandle});
                }

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                    {
                       _service
                    };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                Console.WriteLine("Error encountered while parsing arguments:");
                foreach (var msg in errorMessages)
                    Console.WriteLine(string.Format("\t{0}", msg));

                Console.WriteLine("\nUsage:  SchedulerService [-console] [-instanceId:myinstancename]");
                Console.WriteLine("\t-console \tRun the service interactively for debugging.");
                Console.WriteLine("\t-instanceId:myinstancename \tSet the instance name for the scheduler. This must be unique for every instance in the scheduling cluster. If not specified, the instanceId defaults to the name of the machine.");

                Environment.Exit(ERROR_BAD_ARGUMENTS);
            }
        }

        static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _service.InternalStop();
            _waitHandle.Set();
        }


    }
}
