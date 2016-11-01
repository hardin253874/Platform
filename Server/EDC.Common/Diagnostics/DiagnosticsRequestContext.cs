// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Diagnostics;

namespace EDC.Diagnostics
{
    /// <summary>
    /// Provides helper methods for interacting with the application request data stored within the logical thread data.
    /// </summary>
    [Serializable]
    [DebuggerStepThrough]
    public class DiagnosticsRequestContext
    {
        [ThreadStatic]
        private static DiagnosticsRequestContextData _contextData;

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public static string UserName
        {
            get 
            {
                return _contextData != null ? _contextData.UserName : string.Empty;
            }
        }       

        /// <summary>
        /// Gets the tenant id.
        /// </summary>
        /// <value>
        /// The tenant id.
        /// </value>
        public static long TenantId
        {
            get 
            {
                return _contextData != null ? _contextData.TenantId : -1;                
            }
        }

        /// <summary>
        /// Gets the name of the tenant.
        /// </summary>
        /// <value>
        /// The name of the tenant.
        /// </value>
        public static string TenantName
        {
            get
            {
                return _contextData != null ? _contextData.TenantName : string.Empty;
            }
        }   

        /// <summary>
        ///     Frees the request data from the logical thread.
        /// </summary>
        public static void FreeContext()
        {
            // Free the request context data from the logical context
            _contextData = null;
        }

        /// <summary>
        ///     Gets the context data from the logical thread.
        /// </summary>
        /// <returns>
        ///     An object that represents the current request data.
        /// </returns>
        [DebuggerStepThrough]
        public static DiagnosticsRequestContextData GetContext()
        {
            return _contextData;
        }

        /// <summary>
        /// Sets the context data within the logical thread.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="tenantName">Name of the tenant.</param>
        /// <param name="userName">Name of the user.</param>
        public static void SetContext(long tenantId, string tenantName, string userName)
        {
            // Set the default request context data in the logical context
            _contextData = new DiagnosticsRequestContextData(tenantId, tenantName, userName);
        }
       
        /// <summary>
        ///     Sets the context data within the logical thread.
        /// </summary>
        /// <param name="contextData">The context data.</param>
        /// <remarks></remarks>
        public static void SetContext(DiagnosticsRequestContextData contextData)
        {
            _contextData = contextData;
        }
    }
}
