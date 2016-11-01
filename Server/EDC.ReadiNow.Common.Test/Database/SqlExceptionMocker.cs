// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Data.SqlClient;
using System.Reflection;

namespace EDC.ReadiNow.Test.Database
{
    /// <summary>
    /// Workaround completely test-unfriendly sql error classes.
    /// Copy-paste from http://stackoverflow.com/a/1387030/10245
    /// Adjusted with updates in comments
    /// </summary>
    class SqlExceptionMocker
    {
        private static T Construct<T>(int ctorIndex, params object[] p)
        {
            var ctors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var ctor = ctors[ctorIndex];
            return (T)ctor.Invoke(p);
        }

        public static SqlException MakeSqlException(int errorNumber)
        {

            var collection = Construct<SqlErrorCollection>(0);
            var error = Construct<SqlError>(1, errorNumber, (byte)2, (byte)3, "server name", "This is a Mock-SqlException", "proc", 100);

            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(collection, new object[] { error });


            var e = typeof(SqlException)
                .GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.ExplicitThis, new[] { typeof(SqlErrorCollection), typeof(string) }, new ParameterModifier[] { })
                .Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;

            return e;
        }
    }
}