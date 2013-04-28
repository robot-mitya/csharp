// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoboScriptItemTest.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2012
// </copyright>
// <summary>
//   Класс, тестирующий класс RoboScriptItemTest.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboControlTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;
    using RoboControl;

    /// <summary>
    /// Класс, тестирующий класс RoboScriptItemTest.
    /// </summary>
    [TestFixture]
    public sealed class RoboScriptItemTest
    {
        /// <summary>
        /// Тест конструктора #1.
        /// </summary>
        [Test]
        public void ConstructorTest1()
        {
            var item = new RoboScriptItem(0);
            Assert.IsNotNull(item);
            
            item = new RoboScriptItem(5);
            Assert.IsNotNull(item);
            
            item = new RoboScriptItem(9);
            Assert.IsNotNull(item);
        }

        /// <summary>
        /// Тест конструктора #2.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorTest2()
        {
            var item = new RoboScriptItem(10);
        }

        /// <summary>
        /// Тест метода Update().
        /// </summary>
        [Test]
        public void UpdateTest()
        {
            var item = new RoboScriptItem(5);
            item.SetRoboScript("r0100, Z0001, I0001, W0000, Z0000");

            Assert.AreEqual(5, item.RoboScriptNumber);
            Assert.AreEqual("r0105, Z0001, I0001, W0000, Z0000", item.RoboScript);
            Assert.AreEqual("r0005", item.PlayCommand);
        }
    }
}
