// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VideoHelper.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2012
// </copyright>
// <summary>
//   Класс для приёма и воспроизведения видео.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Microsoft.Xna.Framework.Graphics;

    using MjpegProcessor;
    using RoboCommon;

    /// <summary>
    /// Класс для приёма и воспроизведения видео.
    /// </summary>
    public sealed class VideoHelper
    {
        /// <summary>
        /// Декодер MJPEG.
        /// Используется для воспроизведения потокового видео (точнее, MJPEG), полученного от IP Webcam.
        /// </summary>
        /// <remarks>
        /// Требует подключения .NET библиотеки "MjpegProcessorXna4".
        /// </remarks>
        private MjpegDecoder mjpeg = new MjpegDecoder();

        /// <summary>
        /// Объект, упращающий взаимодействие с роботом.
        /// </summary>
        private UdpCommunicationHelper communicationHelper;

        /// <summary>
        /// Опции управления роботом.
        /// </summary>
        private ControlSettings controlSettings;

        /// <summary>
        /// Initializes a new instance of the VideoHelper class.
        /// </summary>
        /// <param name="communicationHelper">
        /// Объект, упращающий взаимодействие с роботом.
        /// </param>
        /// <param name="controlSettings">
        /// Опции управления роботом.
        /// </param>
        public VideoHelper(UdpCommunicationHelper communicationHelper, ControlSettings controlSettings)
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
        /// Инициализация видеотрансляции.
        /// </summary>
        public void InitializeVideo()
        {
            // Запуск воспроизведения видео:
            if (this.controlSettings.PlayVideo)
            {
                try
                {
                    this.mjpeg.StopStream();
                    this.mjpeg.ParseStream(new Uri(string.Format(
                        @"http://{0}:{1}/videofeed",
                        this.communicationHelper.RoboHeadAddress,
                        this.controlSettings.IpWebcamPort)));
                }
                catch (Exception e)
                {
                    this.communicationHelper.LastErrorMessage = e.Message;
                }
            }
        }

        /// <summary>
        /// Остановка видеотрансляции.
        /// </summary>
        public void FinalizeVideo()
        {
            if (this.controlSettings.PlayVideo)
            {
                try
                {
                    this.mjpeg.StopStream();
                }
                catch (Exception e)
                {
                    this.communicationHelper.LastErrorMessage = e.Message;
                }
            }
        }

        /// <summary>
        /// Получение кадра для отображения.
        /// </summary>
        /// <param name="graphicsDevice">Текущее графическое устройство.</param>
        /// <returns>Текстура с кадром.</returns>
        public Texture2D GetVideoTexture(GraphicsDevice graphicsDevice)
        {
            if (this.controlSettings.PlayVideo)
            {
                try
                {
                    return this.mjpeg.GetMjpegFrame(graphicsDevice);
                }
                catch (Exception e)
                {
                    this.communicationHelper.LastErrorMessage = e.Message;
                    return null;
                }
            }

            return null;
        }
    }
}
