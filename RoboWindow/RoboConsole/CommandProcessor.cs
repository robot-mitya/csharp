// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandProcessor.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   Functions to control sending and receiving commands and messages.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    using RoboCommon;

    /// <summary>
    /// Functions to control sending and receiving commands and messages.
    /// </summary>
    public class CommandProcessor
    {
        /// <summary>
        /// Commands' history.
        /// </summary>
        private readonly CommandHistory commandHistory;

        /// <summary>
        /// Text box to input commands.
        /// </summary>
        private readonly TextBox commandLineTextBox;

        /// <summary>
        /// Information area for commands' and messages' history.
        /// </summary>
        private readonly HistoryBox historyBox;

        /// <summary>
        /// Class that provide communication with robot.
        /// </summary>
        private readonly ICommunicationHelper communicationHelper;

        /// <summary>
        /// Initializes a new instance of the CommandProcessor class.
        /// </summary>
        /// <param name="commandLineTextBox">
        /// Control to input commands.
        /// </param>
        /// <param name="historyBox">
        /// Control for commands and responses history.
        /// </param>
        /// <param name="communicationHelper">
        /// Class that communicates with robot.
        /// </param>
        /// <param name="commandHistory">
        /// Class that containts command history.
        /// </param>
        public CommandProcessor(
            TextBox commandLineTextBox, 
            HistoryBox historyBox, 
            ICommunicationHelper communicationHelper, 
            CommandHistory commandHistory)
        {
            this.commandLineTextBox = commandLineTextBox;
            this.historyBox = historyBox;
            this.communicationHelper = communicationHelper;
            this.commandHistory = commandHistory;
        }

        /// <summary>
        /// Processes command in the command line control.
        /// </summary>
        public void ProcessCommand()
        {
            this.commandHistory.Add(this.commandLineTextBox.Text);
            
            IEnumerable<string> commands = CommunicationHelper.ParseRoboScript(this.commandLineTextBox.Text);

            var notSentCommands = new List<string>();
            foreach (string command in commands)
            {
                // Пустышки в сторону!
                if (command.Trim().Equals(string.Empty))
                {
                    continue;
                }

                // Если есть ошибка, то все последующие команды не посылаются роботу.
                if (notSentCommands.Count > 0)
                {
                    notSentCommands.Add(command);
                }
                else
                {
                    bool sendResult = this.SendMessageToRobot(false, command);

                    if (sendResult)
                    {
                        this.historyBox.AppendTextSentToRobot(this.communicationHelper.LastSentMessage);
                    }
                    else
                    {
                        this.historyBox.AppendTextSentToRobot(string.Format("Ошибка в {0}: {1}", command, this.communicationHelper.LastErrorMessage));
                        notSentCommands.Add(command);
                    }
                }
            }

            // В поле ввода команд оставляем только неотправленные команды.
            if (notSentCommands.Count > 0)
            {
                this.commandLineTextBox.Text = GenerateCommandLine(notSentCommands);
            }
            else
            {
                this.commandLineTextBox.Text = string.Empty;
            }
        }

        /// <summary>
        /// Navigate through commands' history list to select previous command.
        /// The command is automatically typed into command line control.
        /// </summary>
        public void SelectPreviousCommand()
        {
            this.commandLineTextBox.Text = this.commandHistory.GetPreviousCommand();
            this.commandLineTextBox.SelectionStart = this.commandLineTextBox.Text.Length;
        }

        /// <summary>
        /// Navigate through commands' history list to select next command.
        /// The command is automatically typed into command line control.
        /// </summary>
        public void SelectNextCommand()
        {
            this.commandLineTextBox.Text = this.commandHistory.GetNextCommand();
            this.commandLineTextBox.SelectionStart = this.commandLineTextBox.Text.Length;
        }

        /// <summary>
        /// Generate string with commands separated by commas.
        /// </summary>
        /// <param name="commands">
        /// Commands list.
        /// </param>
        /// <returns>
        /// Result string.
        /// </returns>
        private static string GenerateCommandLine(IEnumerable<string> commands)
        {
            string result = string.Empty;
            const string CommandSeparator = ", ";
            foreach (string command in commands)
            {
                result += CommandSeparator + command;
            }

            if (result.Length > 0)
            {
                result = result.Remove(0, CommandSeparator.Length);
            }

            return result;
        }

        /// <summary>
        /// Send command to robot through Wi-Fi (UDP) or COM-port.
        /// </summary>
        /// <param name="throughCom">
        /// if true then communication is through COM-port, else – through WiFi.
        /// </param>
        /// <param name="command">
        /// The command to send.
        /// </param>
        /// <returns>
        /// false on error.
        /// </returns>
        private bool SendMessageToRobot(bool throughCom, string command)
        {
            return this.communicationHelper.SendMessageToRobot(command);
        }
    }
}
