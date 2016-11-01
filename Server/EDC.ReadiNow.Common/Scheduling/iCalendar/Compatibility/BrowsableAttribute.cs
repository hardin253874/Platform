// Copyright 2011-2016 Global Software Innovation Pty Ltd
// ReSharper disable CheckNamespace
// ReSharper disable EmptyNamespace

namespace System.ComponentModel
// ReSharper restore EmptyNamespace
// ReSharper restore CheckNamespace
{
#if NETCF
    [AttributeUsageAttribute(AttributeTargets.All)]
    public sealed class BrowsableAttribute : Attribute
    {
        public BrowsableAttribute(bool value)
        {
        }
    }
#endif
}