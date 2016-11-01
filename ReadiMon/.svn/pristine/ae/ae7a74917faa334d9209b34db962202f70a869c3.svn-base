// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.AddIn.Pipeline;
using ReadiMon.Contract;
using ReadiMon.Shared;

namespace ReadiMon.AddinSideAdapter
{
	/// <summary>
	///     RedisSettings Contract To View Addin Adapter
	/// </summary>
	[AddInAdapter]
	public class RedisSettingsContractToViewAddinAdapter : ContractBase, IRedisSettings
	{
		/// <summary>
		///     The plugin view.
		/// </summary>
		private readonly IRedisSettingsContract _view;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseSettingsContractToViewAddinAdapter" /> class.
		/// </summary>
		/// <param name="view">The view.</param>
		public RedisSettingsContractToViewAddinAdapter( IRedisSettingsContract view )
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