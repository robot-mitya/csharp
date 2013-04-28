// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FormMain.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2012
// </copyright>
// <summary>
//   Главная форма приложения.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboConsole
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    using RoboCommon;

    /// <summary>
    /// Главная форма приложения.
    /// </summary>
    public partial class FormMain : Form
    {
        /// <summary>
        /// Settings helper.
        /// </summary>
        private readonly ConsoleSettingsHelper settingsHelper = new ConsoleSettingsHelper();

        /// <summary>
        /// History of inputed commands.
        /// </summary>
        private readonly CommandHistory commandHistory = new CommandHistory();

        /// <summary>
        /// Object that contains text area to display history of commands and messages and its functionality.
        /// </summary>
        private readonly HistoryBox historyBox;

        /// <summary>
        /// Object that provide communication with robot.
        /// </summary>
        private CommunicationHelper communicationHelper;

        /// <summary>
        /// Object that provides command's processing: navigation in history list, redrawing text boxes, command line processing.
        /// </summary>
        private CommandProcessor commandProcessor;

        /// <summary>
        /// Initializes a new instance of the FormMain class.
        /// </summary>
        public FormMain()
        {
            this.InitializeComponent();
            this.historyBox = new HistoryBox(this.textBoxHistory);
        }

        /// <summary>
        /// This event handler is called after form creation.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void FormMain_Load(object sender, EventArgs e)
        {
            this.labelVersion.Text = "ver." + Application.ProductVersion;

            LinkLabel.Link link = new LinkLabel.Link();
            link.LinkData = "http://www.robot-mitya.ru/articles/?SECTION_ID=98&ELEMENT_ID=386";
            this.linkLabelHelp.Links.Add(link);

            this.settingsHelper.Load();
            this.InitializeCommunication();
        }

        /// <summary>
        /// Обработчик кнопки Отправить.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void ButtonSend_Click(object sender, EventArgs e)
        {
            this.commandProcessor.ProcessCommand();
        }

        /// <summary>
        /// Обработчик нажатия на клавишу поля ввода команд.
        /// </summary>
        /// <param name="sender">Источник события.</param>
        /// <param name="e">Аргументы события.</param>
        private void TextBoxSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Alt && !e.Shift && !e.Control) 
            {
                // nothing
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        this.commandProcessor.ProcessCommand();
                        break;
                    case Keys.Up:
                        this.commandProcessor.SelectPreviousCommand();
                        e.Handled = true;
                        break;
                    case Keys.Down:
                        this.commandProcessor.SelectNextCommand();
                        e.Handled = true;
                        break;
                }
            }
            else if (!e.Alt && !e.Shift && e.Control) 
            {
                // ctrl is pressed
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        this.radioButtonUdpSocket.Checked = true;
                        e.Handled = true;
                        break;
                    case Keys.Down:
                        this.radioButtonComPort.Checked = true;
                        e.Handled = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Radio button checked changed event handler.
        /// </summary>
        /// <param name="sender">Sender control.</param>
        /// <param name="e">Event arguments.</param>
        private void RadioButtonComPortCheckedChanged(object sender, EventArgs e)
        {
            this.communicationHelper.Dispose();

            this.InitializeCommunication();

            this.textBoxSend.SelectAll();
            this.textBoxSend.Focus();
        }

        /// <summary>
        /// Initialization of communication with robot through UDP-socket or COM-port.
        /// </summary>
        private void InitializeCommunication()
        {
            if (this.radioButtonComPort.Checked)
            {
                this.InitializeComPortCommunication();
            }
            else
            {
                this.InitializeUdpCommunication();
            }
        }

        /// <summary>
        /// Initialization of communication with robot through UDP-socket.
        /// </summary>
        private void InitializeUdpCommunication()
        {
            this.communicationHelper = new UdpCommunicationHelper(
                this.settingsHelper.Settings.RoboHeadAddress,
                this.settingsHelper.Settings.UdpSendPort,
                this.settingsHelper.Settings.UdpReceivePort,
                this.settingsHelper.Settings.SingleMessageRepetitionsCount);
            this.communicationHelper.TextReceived += this.OnTextReceived;

            this.commandProcessor = new CommandProcessor(
                this.textBoxSend, 
                this.historyBox, 
                this.communicationHelper,
                this.commandHistory);
        }

        /// <summary>
        /// Initialization of communication with robot through COM-port.
        /// </summary>
        private void InitializeComPortCommunication()
        {
            this.communicationHelper = new ComPortCommunicationHelper(
                this.settingsHelper.Settings.ComPort,
                this.settingsHelper.Settings.BaudRate,
                this.settingsHelper.Settings.SingleMessageRepetitionsCount);
            this.communicationHelper.TextReceived += this.OnTextReceived;

            this.commandProcessor = new CommandProcessor(
                this.textBoxSend, 
                this.historyBox, 
                this.communicationHelper,
                this.commandHistory);
        }

        /// <summary>
        /// The event handler to process received data through COM-port or UDP communication.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments that contains received text.</param>
        private void OnTextReceived(object sender, TextReceivedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                // Using this.Invoke causes deadlock when closing serial port, and BeginInvoke is good practice anyway.                
                this.BeginInvoke(new EventHandler<TextReceivedEventArgs>(this.OnTextReceived), new object[] { sender, e });
                return;
            }

            this.historyBox.AppendTextReceivedFromRobot(e.Text);
        }

        /// <summary>
        /// LinkLabelHelp handler.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void LinkLabelHelpLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        /// <summary>
        /// LinkLabelSettings handler.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void LinkLabelSettingsLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var formSettings = new FormSettings();
            formSettings.IpAddress = this.settingsHelper.Settings.RoboHeadAddress;
            formSettings.SendPort = this.settingsHelper.Settings.UdpSendPort;
            formSettings.ReceivePort = this.settingsHelper.Settings.UdpReceivePort;
            formSettings.ComPort = this.settingsHelper.Settings.ComPort;
            formSettings.BaudRate = this.settingsHelper.Settings.BaudRate;
            if (formSettings.ShowDialog(this) == DialogResult.OK)
            {
                this.settingsHelper.Settings.RoboHeadAddress = formSettings.IpAddress;
                this.settingsHelper.Settings.UdpSendPort = formSettings.SendPort;
                this.settingsHelper.Settings.UdpReceivePort = formSettings.ReceivePort;
                this.settingsHelper.Settings.ComPort = formSettings.ComPort;
                this.settingsHelper.Settings.BaudRate = formSettings.BaudRate;
                this.InitializeCommunication();
                this.settingsHelper.Save();
            }
        }
    }
}
