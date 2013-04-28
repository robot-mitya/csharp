// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextReceivedEventArgs.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   The instance of this class is sent in the event after receiving data through COM-port or UDP-communication.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboCommon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The instance of this class is sent in the event after receiving data through COM-port or UDP-communication.
    /// </summary>
    public class TextReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextReceivedEventArgs" /> class.
        /// </summary>
        /// <param name="text">Text received by COM-port or UDP socket.</param>
        public TextReceivedEventArgs(string text)
        {
            this.Text = text;
        }

        /// <summary>
        /// Gets the text received by COM-port or UDP socket.
        /// </summary>
        public string Text { get; private set; }
    }
}
