// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleSettings.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   Class for application settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class for application settings.
    /// </summary>
    public class ConsoleSettings
    {
        /// <summary>
        /// Initializes a new instance of the ConsoleSettings class.
        /// </summary>
        public ConsoleSettings()
        {
            this.RoboHeadAddress = "192.168.1.1";
            this.UdpSendPort = 51974;
            this.UdpReceivePort = 51973;
            this.ComPort = "COM3";
            this.BaudRate = 9600;
            this.SingleMessageRepetitionsCount = 3;
        }

        /// <summary>
        /// Gets or sets IP-address of the phone.
        /// </summary>
        public string RoboHeadAddress { get; set; }

        /// <summary>
        /// Gets or sets UDP port for datagram output.
        /// </summary>
        public ushort UdpSendPort { get; set; }

        /// <summary>
        /// Gets or sets UDP port for datagram input.
        /// </summary>
        public ushort UdpReceivePort { get; set; }

        /// <summary>
        /// Gets or sets COM port to communicate directly with Arduino controller.
        /// </summary>
        public string ComPort { get; set; }

        /// <summary>
        /// Gets or sets baud rate of the COM port to communicate directly with Arduino controller.
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// Gets or sets quantity of send message repetitions.
        /// </summary>
        /// <remarks>
        /// We use UDP datagrams so single messages like I0001 can be lost. That's why we repeat sending for SingleMessageRepetitionsCount times. Three times by default.
        /// </remarks>
        public byte SingleMessageRepetitionsCount { get; set; }
    }
}
