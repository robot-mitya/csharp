// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ControlSettingsHelper.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   Class that read and save ControlSettings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboControl
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Serialization;

    /// <summary>
    /// Class that read and save ControlSettings.
    /// </summary>
    internal class ControlSettingsHelper
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
        /// Initializes a new instance of the <see cref="ControlSettingsHelper"/> class.
        /// </summary>
        public ControlSettingsHelper()
        {
            this.Settings = new ControlSettings();
            this.settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                "\\" + this.GetCompanyName().Replace(" ", "_") + "\\" + this.GetProductName().Replace(" ", "_");
            this.settingsFullFilename = this.settingsPath + "\\settings.xml";
        }

        /// <summary>
        /// Gets application settings.
        /// </summary>
        public ControlSettings Settings { get; private set; }

        /// <summary>
        /// Saves settings to XML file.
        /// </summary>
        public void Save()
        {
            if (!Directory.Exists(this.settingsPath))
            {
                Directory.CreateDirectory(this.settingsPath);
            }

            var serializer = new XmlSerializer(typeof(ControlSettings));
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

            var serializer = new XmlSerializer(typeof(ControlSettings));
            TextReader reader = new StreamReader(this.settingsFullFilename);
            try
            {
                this.Settings = serializer.Deserialize(reader) as ControlSettings;
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Gets version of the product from assembly info.
        /// </summary>
        /// <returns>Product version.</returns>
        public string GetProductVersion()
        {
            string result = string.Empty;

            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
            {
                return string.Empty;
            }

            AssemblyName assemblyName = assembly.GetName();
            if (assemblyName == null)
            {
                return string.Empty;
            }

            Version version = assemblyName.Version;
            if (version == null)
            {
                return string.Empty;
            }

            return version.ToString();
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
        /// <returns>Product name.</returns>
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
