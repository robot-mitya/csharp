// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FormSettings.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   Application settings form.
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
    using System.Net;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// Application settings form.
    /// </summary>
    public partial class FormSettings : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormSettings" /> class.
        /// </summary>
        public FormSettings()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets IP address of the robot.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets port to send commands to robot.
        /// </summary>
        public ushort SendPort { get; set; }

        /// <summary>
        /// Gets or sets port to receive commands from robot.
        /// </summary>
        public ushort ReceivePort { get; set; }

        /// <summary>
        /// Gets or sets the COM port name.
        /// </summary>
        public string ComPort { get; set; }

        /// <summary>
        /// Gets or sets the COM port baud rate.
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// OK button handler.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonOkClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.None;
            
            IPAddress address;
            if (IPAddress.TryParse(this.textBoxIP.Text, out address))
            {
                this.IpAddress = this.textBoxIP.Text;
            }
            else
            {
                MessageBox.Show(
                    "Неверно указан IP-адрес телефона", 
                    Application.ProductName, 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.textBoxIP.SelectAll();
                this.textBoxIP.Focus();
                return;
            }

            ushort sendPort;
            if (ushort.TryParse(this.textBoxSendPort.Text, out sendPort))
            {
                this.SendPort = sendPort;
            }
            else
            {
                MessageBox.Show(
                    "Неверно указан порт передачи данных", 
                    Application.ProductName, 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.textBoxSendPort.SelectAll();
                this.textBoxSendPort.Focus();
                return;
            }

            ushort receivePort;
            if (ushort.TryParse(this.textBoxReceivePort.Text, out receivePort))
            {
                this.ReceivePort = receivePort;
            }
            else
            {
                MessageBox.Show(
                    "Неверно указан порт приёма данных", 
                    Application.ProductName, 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.textBoxReceivePort.SelectAll();
                this.textBoxReceivePort.Focus();
                return;
            }

            var comPortItem = (ComPortItem)this.comboBoxComPort.SelectedItem;
            this.ComPort = comPortItem == null ? this.comboBoxComPort.Text : comPortItem.Name;

            var baudRateItem = (BaudRateItem)this.comboBoxBaudRate.SelectedItem;
            this.BaudRate = baudRateItem == null ? 9600 : baudRateItem.BaudRate;

            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Form load handler.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void FormSettingsLoad(object sender, EventArgs e)
        {
            this.textBoxIP.Text = this.IpAddress;
            this.textBoxSendPort.Text = this.SendPort.ToString();
            this.textBoxReceivePort.Text = this.ReceivePort.ToString();

            this.comboBoxComPort.Text = this.ComPort;
            IEnumerable<ComPortItem> comPorts = ConsoleSettingsHelper.GetComPorts();
            this.comboBoxComPort.Items.Clear();
            foreach (var port in comPorts)
            {
                this.comboBoxComPort.Items.Add(port);
                if (port.Name == this.ComPort)
                {
                    this.comboBoxComPort.Text = port.ToString();
                }
            }

            this.comboBoxComPort.Text = this.ComPort;

            IEnumerable<BaudRateItem> baudRates = ConsoleSettingsHelper.GetBaudRates();
            this.comboBoxBaudRate.Items.Clear();
            foreach (var baudRate in baudRates)
            {
                this.comboBoxBaudRate.Items.Add(baudRate);
                if (baudRate.BaudRate == this.BaudRate)
                {
                    this.comboBoxBaudRate.Text = baudRate.ToString();
                }
            }
        }
    }
}
