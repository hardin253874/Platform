// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Core.Cache;
using System.Diagnostics;
using System.Threading;
using EDC.ReadiNow.Configuration;

namespace EDC.ReadiNow.Test
{
	/// <summary>
	///     Test helper methods.
	/// </summary>
	public static class TestHelpers
	{
		/// <summary>
		///     Do a ECF serialization of the object to check there are no contract problems.
		/// </summary>
		/// <param name="o">The o.</param>
		/// <returns></returns>
		public static void CheckWcfSerialization( object o )
		{
			/////
			// HACK: Since this method above actually changes any internal instances of ObservableCollection<T> to List<T>,
			// the de-serialized instance (List<T> converted back to ObservableCollection<T>) is returned so the original format
			// can be used.
			/////
			var writer = new MemoryStream( );
			var ser =
				new DataContractSerializer( o.GetType( ) );
			ser.WriteObject( writer, o );
			writer.Seek( 0, SeekOrigin.Begin );
			ser.ReadObject( writer );
			writer.Close( );
		}


        /// <summary>
        ///     Returns the ReadiNow product installation date.
        /// </summary>
        /// <returns></returns>
        public static DateTime GetReadiNowProductInstallationDate()
        {
	        DateTime dt;

			if ( !DateTime.TryParse( ConfigurationSettings.GetServerConfigurationSection( ).SystemInfo.ActivationDate, out dt ) )
			{
				dt = DateTime.MinValue;
			}

			return dt;
        }

        /// <summary>
        /// Clear caches
        /// </summary>
        public static void ClearServerCaches()
        {
            CacheManager.ClearCaches();
        }

        /// <summary>
        /// Solutions that are to be checked for correctness.
        /// </summary>
        public static string[] ValidatableSolutions = new[] { "ReadiNow Core", "ReadiNow Core Data", "ReadiNow Console", "Shared" };

        /// <summary>
        /// Checks if the resource is in a solution that is to be checked for correctness.
        /// </summary>
        public static bool InValidatableSolution(Solution solution)
        {
			if ( solution != null )
			{
				return ValidatableSolutions.Contains( solution.Name );
			}

	        return false;
        }

        /// <summary>
        /// Checks if the resource is in a solution that is to be checked for correctness.
        /// </summary>
        public static void TestConcurrent( int threads, Action action, bool timers = false, int repeats = 1, int paceMs = 0)
        {
            TestConcurrent( threads, ( threadNumber ) => { action( ); }, timers, repeats );
        }

        public static void TestConcurrent( int threads, Action<int> action, bool timers = false, int repeats = 1, int paceMs=0 )
        {
            Random rand = new Random(0);

            long sum = 0;
            object obj = new object( );
            Stopwatch master = new Stopwatch( );
            master.Start( );
            ManualResetEvent evt = new ManualResetEvent( false );
            int completed = 0;

            // Timer logic
            if ( timers )
            {
                Action<int> innerAction = action;
                action = ( int threadNumber ) =>
                {
                    try
                    {
                        long masterStart = master.ElapsedMilliseconds;
                        Stopwatch stopwatch = new Stopwatch( );
                        stopwatch.Start( );
                        innerAction( threadNumber );
                        stopwatch.Stop( );
                        long elapsed = stopwatch.ElapsedMilliseconds;
                        lock ( obj )
                        {
                            Console.WriteLine( "Started: {0} \tElapsed: {1}", masterStart, elapsed );
                            sum += elapsed;
                        }

                    }
                    catch ( Exception ex )
                    {
                        Console.WriteLine( ex.Message );
                        throw;
                    }
                };
            }

            // Repeat/sync logic
            Action<object> threadEntry = ( object data ) =>
            {

                int threadNumber = ( int ) data;

                try
                {
                    for ( int i = 0; i < repeats; i++ )
                    {
                        if (i == 0 && paceMs > 0)
                        {
                            var sleep = rand.Next(paceMs);
                            Thread.Sleep(sleep);
                        }
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        action( threadNumber );

                        var elapsed = stopwatch.ElapsedMilliseconds;

                        if (i < repeats - 1 && elapsed < paceMs)
                        {
                            var sleep = paceMs - (int)elapsed;
                            Thread.Sleep(sleep);
                        }

                    }
                }
                finally
                {
                    lock ( obj )
                    {
                        completed++;
                        if ( completed == threads )
                            evt.Set( );
                    }
                }
            };

            //Note: Task library throttles number of threads :(
            //Task.WaitAll(
            //    Enumerable.Range( 0, threads )
            //    .Select( x => Task.Factory.StartNew( action ) )
            //    .ToArray( ) );

            ParameterizedThreadStart ts = new ParameterizedThreadStart( threadEntry );
            for ( int i = 0; i < threads; i++ )
            {
                Thread thread = new Thread( ts );
                thread.Start( i );
            }
            evt.WaitOne( );

            if ( timers )
            {
                Console.WriteLine( "Average: {0}", sum / ( threads * repeats ) );
                Console.WriteLine( "Total {0}", master.ElapsedMilliseconds );
            }
                    
        }
        
	}
}