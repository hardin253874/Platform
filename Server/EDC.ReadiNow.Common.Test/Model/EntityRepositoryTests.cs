// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.Test.Model
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class EntityRepositoryTests
    {
        [Test]
        public void Test_CanCreate( )
        {
            Assert.That( Factory.EntityRepository, Is.Not.Null );
        }

        //[Test]
        //public void Test_CanMock( )
        //{
        //    IEntityRepository repo = new Mock<IEntityRepository>( ).Object;
        //    Assert.That( repo, Is.Not.Null );
        //}
    }
}
