// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.AddIn.Pipeline;
using ReadiMon.Contract;
using ReadiMon.Shared;

namespace ReadiMon.HostSideAdapter
{
	/// <summary>
	///     RedisSettings Contract To View Host Adapter
	/// </summary>
	public class RedisSettingsContractToViewHostAdapter : ContractBase, IRedisSettingsContract
	{
		/// <summary>
		///     The view
		/// </summary>
		private readonly IRedisSettings _view;

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisSettingsContractToViewHostAdapter" /> class.
		/// </summary>
		/// <param name="view">The view.</param>
		public RedisSettingsContractToViewHostAdapter( IRedisSettings view )
		{
			_view = view;
		}

		/// <summary>
		///     Gets the name of the server.
		/// </summary>
		/// <value>
		///     The name of the server.
		/// </value>
		public string ServerName
		{
			get
			{
				return _view.ServerName;
			}
		}

		/// <summary>
		///     Gets the port.
		/// </summary>
		/// <value>
		///     The port.
		/// </value>
		public int Port
		{
			get
			{
				return _view.Port;
			}
		}
	}
}