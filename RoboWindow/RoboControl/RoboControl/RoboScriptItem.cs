// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoboScriptItem.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2012
// </copyright>
// <summary>
//   Класс для хранения одного из 10-ти РобоСкриптов.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using RoboCommon;

    /// <summary>
    /// Класс для хранения одного из 10-ти РобоСкриптов.
    /// </summary>
    public sealed class RoboScriptItem
    {
        /// <summary>
        /// Максимальный номер РобоСкрипта.
        /// </summary>
        private const byte MaxRoboScriptNumber = 9;

        /// <summary>
        /// Initializes a new instance of the RoboScriptItem class.
        /// </summary>
        /// <param name="roboScriptNumber">Номер РобоСкрипта от 0 до MaxRoboScriptNumber.</param>
        public RoboScriptItem(byte roboScriptNumber)
        {
            if ((roboScriptNumber < 0) || (roboScriptNumber > MaxRoboScriptNumber))
            {
                throw new ArgumentOutOfRangeException("roboScriptNumber");
            }

            this.RoboScriptNumber = roboScriptNumber;
        }

        /// <summary>
        /// Gets Номер РобоСкрипта от 0 до MaxRoboScriptNumber.
        /// </summary>
        public byte RoboScriptNumber { get; private set; }

        /// <summary>
        /// Gets Текст РобоСкрипта с разделителями-запятыми.
        /// </summary>
        public string RoboScript { get; private set; }

        /// <summary>
        /// Gets Команда для воспроизведения данного РобоСкрипта.
        /// </summary>
        public string PlayCommand { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether РобоСкрипт отправлялся роботу.
        /// </summary>
        public bool WasSent { get; set; }

        /// <summary>
        /// Gets a value indicating whether отсутствует текст РобоСкрипта в элементе.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this.RoboScript.Equals(string.Empty);
            }
        }

        /// <summary>
        /// Sets RoboScript and PlayCommand properties.
        /// </summary>
        /// <param name="value">RoboScript text.</param>
        public void SetRoboScript(string value)
        {
            // Текст любого РобоСкрипта начинается с сообщения r01xx. Где хх это номер РобоСкрипта.
            // В результате ошибки, в опциях приложения номер может стоять неверный номер.
            // Поэтому подправляем номер в соответствии с номером this.roboScriptNumber.
            if ((value.Length >= CommunicationHelper.MessageLength) && value.StartsWith("r01"))
            {
                string numberHex = this.RoboScriptNumber.ToString("X2");

                this.RoboScript = value;
                this.RoboScript = this.RoboScript.Insert(CommunicationHelper.MessageLength - 2, numberHex);
                this.RoboScript = this.RoboScript.Remove(CommunicationHelper.MessageLength, 2);

                this.PlayCommand = "r00" + numberHex;
            }
            else
            {
                this.RoboScript = string.Empty;
                this.PlayCommand = string.Empty;
            }
        }
    }
}
