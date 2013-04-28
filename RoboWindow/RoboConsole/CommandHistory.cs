// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandHistory.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2012
// </copyright>
// <summary>
//   История введённых команд.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboConsole
{
    using System.Collections.Generic;

    /// <summary>
    /// История введённых команд.
    /// </summary>
    public class CommandHistory
    {
        /// <summary>
        /// История передаваемых команд (список).
        /// </summary>
        private IList<string> history = new List<string>();

        /// <summary>
        /// Текущая позиция. Используется при переборе команд из истории.
        /// </summary>
        private int currentPosition = 0;

        /// <summary>
        /// Добавление команды в историю.
        /// </summary>
        /// <param name="command">
        /// Выполненная и сохраняемая в истории команда.
        /// </param>
        public void Add(string command)
        {
            this.history.Add(command);
            this.currentPosition = this.history.Count;
        }

        /// <summary>
        /// Смещение на одну команду назад по истории.
        /// </summary>
        /// <returns>
        /// Выбранная команда.
        /// </returns>
        public string GetPreviousCommand()
        {
            if (this.history.Count == 0)
            {
                return string.Empty;
            }

            this.currentPosition--;
            if (this.currentPosition < 0)
            {
                this.currentPosition = 0;
            }

            return this.history[this.currentPosition];
        }

        /// <summary>
        /// Смещение на одну команду вперёд по истории.
        /// </summary>
        /// <returns>
        /// Выбранная команда.
        /// </returns>
        public string GetNextCommand()
        {
            if (this.history.Count == 0)
            {
                return string.Empty;
            }

            this.currentPosition++;
            if (this.currentPosition >= this.history.Count)
            {
                this.currentPosition = this.history.Count;
                return string.Empty;
            }

            return this.history[this.currentPosition];
        }
    }
}
