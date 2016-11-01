// Copyright 2011-2015 Global Software Innovation Pty Ltd

using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     Thread Data
	/// </summary>
	public class ThreadData : ViewModelBase
	{
		/// <summary>
		///     The _app domain
		/// </summary>
		private string _appDomain;

		/// <summary>
		///     The back ground
		/// </summary>
		private string _backGround;

		/// <summary>
		///     The CallStack
		/// </summary>
		private string _callStack;

		/// <summary>
		///     The CPU usage
		/// </summary>
		private float _cpuUsage;

		/// <summary>
		///     The Id
		/// </summary>
		private int _id;

		/// <summary>
		///     The OS thread identifier
		/// </summary>
		private int _osThreadId;

		/// <summary>
		///     The source application domain identifier
		/// </summary>
		private int _sourceAppDomainId;

		/// <summary>
		///     The	source process identifier
		/// </summary>
		private int _sourceProcessId;

		/// <summary>
		///     The _tooltip
		/// </summary>
		private string _tooltip;

		/// <summary>
		///     Initializes a new instance of the <see cref="ThreadData" /> class.
		/// </summary>
		public ThreadData( )
		{
			_backGround = "Transparent";
		}

		/// <summary>
		///     Gets or sets the application domain.
		/// </summary>
		/// <value>
		///     The application domain.
		/// </value>
		public string AppDomain
		{
			get
			{
				return _appDomain;
			}
			set
			{
				SetProperty( ref _appDomain, value );
			}
		}

		/// <summary>
		///     Gets the back ground.
		/// </summary>
		/// <value>
		///     The back ground.
		/// </value>
		public string BackGround
		{
			get
			{
				return _backGround;
			}
			set
			{
				SetProperty( ref _backGround, value );
			}
		}

		/// <summary>
		///     Gets or sets the call stack.
		/// </summary>
		/// <value>
		///     The call stack.
		/// </value>
		public string CallStack
		{
			get
			{
				return _callStack;
			}
			set
			{
				SetProperty( ref _callStack, value );

				if ( string.IsNullOrEmpty( value ) )
				{
					Tooltip = "No CallStack available.";
				}
				else
				{
					Tooltip = value;
				}
			}
		}

		/// <summary>
		///     Gets or sets the CPU usage.
		/// </summary>
		/// <value>
		///     The CPU usage.
		/// </value>
		public float CpuUsage
		{
			get
			{
				return _cpuUsage;
			}
			set
			{
				SetProperty( ref _cpuUsage, value );
			}
		}

		/// <summary>
		///     Gets or sets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		public int Id
		{
			get
			{
				return _id;
			}
			set
			{
				SetProperty( ref _id, value );
			}
		}

		/// <summary>
		///     Gets or sets the OS thread identifier.
		/// </summary>
		/// <value>
		///     The OS thread identifier.
		/// </value>
		public int OsThreadId
		{
			get
			{
				return _osThreadId;
			}
			set
			{
				SetProperty( ref _osThreadId, value );
			}
		}

		/// <summary>
		///     Gets or sets the source application domain identifier.
		/// </summary>
		/// <value>
		///     The source application domain identifier.
		/// </value>
		public int SourceAppDomainId
		{
			get
			{
				return _sourceAppDomainId;
			}
			set
			{
				SetProperty( ref _sourceAppDomainId, value );
			}
		}

		/// <summary>
		///     Gets or sets the source process identifier.
		/// </summary>
		/// <value>
		///     The source process identifier.
		/// </value>
		public int SourceProcessId
		{
			get
			{
				return _sourceProcessId;
			}
			set
			{
				SetProperty( ref _sourceProcessId, value );
			}
		}

		/// <summary>
		///     Gets or sets the tool-tip.
		/// </summary>
		/// <value>
		///     The tool-tip.
		/// </value>
		public string Tooltip
		{
			get
			{
				return _tooltip;
			}
			set
			{
				SetProperty( ref _tooltip, value );
			}
		}

		/// <summary>
		///     Creates a ThreadData from an existing ThreadInfo.
		/// </summary>
		/// <param name="threadInfo">The thread information.</param>
		/// <returns></returns>
		public static ThreadData FromThreadInfo( ThreadInfo threadInfo )
		{
			if ( threadInfo == null )
			{
				return null;
			}

			var data = new ThreadData
			{
				AppDomain = threadInfo.AppDomain,
				CallStack = threadInfo.CallStack,
				CpuUsage = threadInfo.CpuUsage,
				Id = threadInfo.Id,
				OsThreadId = threadInfo.OsThreadId
			};

			return data;
		}
	}
}