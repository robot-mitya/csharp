// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Runner.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   Главный класс проекта. Обеспечивает запуск тестов. Решение разрабатывалось в среде
//   Microsoft Visual C# Express с использованием фреймворка NUnit. Настройка среды описана
//   здесь: http://hobbinsblog.blogspot.com/2008/06/nunit-visual-studio-express.html
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboCommonTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Главный класс проекта.
    /// </summary>
    public class Runner
    {
        /// <summary>
        /// Точка входа. Запускает выполнение тестов.
        /// </summary>
        /// <param name="args">
        /// Аргументы приложения.
        /// </param>
        [STAThread]
        public static void Main(string[] args)
        {
            NUnit.ConsoleRunner.Runner.Main(args);
        }
    }
}
