// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Various extension methods.
    /// </summary>
    static class ExtensionMethods
    {
		/// <summary>
		/// Post a progress message back to the host.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="message">The message</param>
        internal static void UpdateProgress(this GeneratorSettings settings, string message)
        {
            if (settings.CurrentActivityCallback != null)
                settings.CurrentActivityCallback(message);
        }
    }
}
