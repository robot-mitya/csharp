// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleSettingsHelper.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   Class that read and save ConsoleSettings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboConsole
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Reflection;
    using System.Text;
    using System.Xml.Serialization;

    /// <summary>
    /// Class that read and save ConsoleSettings.
    /// </summary>
    internal class ConsoleSettingsHelper
    {
        /// <summary>
        /// The directory where configuration file is located.
        /// </summary>
        private string settingsPath;

        /// <summary>
        /// The name of configuration file.
        /// </summary>
        private string settingsFullFilename;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleSettingsHelper"/> class.
        /// </summary>
        public ConsoleSettingsHelper()
        {
            this.Settings = new ConsoleSettings();
            this.settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                "\\" + this.GetCompanyName().Replace(" ", "_") + "\\" + this.GetProductName().Replace(" ", "_");
            this.settingsFullFilename = this.settingsPath + "\\settings.xml";
        }

        /// <summary>
        /// Gets application settings.
        /// </summary>
        public ConsoleSettings Settings { get; private set; }

        /// <summary>
        /// Gets COM port list.
        /// </summary>
        /// <returns>Enumerable list of COM ports.</returns>
        public static IEnumerable<ComPortItem> GetComPorts()
        {
            var result = new List<ComPortItem>();

            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);

            try
            {
                foreach (ManagementObject item in searcher.Get())
                {
                    string portName = item["DeviceID"].ToString();
                    string portDescription = item["DeviceID"].ToString();

                    // COM port with Arduino is not detected.
                    // portDescription.Contains("Arduino") is not working.
                    // I should find out how to get value "Arduino Uno" from "Описание устройства, предоставленное шиной" parameter.
                    // And where is this parameter?
                    result.Add(new ComPortItem(portName, portDescription.Contains("Arduino")));
                }
            }
            catch (ManagementException)
            {
            }

            return result;
        }

        /// <summary>
        /// Gets COM port baud rates.
        /// </summary>
        /// <returns>Baud rates list.</returns>
        public static IEnumerable<BaudRateItem> GetBaudRates()
        {
            return new List<BaudRateItem>
            {
                new BaudRateItem(300),
                new BaudRateItem(1200),
                new BaudRateItem(2400),
                new BaudRateItem(4800),
                new BaudRateItem(9600),
                new BaudRateItem(14400),
                new BaudRateItem(19200),
                new BaudRateItem(28800),
                new BaudRateItem(38400),
                new BaudRateItem(57600),
                new BaudRateItem(115200)
            };
        }

        /// <summary>
        /// Saves settings to XML file.
        /// </summary>
        public void Save()
        {
            if (!Directory.Exists(this.settingsPath))
            {
                Directory.CreateDirectory(this.settingsPath);
            }

            var serializer = new XmlSerializer(typeof(ConsoleSettings));
            TextWriter writer = new StreamWriter(this.settingsFullFilename);
            try
            {
                serializer.Serialize(writer, this.Settings);
            }
            finally
            {
                writer.Close();
            }
        }

        /// <summary>
        /// Loads settings from XML file.
        /// </summary>
        public void Load()
        {
            if (!File.Exists(this.settingsFullFilename))
            {
                // If configuration file doesn't exists then it is created with default values.
                this.Save();
            }

            var serializer = new XmlSerializer(typeof(ConsoleSettings));
            TextReader reader = new StreamReader(this.settingsFullFilename);
            try
            {
                this.Settings = serializer.Deserialize(reader) as ConsoleSettings;
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Gets name of the company from assembly info.
        /// </summary>
        /// <returns>Company name.</returns>
        private string GetCompanyName()
        {
            string result = string.Empty;

            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    result = ((AssemblyCompanyAttribute)customAttributes[0]).Company;
                }

                if (string.IsNullOrEmpty(result))
                {
                    result = string.Empty;
                }
            }

            return result.Trim();
        }

        /// <summary>
        /// Gets name of the product from assembly info.
        /// </summary>
        /// <returns>Company name.</returns>
        private string GetProductName()
        {
            string result = string.Empty;

            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    result = ((AssemblyProductAttribute)customAttributes[0]).Product;
                }

                if (string.IsNullOrEmpty(result))
                {
                    result = string.Empty;
                }
            }

            return result.Trim();
        }
    }
}
