// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaudRateItem.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   COM port baud rate item for ComboBox.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboConsole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// COM port baud rate item for ComboBox.
    /// </summary>
    public class BaudRateItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaudRateItem"/> class.
        /// </summary>
        /// <param name="baudRate">
        /// Baud rate.
        /// </param>
        public BaudRateItem(int baudRate)
        {
            this.BaudRate = baudRate;
        }

        /// <summary>
        /// Gets or sets the baud rate.
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// Gets the Baud rate text.
        /// </summary>
        /// <returns>
        /// Baud rate for ComboBox.
        /// </returns>
        public override string ToString()
        {
            return this.BaudRate.ToString() + " бод";
        }
    }
}
