// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.Diagnostics;
using System.Windows.Media;
using System.Windows;
using LogViewer.Common;

namespace LogViewer.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    internal class EventLogEntryInfo : ObservableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogEntryInfo"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public EventLogEntryInfo(MainWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
            IsAccepted = true;
        }       


        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether this instance is accepted.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is accepted; otherwise, <c>false</c>.
        /// </value>
        public bool IsAccepted { get; set; }


        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public Guid Id { get; set; }


        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public DateTime Date { get; set; }


        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }


        /// <summary>
        /// Gets the message to be displayed in the grid.
        /// Display the first 100 characters.
        /// </summary>
        /// <value>
        /// The message to be displayed in the grid.
        /// </value>
        public string GridMessage
        {
            get
            {
                if (string.IsNullOrEmpty(Message) || Message.Length <= Constants.MaxGridMessageLength)
                {
                    return Message;
                }

                var stringBuilder = new StringBuilder(Message, 0, Constants.MaxGridMessageLength, Constants.MaxGridMessageLength + 10);
                stringBuilder.Append("...");
                return stringBuilder.ToString();
            }            
        }


        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }


        /// <summary>
        /// Gets or sets the process.
        /// </summary>
        /// <value>
        /// The process.
        /// </value>
        public string Process { get; set; }


        /// <summary>
        /// Gets or sets the thread id.
        /// </summary>
        /// <value>
        /// The thread id.
        /// </value>
        public int ThreadId { get; set; }


        /// <summary>
        /// Gets or sets the machine.
        /// </summary>
        /// <value>
        /// The machine.
        /// </value>
        public string Machine { get; set; }


        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        public long Timestamp { get; set; }


        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public EventLogLevel Level 
        {
            get
            {
                return level;
            }
            set
            {
                level = value;

                switch (level)
                {
                    case EventLogLevel.Error:
                        foregroundcolor = new SolidColorBrush(Colors.Red);
                        break;
                    case EventLogLevel.Warning:
                        foregroundcolor = new SolidColorBrush(Colors.Blue);
                        break;
                    case EventLogLevel.Information:
                        foregroundcolor = new SolidColorBrush(Colors.Black);
                        break;
                    case EventLogLevel.Trace:
                        foregroundcolor = new SolidColorBrush(Colors.Gray);
                        break;
                    default:
                        foregroundcolor = new SolidColorBrush(Colors.Black);
                        break;
                }
            }
        }
        private EventLogLevel level;


        /// <summary>
        /// Gets the color of the background.
        /// </summary>
        /// <value>
        /// The color of the background.
        /// </value>
        public Brush BackgroundColor
        {
            get
            {                
                return viewModel.CalculateBackgroundColor(this);
            }
        }


        /// <summary>
        /// Gets the color of the foreground.
        /// </summary>
        /// <value>
        /// The color of the foreground.
        /// </value>
        public Brush ForegroundColor
        {
            get
            {
                return foregroundcolor;                
            }
        }
        private Brush foregroundcolor;


        /// <summary>
        /// 
        /// </summary>
        private MainWindowViewModel viewModel;


        /// <summary>
        /// Gets or sets the tenant id.
        /// </summary>
        /// <value>
        /// The tenant id.
        /// </value>
        public long TenantId { get; set; }


        /// <summary>
        /// Gets or sets the name of the tenant.
        /// </summary>
        /// <value>
        /// The name of the tenant.
        /// </value>
        public string TenantName { get; set; }


        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }


        /// <summary>
        /// Gets or sets the log file path.
        /// </summary>
        /// <value>
        /// The log file path.
        /// </value>
        public string LogFilePath { get; set; }
        #endregion        
    }
}