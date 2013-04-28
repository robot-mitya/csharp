// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComPortItem.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   Description of COM port for ComboBox.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Description of COM port.
    /// </summary>
    public class ComPortItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComPortItem"/> class.
        /// </summary>
        /// <param name="name">
        /// COM port name.
        /// </param>
        public ComPortItem(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComPortItem"/> class.
        /// </summary>
        /// <param name="name">
        /// COM port name.
        /// </param>
        /// <param name="arduinoConnected">
        /// Indicates whether controller is connected or not.
        /// </param>
        public ComPortItem(string name, bool arduinoConnected)
        {
            this.Name = name;
            this.ArduinoConnected = arduinoConnected;
        }

        /// <summary>
        /// Gets or sets the COM port name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Arduino is connected to this COM port.
        /// </summary>
        public bool ArduinoConnected { get; set; }

        /// <summary>
        /// Gets the COM port name with an indicator if Arduino controller is connected.
        /// </summary>
        /// <returns>Port name with Arduino indicator if it is connected.</returns>
        public override string ToString()
        {   
            return this.ArduinoConnected ? this.Name + " (Arduino)" : this.Name;
        }
    }
}
