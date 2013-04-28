// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HistoryBox.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   Class that contains text area to display history of commands and messages and its functionality.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboConsole
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// Class that contains text area to display history of commands and messages and its functionality.
    /// </summary>
    public class HistoryBox
    {
        /// <summary>
        /// Incapsulated text area to display commands and messages.
        /// </summary>
        private readonly RichTextBox textBox;

        /// <summary>
        /// Initializes a new instance of the HistoryBox class.
        /// </summary>
        /// <param name="textBox">
        /// Text area for history commands and messages.
        /// </param>
        public HistoryBox(RichTextBox textBox)
        {
            this.textBox = textBox;
        }

        /// <summary>
        /// Append command or message text to history text area that was sent to robot.
        /// </summary>
        /// <param name="text">
        /// Text to append.
        /// </param>
        public void AppendTextSentToRobot(string text)
        {
            this.textBox.SelectionColor = SystemColors.WindowText;

            int linesCount = this.textBox.Lines.Length;
            if (linesCount > 0)
            {
                if (this.textBox.Lines[linesCount - 1].Length > 0)
                {
                    this.textBox.AppendText(Environment.NewLine);
                }
            }

            this.textBox.AppendText(text);
            this.textBox.AppendText(Environment.NewLine);
            this.textBox.ScrollToCaret();
        }

        /// <summary>
        /// Append command or message text to history text area that was received from robot.
        /// </summary>
        /// <param name="text">
        /// Text to append.
        /// </param>
        public void AppendTextReceivedFromRobot(string text)
        {
            this.textBox.SelectionColor = Color.DarkBlue;
            this.textBox.AppendText(text);
            this.textBox.ScrollToCaret();
        }
    }
}
