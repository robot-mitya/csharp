// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommunicationHelper.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2011
// </copyright>
// <summary>
//   Класс для взаимодействия с головой робота.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboCommon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Ports;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    /// <summary>
    /// The delegate for the event after receiving data through COM-port or UDP-communication.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments that contains received text.</param>
    public delegate void TextReceivedEventHandler(object sender, TextReceivedEventArgs e);

    /// <summary>
    /// Abstract class for communicating with the robot.
    /// </summary>
    public abstract class CommunicationHelper : ICommunicationHelper, IDisposable
    {
        /// <summary>
        /// Constant length of message in our language.
        /// </summary>
        private static byte messageLength = 5;

        /// <summary>
        /// Initializes a new instance of the CommunicationHelper class.
        /// </summary>
        /// <param name="nonrecurrentMessageRepetitions">
        /// Number of repetitions we send nonrecurrent messages to the robot.
        /// </param>
        protected CommunicationHelper(int nonrecurrentMessageRepetitions)
        {
            this.NonrecurrentMessageRepetitions = nonrecurrentMessageRepetitions;
            this.LastErrorMessage = string.Empty;
            this.LastSentMessage = string.Empty;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CommunicationHelper" /> class.
        /// </summary>
        ~CommunicationHelper()
        {
            this.FinalizePort();
        }

        /// <summary>
        /// The event after receiving data through COM-port or UDP-communication.
        /// </summary>
        public event TextReceivedEventHandler TextReceived;

        /// <summary>
        /// Gets the length of message in our language.
        /// </summary>
        public static byte MessageLength
        {
            get
            {
                return messageLength;
            }
        }

        /// <summary>
        /// Gets or sets number of repetitions we send nonrecurrent messages to the robot.
        /// </summary>
        /// <remarks>
        /// Nonrecurrent messages (such as M, t, y, n) can be lost while transferring.
        /// That's why they we send them nonrecurrentMessageRepetitions times. Three times by default.
        /// </remarks>
        public int NonrecurrentMessageRepetitions { get; set; }

        /// <summary>
        /// Gets or sets Текст последней ошибки.
        /// </summary>  
        public string LastErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets Последнее успешно отправленное роботу сообщение.
        /// Сообщение, передаваемое методу SendMessageToRobot, может быть представлено в краткой форме.
        /// Числовое значение может отсутствовать или не содержать лидирующих нулей. После успешной отправки
        /// свойство LastSentMessage будет содержать это сообщение, представленное в полной форме.
        /// </summary>  
        public string LastSentMessage { get; protected set; }

        /// <summary>
        /// Получение списка команд из строки на РобоСкрипте с разделителями-запятыми.
        /// </summary>
        /// <param name="roboScript">Текст РобоСкрипта.</param>
        /// <returns>Список команд.</returns>
        public static IEnumerable<string> ParseRoboScript(string roboScript)
        {
            return roboScript.Split(',').Select(x => x.Trim()).ToArray();
        }

        /// <summary>
        /// Releases communication resources.
        /// </summary>
        public void Dispose()
        {
            this.FinalizePort();
        }

        /// <summary>
        /// Передать роботу сообщение.
        /// </summary>
        /// <param name="message">
        /// Текст сообщения.
        /// </param>
        /// <returns>
        /// true, если нет ошибок.
        /// </returns>
        public bool SendMessageToRobot(string message)
        {
            string correctedMessage;
            try
            {
                if (!this.CorrectMessage2(message, out correctedMessage))
                {
                    return false;
                }

                this.TransmitMessage(correctedMessage);
            }
            catch (Exception e)
            {
                this.LastErrorMessage = e.Message;
                return false;
            }

            this.LastSentMessage = correctedMessage;
            return true;
        }

        /// <summary>
        /// Передать роботу неповторяющееся сообщение.
        /// </summary>
        /// <param name="message">
        /// Сообщение, передаваемое роботу.
        /// </param>
        /// <param name="voidMessage">
        /// Сообщение с этим же идентификатором, но с "пустым" значением.</param>
        /// <returns>
        /// true, если нет ошибок.
        /// </returns>
        /// <remarks>
        /// Сообщения для управления моторами, например, повторяются постоянно, каждые несколько милисекунд.
        /// Потеря одного такого сообщения несущественно. А сообщения, например, смены настроения (мордочки)
        /// передаются по команде оператора. Потеря - пропуск команды. Поэтому сообщения повторяются 
        /// несколько раз. Особенность обработки сообщений на приёмной стороне (хэш таблица для исключения
        /// обработки абсолютно идентичных поступивших друг за другом сообщений) требует после команды смены
        /// настроения дать аналогичную команду с "пустым" значением.
        /// </remarks>
        public bool SendNonrecurrentMessageToRobot(string message, string voidMessage)
        {
            int repetitions = this.NonrecurrentMessageRepetitions;

            for (int i = 0; i < repetitions; i++)
            {
                if (!this.SendMessageToRobot(message))
                {
                    return false;
                }
            }

            if (voidMessage != string.Empty)
            {
                for (int i = 0; i < repetitions; i++)
                {
                    if (!this.SendMessageToRobot(voidMessage))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        /// <summary>
        /// Передать роботу неповторяющееся сообщение.
        /// </summary>
        /// <param name="message">
        /// Сообщение, передаваемое роботу.
        /// </param>
        /// <returns>
        /// true, если нет ошибок.
        /// </returns>
        /// <remarks>
        /// Сообщения для управления моторами, например, повторяются постоянно, каждые несколько милисекунд.
        /// Потеря одного такого сообщения несущественно. А сообщения, например, смены настроения (мордочки)
        /// передаются по команде оператора. Потеря - пропуск команды. Поэтому такие сообщения повторяются 
        /// несколько раз.
        /// </remarks>
        public bool SendNonrecurrentMessageToRobot(string message)
        {
            return this.SendNonrecurrentMessageToRobot(message, string.Empty);
        }

        /// <summary>
        /// Передача текста РобоСкрипта роботу.
        /// </summary>
        /// <param name="roboScript">Текст РобоСкрипта с разделителями-запятыми.</param>
        /// <returns>false, если возникла ошибка. Тест ошибки будет в свойстве LastErrorMessage.</returns>
        public bool SendRoboScriptToRobot(string roboScript)
        {
            IEnumerable<string> commands = ParseRoboScript(roboScript);

            foreach (string command in commands)
            {
                bool result = this.SendMessageToRobot(command);
                if (!result)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generates the text received event.
        /// </summary>
        /// <param name="e">Event arguments that contains received text.</param>
        protected virtual void OnTextReceived(TextReceivedEventArgs e)
        {
            if (this.TextReceived != null)
            {
                this.TextReceived(this, e);
            }
        }

        /// <summary>
        /// Проверка и коррекция сообщения. Сообщение приводится к виду: идентификатор (1 символ), 
        /// HEX значение (4 символа 0..9, A..F).
        /// </summary>
        /// <param name="message">Исходное значение. В значении могут отсутствовать лидирующие нули.</param>
        /// <param name="correctedMessage">Скорректированное пятисимвольное сообщение.</param>
        /// <returns>true, если удалось, false и LastErrorMessage если нет.</returns>
        protected bool CorrectMessage2(string message, out string correctedMessage)
        {
            try
            {
                correctedMessage = MessageHelper.CorrectMessage(message);
                return true;
            }
            catch (RoboMessageException e)
            {
                correctedMessage = string.Empty;
                this.LastErrorMessage = e.Message;
                return false;
            }
        }

        /// <summary>
        /// Internal method for message transmition. Should be overridden in subclasses for UDP or COM serial transmission.
        /// Doesn't correct message. Doesn't handle errors, just generate exceptions.
        /// </summary>
        /// <param name="message">
        /// Message to transmit.
        /// </param>
        protected abstract void TransmitMessage(string message);

        /// <summary>
        /// Communication finalization. Called in Dispose method.
        /// </summary>
        protected abstract void FinalizePort();
    } // class
} // namespace
