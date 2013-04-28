// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2011
// </copyright>
// <summary>
//   Главный класс приложения.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboControl
{
    using System;

#if WINDOWS || XBOX
    /// <summary>
    /// Главный класс приложения.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">
        /// Параметры командной строки.
        /// </param>
        public static void Main(string[] args)
        {
            using (GameRobot game = new GameRobot())
            {
                game.Run();
            }
        }
    }
#endif
}