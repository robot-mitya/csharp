// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScreenConstants.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   Constants that are used for screen output.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboControl
{
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Constants that are used for screen output.
    /// </summary>
    public static class ScreenConstants
    {
        /// <summary>
        /// The screen is devided by cells. This is the height of the cell.
        /// </summary>
        private static int cellHeight = 25;

        /// <summary>
        /// The screen is devided by cells. This is the width of the cell.
        /// </summary>
        private static int cellWidth = 190;

        /// <summary>
        /// Position in pixels of the left upper cell.
        /// </summary>
        private static Vector2 startPosition = new Vector2(20, 20);

        /// <summary>
        /// Gets position in pixels of the cell.
        /// </summary>
        /// <param name="column">Cell column index.</param>
        /// <param name="row">Cell row index.</param>
        /// <returns>Left upper coordinate of the cell in pixels.</returns>
        public static Vector2 GetTextPosition(int column, int row)
        {
            return new Vector2(
                startPosition.X + (column * cellWidth),
                startPosition.Y + (row * cellHeight));
        }
    }
}
