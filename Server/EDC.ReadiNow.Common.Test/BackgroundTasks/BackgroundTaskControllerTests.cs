// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Threading;
using Autofac;
using EDC.ReadiNow.BackgroundTasks;
using EDC.ReadiNow.Core;
using Moq;
using NUnit.Framework;

// ReSharper disable AccessToDisposedClosure
namespace EDC.ReadiNow.Test.BackgroundTasks
{
	[TestFixture]
	public class BackgroundTaskControllerTests
	{
		[Test]
		public void Registration( )
		{
			var controller = Factory.Current.Resolve<IBackgroundTaskController>( );

			Assert.That( controller, Is.Not.Null );
		}

		[Test]
		public void StartAll( )
		{
			var redisMgr = Factory.DistributedMemoryManager;

			using ( CountdownEvent mock1Counter = new CountdownEvent( 2 ) )
			using ( CountdownEvent mock2Counter = new CountdownEvent( 1 ) )
			{
				// active TM
				var mockTm1 = new Mock<IBackgroundTaskManager>( MockBehavior.Strict );
				mockTm1.Name = nameof( mockTm1 );
				mockTm1.Setup( tm => tm.IsActive ).Returns( true ).Callback( ( ) => mock1Counter.Signal( ) );
				mockTm1.Setup( tm => tm.Start( ) ).Callback( ( ) => mock1Counter.Signal( ) );
				mockTm1.Setup( tm => tm.IsActive ).Returns( true ).Callback( ( ) => mock1Counter.Signal( ) );
				mockTm1.Setup( tm => tm.Start( ) ).Callback( ( ) => mock1Counter.Signal( ) );

				using ( var controller1 = new BackgroundTaskController( redisMgr, mockTm1.Object ) )
				{
					// passive TM
					var mockTm2 = new Mock<IBackgroundTaskManager>( MockBehavior.Strict );
					mockTm1.Name = nameof( mockTm2 );
					mockTm2.Setup( tm => tm.IsActive ).Returns( false ).Callback( ( ) => mock2Counter.Signal( ) );
					mockTm2.Setup( tm => tm.IsActive ).Returns( false ).Callback( ( ) => mock2Counter.Signal( ) );
					using ( var controller2 = new BackgroundTaskController( redisMgr, mockTm2.Object ) )
					{
						controller1.StartAll( );
						mock1Counter.Wait( 5000 );
						mock2Counter.Wait( 5000 );

						mock1Counter.Reset( );
						mock2Counter.Reset( );


						controller2.StartAll( );
						mock1Counter.Wait( 5000 );
						mock2Counter.Wait( 5000 );

						mockTm1.VerifyAll( );
						mockTm2.VerifyAll( );
					}
				}
			}
		}


		[Test]
		public void StopAll( )
		{
			var redisMgr = Factory.DistributedMemoryManager;

			// active TM
			var mockTm11 = new Mock<IBackgroundTaskManager>( MockBehavior.Strict );
			mockTm11.Name = nameof( mockTm11 );
			mockTm11.Setup( tm => tm.IsActive ).Returns( true );
			mockTm11.Setup( tm => tm.Stop( ) );
			mockTm11.Setup( tm => tm.IsActive ).Returns( true );
			mockTm11.Setup( tm => tm.Stop( ) );
			using ( var controller1 = new BackgroundTaskController( redisMgr, mockTm11.Object ) )
			{
				// passive TM
				var mockTm22 = new Mock<IBackgroundTaskManager>( MockBehavior.Strict );
				mockTm22.Name = nameof( mockTm22 );
				mockTm22.Setup( tm => tm.IsActive ).Returns( false );
				mockTm22.Setup( tm => tm.IsActive ).Returns( false );
				using ( var controller2 = new BackgroundTaskController( redisMgr, mockTm22.Object ) )
				{
					controller1.StopAll( );
					controller2.StopAll( );

					mockTm11.VerifyAll( );
					mockTm22.VerifyAll( );
				}
			}
		}

		[Test]
		public void TwoControllersTalking( )
		{
		}
	}
}