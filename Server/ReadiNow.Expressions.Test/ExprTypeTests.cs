// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;

namespace ReadiNow.Expressions.Test
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class ExprTypeTests
    {
        [Test]
        public void FromFieldEntity_String()
        {
            var f = new StringField();

            ExprType res = ExprTypeHelper.FromFieldEntity(f);
            Assert.That(res.Type, Is.EqualTo(DataType.String));
        }

        [Test]
        public void FromFieldEntity_Decimal()
        {
            var f = new DecimalField();
            f.DecimalPlaces = 5;

            ExprType res = ExprTypeHelper.FromFieldEntity(f);
            Assert.That(res.Type, Is.EqualTo(DataType.Decimal));
            Assert.That(res.DecimalPlaces, Is.EqualTo(5));
        }

        [Test]
        public void FromFieldEntity_Currency()
        {
            var f = new CurrencyField();
            f.DecimalPlaces = 5;

            ExprType res = ExprTypeHelper.FromFieldEntity(f);
            Assert.That(res.Type, Is.EqualTo(DataType.Currency));
            Assert.That(res.DecimalPlaces, Is.EqualTo(5));
        }
    }
}