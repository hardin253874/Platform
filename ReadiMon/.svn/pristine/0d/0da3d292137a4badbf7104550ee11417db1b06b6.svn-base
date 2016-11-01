// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Runtime.InteropServices;

namespace ReadiMon.Plugin.Misc
{
	/// <summary>
	///     Win32 Interoperability.
	/// </summary>
	internal static class Win32
	{
		/// <summary>
		///     A clipboard viewer window receives the WM_CHANGECBCHAIN message when
		///     another window is removing itself from the clipboard viewer chain.
		/// </summary>
		internal const int WM_CHANGECBCHAIN = 0x030D;

		/// <summary>
		///     The WM_DRAWCLIPBOARD message notifies a clipboard viewer window that
		///     the content of the clipboard has changed.
		/// </summary>
		internal const int WM_DRAWCLIPBOARD = 0x0308;

		/// <summary>
		///     Changes the clipboard chain.
		/// </summary>
		/// <param name="hWndRemove">The h WND remove.</param>
		/// <param name="hWndNewNext">The h WND new next.</param>
		/// <returns></returns>
		[DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
		internal static extern bool ChangeClipboardChain( IntPtr hWndRemove, IntPtr hWndNewNext );

		/// <summary>
		///     Sends the message.
		/// </summary>
		/// <param name="hWnd">The h WND.</param>
		/// <param name="Msg">The MSG.</param>
		/// <param name="wParam">The w parameter.</param>
		/// <param name="lParam">The l parameter.</param>
		/// <returns></returns>
		[DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
		internal static extern IntPtr SendMessage( IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam );

		/// <summary>
		///     Sets the clipboard viewer.
		/// </summary>
		/// <param name="hWndNewViewer">The h WND new viewer.</param>
		/// <returns></returns>
		[DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
		internal static extern IntPtr SetClipboardViewer( IntPtr hWndNewViewer );
	}
}