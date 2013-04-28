// <copyright file="CommandHistoryTest.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2012
// </copyright>
// <summary>
//   Класс, тестирующий класс CommandHistory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboConsoleTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    using NUnit.Framework;
    
    using RoboConsole;

    /// <summary>
    /// Класс, тестирующий класс CommandHistory.
    /// </summary>
    [TestFixture]
    public sealed class CommandHistoryTest
    {
        /// <summary>
        /// Тест пустой истории.
        /// </summary>
        [Test]
        public void TestEmptyHistoryNavigation()
        {
            var commandHistory = new CommandHistory();
            
            string command = commandHistory.GetPreviousCommand();
            Assert.IsEmpty(command);

            command = commandHistory.GetNextCommand();
            Assert.IsEmpty(command);
        }

        /// <summary>
        /// Тест метода GetPreviousCommand().
        /// </summary>
        [Test]
        public void TestGetPreviousCommand()
        {
            var commandHistory = new CommandHistory();

            commandHistory.Add("A0001");            
            string command = commandHistory.GetPreviousCommand();
            Assert.AreEqual("A0001", command);
            command = commandHistory.GetPreviousCommand();
            Assert.AreEqual("A0001", command);

            commandHistory.Add("A0002");
            command = commandHistory.GetPreviousCommand();
            Assert.AreEqual("A0002", command);
            command = commandHistory.GetPreviousCommand();
            Assert.AreEqual("A0001", command);
            command = commandHistory.GetPreviousCommand();
            Assert.AreEqual("A0001", command);

            commandHistory.Add("A0003");
            command = commandHistory.GetPreviousCommand();
            Assert.AreEqual("A0003", command);
            command = commandHistory.GetPreviousCommand();
            Assert.AreEqual("A0002", command);
            command = commandHistory.GetPreviousCommand();
            Assert.AreEqual("A0001", command);
            command = commandHistory.GetPreviousCommand();
            Assert.AreEqual("A0001", command);
        }

        /// <summary>
        /// Тест метода GetNextCommand().
        /// </summary>
        [Test]
        public void TestGetNextCommand()
        {
            var commandHistory = new CommandHistory();

            commandHistory.Add("A0001");
            string command = commandHistory.GetNextCommand();
            Assert.IsEmpty(command);
            command = commandHistory.GetPreviousCommand();
            Assert.AreEqual("A0001", command);
            command = commandHistory.GetNextCommand();
            Assert.IsEmpty(command);

            commandHistory.Add("A0002");
            command = commandHistory.GetPreviousCommand();
            Assert.AreEqual("A0002", command);
            command = commandHistory.GetPreviousCommand();
            Assert.AreEqual("A0001", command);
            command = commandHistory.GetNextCommand();
            Assert.AreEqual("A0002", command);
            command = commandHistory.GetNextCommand();
            Assert.IsEmpty(command);
        }
    }
}
