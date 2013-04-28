// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioHelper.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2012
// </copyright>
// <summary>
//   Класс для приёма и воспроизведения звука.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using AXVLC;
    using RoboCommon;

    /// <summary>
    /// Класс для приёма и воспроизведения звука.
    /// </summary>
    public sealed class AudioHelper
    {
        /// <summary>
        /// Плагин VLC. 
        /// Используется для воспроизведения потокового аудио, полученного от IP Webcam.
        /// </summary>
        /// <remarks>
        /// Объект из ActiveX-библиотеки VLC (www.videolan.org).
        /// Требует установки VLC, регистрации ActiveX-библиотеки axvlc.dll, а затем добавления в ссылки проекта COM-компоненты "VideoLAN VLC ActiveX Plugin" (в обозревателе решений отображается как "AXVLC").
        /// </remarks>
        private AXVLC.VLCPlugin2 audio;

        /// <summary>
        /// Объект, упращающий взаимодействие с роботом.
        /// </summary>
        private UdpCommunicationHelper communicationHelper;

        /// <summary>
        /// Опции управления роботом.
        /// </summary>
        private ControlSettings controlSettings;

        /// <summary>
        /// Initializes a new instance of the AudioHelper class.
        /// </summary>
        /// <param name="communicationHelper">
        /// Объект, упращающий взаимодействие с роботом.
        /// </param>
        /// <param name="controlSettings">
        /// Опции управления роботом.
        /// </param>
        public AudioHelper(UdpCommunicationHelper communicationHelper, ControlSettings controlSettings)
        {
            if (communicationHelper == null)
            {
                throw new ArgumentNullException("communicationHelper");
            }

            if (controlSettings == null)
            {
                throw new ArgumentNullException("controlSettingsHelper");
            }

            this.communicationHelper = communicationHelper;
            this.controlSettings = controlSettings;
        }

        /// <summary>
        /// Инициализация аудиотрансляции.
        /// </summary>
        public void InitializeAudio()
        {
            // Запуск воспроизведения аудио:
            if (this.controlSettings.PlayAudio)
            {
                try
                {
                    if (this.audio == null)
                    {
                        this.audio = new AXVLC.VLCPlugin2Class();
                        //this.audio = new AXVLC.VLCPlugin2();
                    }

                    this.audio.Visible = false;
                    if (this.audio.playlist.items != null)
                    {
                        this.audio.playlist.items.clear();
                    }

                    this.audio.AutoPlay = true;
                    this.audio.Volume = 200;
                    string[] options = new string[] { @":network-caching=20" };
                    this.audio.playlist.add(
                        string.Format(
                            @"http://{0}:{1}/audio.wav",
                            this.communicationHelper.RoboHeadAddress,
                            this.controlSettings.IpWebcamPort),
                        null,
                        options);
                    this.audio.playlist.playItem(0);
                }
                catch (Exception e)
                {
                    this.communicationHelper.LastErrorMessage = e.Message;
                }
            }
        }

        /// <summary>
        /// Остановка звукового потока.
        /// </summary>
        public void FinalizeAudio()
        {
            if (this.controlSettings.PlayAudio)
            {
                try
                {
                    if (this.audio.playlist.items.count > 0)
                    {
                        if (this.audio.playlist.isPlaying)
                        {
                            this.audio.playlist.stop();
                        }
                    }
                }
                catch (Exception e)
                {
                    this.communicationHelper.LastErrorMessage = e.Message;
                }
            }
        }
    } // class
} // namespace
