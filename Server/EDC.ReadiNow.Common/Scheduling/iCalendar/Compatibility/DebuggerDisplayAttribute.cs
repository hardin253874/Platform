// Copyright 2011-2016 Global Software Innovation Pty Ltd
// ReSharper disable CheckNamespace
// ReSharper disable EmptyNamespace

namespace System.Diagnostics
// ReSharper restore EmptyNamespace
// ReSharper restore CheckNamespace
{
#if NETCF
    [AttributeUsageAttribute(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Delegate, AllowMultiple = true)]
    [ComVisibleAttribute(true)]
    public sealed class DebuggerDisplayAttribute : Attribute
    {
        public DebuggerDisplayAttribute(string value)
        {
        }
    }
#endif
}