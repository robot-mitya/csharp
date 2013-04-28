// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MoodHelper.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2011
// </copyright>
// <summary>
//   Вспомогательный класс, предназначенный для управления настроением робота.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using RoboCommon;

    /// <summary>
    /// Настроения робота.
    /// </summary>
    public enum Mood
    { 
        /// <summary>
        /// Нормальное настроение.
        /// </summary>
        Normal,

        /// <summary>
        /// Счастливое настроение.
        /// </summary>
        Happy,

        /// <summary>
        /// Грустное настроение.
        /// </summary>
        Blue,

        /// <summary>
        /// Злое настроение.
        /// </summary>
        Angry,

        /// <summary>
        /// Убитое настроение.
        /// </summary>
        Disaster
    }

    /// <summary>
    /// Вспомогательный класс, предназначенный для управления настроением робота.
    /// </summary>
    public sealed class MoodHelper
    {
        /// <summary>
        /// Объект, упращающий взаимодействие с роботом.
        /// </summary>
        private CommunicationHelper communicationHelper;

        /// <summary>
        /// Опции управления роботом.
        /// </summary>
        private ControlSettings controlSettings;

        /// <summary>
        /// Текущее настроение робота.
        /// </summary>
        private Mood mood = Mood.Normal;

        /// <summary>
        /// Initializes a new instance of the MoodHelper class.
        /// </summary>
        /// <param name="communicationHelper">
        /// Объект для взаимодействия с головой робота.
        /// </param>
        /// <param name="controlSettings">
        /// Опции управления роботом.
        /// </param>
        public MoodHelper(CommunicationHelper communicationHelper, ControlSettings controlSettings)
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
        /// Gets Текущее настроение робота.
        /// </summary>
        public Mood Mood
        {
            get
            {
                return this.mood;
            }
        }

        /// <summary>
        /// Инициализация экземпляра класса для взаимодействия с роботом.
        /// </summary>
        /// <param name="communicationHelper">Уже проинициализированный экземпляр.</param>
        public void Initialize(CommunicationHelper communicationHelper)
        {
            this.communicationHelper = communicationHelper;
        }

        /// <summary>
        /// Установка настроения робота.
        /// </summary>
        /// <param name="mood">
        /// Новое настроение.
        /// </param>
        public void SetMood(Mood mood)
        {
            this.CheckCommunicationHelper();

            string command = this.GenerateMoodCommand(mood);
            this.communicationHelper.SendNonrecurrentMessageToRobot(command, "M0000");
            this.mood = mood;
        }

        /// <summary>
        /// Генерация и передача команды виляния хвостом.
        /// </summary>
        public void WagTail()
        {
            this.communicationHelper.SendNonrecurrentMessageToRobot("t0002", "t0000");
        }

        /// <summary>
        /// Генерация и передача команды кивания головой (ответ "да").
        /// </summary>
        public void ShowYes()
        {
            this.communicationHelper.SendNonrecurrentMessageToRobot("y0002", "y0000");
        }

        /// <summary>
        /// Генерация и передача команды кивания головой (ответ "нет").
        /// </summary>
        public void ShowNo()
        {
            this.communicationHelper.SendNonrecurrentMessageToRobot("n0002", "n0000");
        }

        /// <summary>
        /// Генерация и передача команды очень-очень грустного настроения.
        /// </summary>
        /// <param name="lookHelper">
        /// Экземпляр класса для управления обзором.
        /// </param>
        public void ShowReadyToPlay(LookHelper lookHelper)
        {
            this.communicationHelper.SendNonrecurrentMessageToRobot("M0102", "M0000");
            this.mood = Mood.Normal;

            lookHelper.FixedLook(lookHelper.FixedLookX, this.controlSettings.VerticalReadyToPlayDegree);
        }

        /// <summary>
        /// Генерация и передача команды очень-очень грустного настроения.
        /// </summary>
        /// <param name="lookHelper">
        /// Экземпляр класса для управления обзором.
        /// </param>
        public void ShowDepression(LookHelper lookHelper)
        {
            this.communicationHelper.SendNonrecurrentMessageToRobot("M0103", "M0000");
            this.mood = Mood.Blue;

            lookHelper.FixedLook(lookHelper.FixedLookX, this.controlSettings.VerticalMinimumDegree);
        }

        /// <summary>
        /// Проверка инициализации экземпляра класса для взаимодействия с роботом.
        /// </summary>
        private void CheckCommunicationHelper()
        {
            if (this.communicationHelper == null)
            {
                throw new NullReferenceException("MoodHelper не инициализирован.");
            }
        }

        /// <summary>
        /// Формирование команды для задания настроения роботу.
        /// </summary>
        /// <param name="mood">Новое настроение.</param>
        /// <returns>Текст команды.</returns>
        private string GenerateMoodCommand(Mood mood)
        {   
            switch (mood)
            {
                case Mood.Normal:
                    return "M0001";
                case Mood.Happy:
                    return "M0002";
                case Mood.Blue:
                    return "M0003";
                case Mood.Angry:
                    return "M0004";
                case Mood.Disaster:
                    return "M0005";
                default:
                    return "M0000";
            }
        }
    }
}
