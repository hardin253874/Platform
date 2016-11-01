// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDC.ReadiNow.Model.CacheInvalidation;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    ///     Security bypass context
    /// </summary>
    [Serializable]
    public sealed class SecurityBypassContext : IDisposable
    {
        /// <summary>
        ///     Indicates whether this object has been disposed or not.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// True to bypass security, false otherwise.
        /// </summary>
        public bool BypassSecurity { get; private set; }

        /// <summary>
        /// Parent bypass context
        /// </summary>
        public SecurityBypassContext _parent = null;

        /// <summary>
        /// Current bypass context
        /// </summary>
        [ThreadStatic]
        public static SecurityBypassContext _current = null;

        /// <summary>
        /// Is the security bypass context active
        /// </summary>
        [ThreadStatic]
        public static bool _isActive = false;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SecurityBypassContext" /> class.
        /// </summary>
        public SecurityBypassContext():
            this(true)
        {
            // Do nothing
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityBypassContext" /> class.
        /// </summary>
        /// <param name="bypassSecurity">if set to <c>true</c> security is bypassed otherwise security is enabled.</param>
        public SecurityBypassContext(bool bypassSecurity)
        {
            BypassSecurity = bypassSecurity;

            _parent = _current;
            _current = this;
            _isActive = bypassSecurity;
        }

        /// <summary>
        /// Run the given <see cref="Action"/> with access control checks bypassed.
        /// </summary>
        /// <param name="action">
        /// The <see cref="Action"/> to run. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> cannot be null.
        /// </exception>
        public static void Elevate( Action action )
        {
            if ( action == null )
            {
                throw new ArgumentNullException( "action" );
            }

            using ( new SecurityBypassContext( ) )
            {
                action( );
            }
        }

        /// <summary>
        /// Evaluates the given callback with access control checks bypassed.
        /// </summary>
        /// <param name="func">
        /// The callback to run. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func"/> cannot be null.
        /// </exception>
        public static T Elevate<T>( Func<T> func )
        {
            if ( func == null )
            {
                throw new ArgumentNullException( "func" );
            }

            using ( new SecurityBypassContext( ) )
            {
                return func( );
            }
        }

        /// <summary>
        /// Run the given <see cref="Action"/> with access control checks bypassed.
        /// </summary>
        /// <param name="elevateOnlyIfTrue">Optional. Only perform the elevation if this is true.</param>
        /// <param name="action">
        /// The <see cref="Action"/> to run. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> cannot be null.
        /// </exception>
        public static void ElevateIf( bool elevateOnlyIfTrue, Action action )
        {
            if ( elevateOnlyIfTrue )
                Elevate( action );
            else
                action( );
        }

        /// <summary>
        /// Evaluates the given callback with access control checks bypassed.
        /// </summary>
        /// <param name="elevateOnlyIfTrue">Optional. Only perform the elevation if this is true.</param>
        /// <param name="func">
        /// The callback to run. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func"/> cannot be null.
        /// </exception>
        public static T ElevateIf<T>( bool elevateOnlyIfTrue, Func<T> func )
        {
            if ( elevateOnlyIfTrue )
                return Elevate( func );
            return func( );
        }

        /// <summary>
        /// Run the given <see cref="Action"/> with all bypass contexts temporarily removed.
        /// </summary>
        /// <param name="action">
        /// The <see cref="Action"/> to run. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> cannot be null.
        /// </exception>
        public static void RunAsUser(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            using(new SecurityBypassContext(false))
            {
                action();
            }
        }        

        /// <summary>
        ///     Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public static bool IsActive
        {
            get { return _isActive; }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            /////
            // No need to call the finalizer.
            /////
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        // ReSharper disable UnusedParameter.Local
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _current = _parent;

                if ( _current == null )
                {
                    _isActive = false;
                }
                else
                {
                    _isActive = _current.BypassSecurity;
                }

                /////
                // Dispose complete.
                /////
                _disposed = true;
            }
        }
        // ReSharper restore UnusedParameter.Local
    }
}
