// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace TenantDiffTool.Core
{
	/// <summary>
	///     Gets the source.
	/// </summary>
	public interface ISourceProvider
	{
		/// <summary>
		///     Gets the source.
		/// </summary>
		/// <returns></returns>
		ISource GetSource( );
	}
}