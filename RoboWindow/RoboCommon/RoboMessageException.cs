// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoboMessageException.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   Exceptions in message parsing and execution.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboCommon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Exceptions in message parsing and execution.
    /// </summary>
    public class RoboMessageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoboMessageException" /> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        public RoboMessageException(string message)
            : base(message)
        {
        }
    }
}
