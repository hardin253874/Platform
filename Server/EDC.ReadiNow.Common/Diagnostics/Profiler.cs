// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Diagnostics
{
    /// <summary>
    /// A very simple wrapper over stopwatch.
    /// Feel free to make it more useful.
    /// </summary>
    public class Profiler : IDisposable
    {        
        /// <summary>
        /// Depth of nesting.
        /// </summary>
        [ThreadStatic]
        static int depth = 0;

        /// <summary>
        /// The root profiling block.
        /// </summary>
        [ThreadStatic]
        static Profiler root = null;

        /// <summary>
        /// The current profiling block.
        /// </summary>
        [ThreadStatic]
        static Profiler current = null;

        /// <summary>
        /// The current profiling block.
        /// </summary>
        [ThreadStatic]
        static bool suppressProfiling = false;

        /// <summary>
        /// Message for this block.
        /// </summary>
        string _message;

        /// <summary>
        /// Timer for this block.
        /// </summary>
        Stopwatch _stopwatch;

        /// <summary>
        /// Cumulative time spent on nested profilers within the current block.
        /// </summary>
        double _innerDelay = 0;

        /// <summary>
        /// The parent to this profiler block.
        /// </summary>
        Profiler _parent;

        /// <summary>
        /// Cumulative time spent on nested profilers within the current block.
        /// </summary>
        double _startTime = 0;

        /// <summary>
        /// String builder for root.
        /// </summary>
        StringWriter _writer = null;

        bool _hasChildren = false;

        /// <summary>
        /// Start a block of code to be timed. Use within a 'using' block.
        /// </summary>
        /// <returns>An IDisposable, which should be disposed when completed.</returns>
        public static IDisposable Measure(string message)
        {
            if (suppressProfiling)
                return null;

            return new Profiler(message, false);
        }

        /// <summary>
        /// Start a block of code to be timed. Use within a 'using' block.
        /// </summary>
        /// <returns>An IDisposable, which should be disposed when completed.</returns>
        public static IDisposable Measure(string message, params object[] args)
        {
            if (suppressProfiling)
                return null;

            return new Profiler(string.Format(message, args), false);
        }

        /// <summary>
        /// Start a block of code to be timed. Use within a 'using' block.
        /// </summary>
        /// <returns>An IDisposable, which should be disposed when completed.</returns>
        public static IDisposable MeasureAndSuppress(string message, params object[] args)
        {
            if (suppressProfiling)
                return null;

            return new Profiler(string.Format(message, args), true);
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        private Profiler(string message, bool suppressInner)
        {
            // Reset root
            if (root == null)
            {
                _writer = new StringWriter();
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                root = this;
            }
            else
            {
                _stopwatch = root._stopwatch;
                _writer = root._writer;
            }

            // Captures state
            _startTime = _stopwatch.Elapsed.TotalMilliseconds;
            _message = message;
            _parent = current;
            current = this;
            suppressProfiling = suppressInner || (!ProcessMonitorWriter.Instance.IsEnabled && !EventLog.Application.TraceEnabled);

            if (_parent != null && _parent.AddChild())
                _writer.Write(", ");
            _writer.WriteLine("{");
            depth++;
            WriteLine("\"label\": \"" + _message.Replace("\"", "\"\"") + "\",");            
        }

        /// <summary>
        /// Log the results.
        /// </summary>
        void IDisposable.Dispose()
        {
            suppressProfiling = false;
            current = _parent;
            double stopTime = _stopwatch.Elapsed.TotalMilliseconds;
            double delay = stopTime - _startTime;
            double ownDelay = delay - _innerDelay;

            if (_parent != null)
            {
                _parent._innerDelay += delay;
            }

            // Write json
            if (_hasChildren)
            {
                depth--;
                _writer.WriteLine("],");
            }
            WriteLine("\"start\": {0:F2}, \"stop\": {1:F2}, \"totalMs\": {2:F2}, \"ownMs\": {3:F2}", _startTime, stopTime, delay, ownDelay);
            depth--;
            Write("}");

            // Write result to log
            if (this == root)
            {
                string prefix = string.Format("Profile: {0} ({1}ms)\n", _message, delay);

                _stopwatch.Stop();
                root = null;
                _writer.Flush();

                string msg = prefix + _writer.ToString();

                if (EventLog.Application.TraceEnabled)
                {
                    EventLog.Application.WriteTrace(msg);
                }
                
                if (ProcessMonitorWriter.Instance.IsEnabled)
                {
                    ProcessMonitorWriter.Instance.Write(msg);
                }                
            }
        }

        /// <summary>
        /// Called on a parent to indicate it has a child. 
        /// </summary>
        /// <returns>Returns true if there are previous children.</returns>
        bool AddChild()
        {
            var res = _hasChildren;
            if (!_hasChildren)
            {
                _hasChildren = true;
                Write("\"children\": [");
                depth++;
            }
            return res;
        }

        void Write(string text)
        {
            _writer.Write(new string(' ', depth * 4));
            _writer.Write(text);
        }

        void WriteLine(string text)
        {
            Write(text);
            _writer.WriteLine();
        }

        void WriteLine(string text, params object[] data)
        {
            string formatted = string.Format(text, data);
            WriteLine(formatted);
        }
    }
}
