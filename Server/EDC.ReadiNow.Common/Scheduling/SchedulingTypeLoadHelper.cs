// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Reflection;
using Quartz.Simpl;

namespace EDC.ReadiNow.Scheduling
{
	/// <summary>
	///     The scheduling type load helper class.
	/// </summary>
	/// <seealso cref="Quartz.Simpl.SimpleTypeLoadHelper" />
	/// <remarks>
	///     Note* This is to be left empty at the moment. Please do not delete.
	/// </remarks>
	public class SchedulingTypeLoadHelper : SimpleTypeLoadHelper
	{
		/// <summary>
		///     Called to give the ClassLoadHelper a chance to Initialize itself,
		///     including the opportunity to "steal" the class loader off of the calling
		///     thread, which is the thread that is initializing Quartz.
		/// </summary>
		public override void Initialize( )
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		}

		/// <summary>
		///     Handles the AssemblyResolve event of the CurrentDomain control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">The <see cref="ResolveEventArgs" /> instance containing the event data.</param>
		/// <returns></returns>
		private Assembly CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
		{
			return null;
		}
	}
}