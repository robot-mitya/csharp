// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2012
// </copyright>
// <summary>
//   Главный класс приложения.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboConsole
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;

    /// <summary>
    /// Главный класс приложения.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Title for error message boxes.
        /// </summary>
        private const string ErrorTitle = "Windows Forms Error";

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            // Add the event handler for handling UI thread exceptions to the event.
            Application.ThreadException += 
                new ThreadExceptionEventHandler(Program.UIThreadExceptionHandler);

            // Set the unhandled exception mode to force all Windows Forms errors to go through our handler.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(Program.CurrentDomain_UnhandledException); 
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }

        /// <summary>
        /// Handle for untrapped thread exceptions.
        /// </summary>
        /// <param name="sender">
        /// Sender object.
        /// </param>
        /// <param name="t">
        /// Exception event arguments.
        /// </param>
        private static void UIThreadExceptionHandler(object sender, ThreadExceptionEventArgs t)
        {
            DialogResult result = DialogResult.Cancel;
            try
            {
                result = Program.ShowThreadExceptionDialog(t.Exception);
            }
            catch
            {
                try
                {
                    string text = "Fatal " + ErrorTitle;
                    MessageBox.Show(text, text, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
                }
                finally
                {
                    Application.Exit();
                }
            }

            // Exits the program when the user clicks Abort.
            if (result == DialogResult.Abort)
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// Opens a message box with information about exception.
        /// </summary>
        /// <param name="e">Trapped exception.</param>
        /// <returns>Dialog result.</returns>
        private static DialogResult ShowThreadExceptionDialog(Exception e)
        {
            string errorMsg = "An application error occurred.\n\n";
            errorMsg = errorMsg + e.Message + "\n\nStack Trace:\n" + e.StackTrace;
            return MessageBox.Show(errorMsg, ErrorTitle, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
        }

        /// <summary>
        /// Occurs when an exception is not caught.
        /// </summary>
        /// <param name="sender">
        /// Sender object.
        /// </param>
        /// <param name="e">
        /// Exception event arguments.
        /// </param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            string errorMsg = "An application error occurred.\n\n";
            errorMsg = errorMsg + ex.Message + "\n\nStack Trace:\n" + ex.StackTrace;
            MessageBox.Show(errorMsg, ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }
    }
}
