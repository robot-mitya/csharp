// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DriveHelper.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2011
// </copyright>
// <summary>
//   Вспомогательный класс, предназначенный для организации работы с ходовыми двигателями робота.
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
    /// Вспомогательный класс, предназначенный для организации работы с ходовыми двигателями робота.
    /// </summary>
    public class DriveHelper
    {
        /// <summary>
        /// Объект, упращающий взаимодействие с роботом.
        /// </summary>
        private ICommunicationHelper communicationHelper;

        /// <summary>
        /// Опции управления роботом.
        /// </summary>
        private ControlSettings controlSettings;

        /// <summary>
        /// Признак турбо-режима робота.
        /// </summary>
        /// <remarks>
        /// Питание двигателей осуществляется от 12 В, это предельное их напряжение, поэтому введено 
        /// программное ограничение напряжения, подаваемого на двигатели. Только в турбо-режиме на 
        /// двигатели может быть подано полное напряжение.
        /// </remarks>
        private bool turboModeOn = false;

        /// <summary>
        /// Текущая установленная скорость при управлении роботом от клавиатуры.
        /// </summary>
        private byte speedForKeyboardControl;

        /// <summary>
        /// Признак режима разворота.
        /// </summary>
        /// <remarks>
        /// В режиме разворота боковые повороты джойстика движения приводят к вращению левых и правых 
        /// колёс в разные стороны. В обычном режиме одна из сторон только замедляемся (вплоть до остановки).
        /// </remarks>
        private bool rotationModeOn = false;

        /// <summary>
        /// Последняя команда, переданная на левый мотор.
        /// </summary>
        private string leftMotorCommand = string.Empty;

        /// <summary>
        /// Последняя команда, переданная на левый мотор.
        /// </summary>
        private string rightMotorCommand = string.Empty;

        /// <summary>
        /// Initializes a new instance of the DriveHelper class.
        /// </summary>
        /// <param name="communicationHelper">
        /// Объект для взаимодействия с головой робота.
        /// </param>
        /// <param name="controlSettings">
        /// Опции управления роботом.
        /// </param>
        public DriveHelper(ICommunicationHelper communicationHelper, ControlSettings controlSettings)
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

            this.speedForKeyboardControl = controlSettings.Speed3;
        }

        /// <summary>
        /// Gets or sets a value indicating whether турбо-режим включен.
        /// </summary>
        public bool TurboModeOn
        {
            get
            {
                return this.turboModeOn;
            }

            set
            {
                this.turboModeOn = value;
            }
        }

        /// <summary>
        /// Gets or sets текущую установленную скорость при управлении роботом от клавиатуры.
        /// </summary>
        public byte SpeedForKeyboardControl
        {
            get
            {
                return this.speedForKeyboardControl;
            }

            set
            {
                this.speedForKeyboardControl = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether режим разворота включен. Только для управления джойстиком.
        /// </summary>
        public bool RotationModeOn
        {
            get
            {
                return this.rotationModeOn;
            }

            set
            {
                this.rotationModeOn = value;
            }
        }

        /// <summary>
        /// Gets Последняя команда, переданная на левый мотор.
        /// </summary>
        public string LeftMotorCommand 
        { 
            get 
            { 
                return this.leftMotorCommand; 
            } 
        }

        /// <summary>
        /// Gets Последняя команда, переданная на правый мотор.
        /// </summary>
        public string RightMotorCommand 
        { 
            get 
            {
                return this.rightMotorCommand; 
            } 
        }

        /// <summary>
        /// Обеспечивает нелинейный прирост скорости. Функция - дуга окружности. f(x) = sqrt(2x - x^2)
        /// </summary>
        /// <param name="speed">Исходная скорость.</param>
        /// <returns>Целевая скорость после подставления в функцию.</returns>
        public static int NonlinearSpeedCorrection(int speed)
        {            
            double floatSpeed = Math.Abs(speed);
            floatSpeed = floatSpeed / 255.0;
            double floatResult = Math.Sqrt((2 * floatSpeed) - (floatSpeed * floatSpeed));
            floatResult = floatResult * 255.0;
            int result = (int)Math.Round(floatResult);
            result = result > 255 ? 255 : result;
            result = result < 0 ? 0 : result;
            if (speed < 0)
            {
                result = -result;
            }

            return result;
        }

        /// <summary>
        /// Расчёт скоростей двигателей, исходя из значений координат джойстика движения.
        /// </summary>
        /// <param name="x">Координата x джойстика в интервале [-1, 1].</param>
        /// <param name="y">Координата y джойстика в интервале [-1, 1].</param>
        /// <param name="leftSpeed">Возвращаемая скорость левого двигателя.</param>
        /// <param name="rightSpeed">Возвращаемая скорость правого двигателя.</param>
        public void CalculateMotorsSpeed(double x, double y, out int leftSpeed, out int rightSpeed)
        {
            double vectorLength = Math.Sqrt((x * x) + (y * y));
            vectorLength = vectorLength < 0 ? 0 : vectorLength;
            vectorLength = vectorLength > 1 ? 1 : vectorLength;

            double sinAlpha = vectorLength == 0 ? 0 : (y >= 0 ? y / vectorLength : -y / vectorLength);

            // Аж два раза корректирую синус. Дошёл до этого эмпирически. Только так чувствуется эффект.
            sinAlpha = 1 - Math.Sqrt(1 - (sinAlpha * sinAlpha)); // (нелинейная корректировка синуса) f(x) = 1 - sqrt(1 - x^2)
            sinAlpha = 1 - Math.Sqrt(1 - (sinAlpha * sinAlpha)); // (нелинейная корректировка синуса) f(x) = 1 - sqrt(1 - x^2)
            sinAlpha = sinAlpha < 0 ? 0 : sinAlpha;
            sinAlpha = sinAlpha > 1 ? 1 : sinAlpha;

            bool smallAlpha = sinAlpha < this.controlSettings.SinAlphaBound;
            bool rotationModeOn = this.rotationModeOn && smallAlpha;

            if ((x >= 0) && (y >= 0))
            {
                leftSpeed = this.Map(vectorLength, 0, 255);
                rightSpeed = this.rotationModeOn ? -leftSpeed : this.Map(sinAlpha * vectorLength, 0, 255);
            }
            else if ((x < 0) && (y >= 0))
            {
                rightSpeed = this.Map(vectorLength, 0, 255);
                leftSpeed = this.rotationModeOn ? -rightSpeed : this.Map(sinAlpha * vectorLength, 0, 255);
            }
            else if ((x < 0) && (y < 0))
            {
                rightSpeed = this.Map(vectorLength, 0, -255);
                leftSpeed = this.rotationModeOn ? -rightSpeed : this.Map(sinAlpha * vectorLength, 0, -255);
            }
            else
            { // (x >= 0) && (y < 0)
                leftSpeed = this.Map(vectorLength, 0, -255);
                rightSpeed = this.rotationModeOn ? -leftSpeed : this.Map(sinAlpha * vectorLength, 0, -255);
            }

            // Делаю нелинейный прирост скорости. Функция - дуга окружности. f(x) = sqrt(2x - x^2)
            leftSpeed = NonlinearSpeedCorrection(leftSpeed);
            rightSpeed = NonlinearSpeedCorrection(rightSpeed);

            this.CorrectMotorsSpeedForTurboMode(ref leftSpeed, ref rightSpeed);
        }

        /// <summary>
        /// Расчёт скоростей двигателей, исходя из состояний клавиш управления движением.
        /// </summary>
        /// <param name="forwardPressed">Нажата клавиша "Вперёд".</param>
        /// <param name="backwardPressed">Нажата клавиша "Назад".</param>
        /// <param name="leftPressed">Нажата клавиша "Влево".</param>
        /// <param name="rightPressed">Нажата клавиша "Вправо".</param>
        /// <param name="leftSpeed">Возвращаемая скорость левого двигателя.</param>
        /// <param name="rightSpeed">Возвращаемая скорость правого двигателя.</param>
        public void CalculateMotorsSpeed(
            bool forwardPressed, 
            bool backwardPressed, 
            bool leftPressed, 
            bool rightPressed,
            out int leftSpeed, 
            out int rightSpeed)
        {
            const int SpeedRetarding = 1000;

            leftSpeed = 0;
            rightSpeed = 0;
            if (forwardPressed)
            {
                if (leftPressed && rightPressed)
                {
                    // Движение вперёд
                    leftSpeed = this.speedForKeyboardControl;
                    rightSpeed = this.speedForKeyboardControl;
                }
                else if (leftPressed)
                {
                    // Поворот налево в движении
                    leftSpeed = this.speedForKeyboardControl / SpeedRetarding;
                    rightSpeed = this.speedForKeyboardControl;
                }
                else if (rightPressed)
                {
                    // Поворот направо в движении
                    leftSpeed = this.speedForKeyboardControl;
                    rightSpeed = this.speedForKeyboardControl / SpeedRetarding;
                }
                else if (backwardPressed)
                {
                    // Остановка
                    leftSpeed = 0;
                    rightSpeed = 0;
                }
                else
                {
                    // Движение вперёд
                    leftSpeed = this.speedForKeyboardControl;
                    rightSpeed = this.speedForKeyboardControl;
                }
            }
            else if (backwardPressed)
            {
                if (leftPressed && rightPressed)
                {
                    // Движение назад
                    leftSpeed = -this.speedForKeyboardControl;
                    rightSpeed = -this.speedForKeyboardControl;
                }
                else if (leftPressed)
                {
                    // Поворот направо в движении
                    leftSpeed = -this.speedForKeyboardControl / SpeedRetarding;
                    rightSpeed = -this.speedForKeyboardControl;
                }
                else if (rightPressed)
                {
                    // Поворот налево в движении
                    leftSpeed = -this.speedForKeyboardControl;
                    rightSpeed = -this.speedForKeyboardControl / SpeedRetarding;
                }
                else if (forwardPressed)
                {
                    // Остановка
                    leftSpeed = 0;
                    rightSpeed = 0;
                }
                else
                {
                    // Движение назад
                    leftSpeed = -this.speedForKeyboardControl;
                    rightSpeed = -this.speedForKeyboardControl;
                }
            }
            else if (leftPressed)
            {
                if (rightPressed)
                {
                    // Остановка
                    leftSpeed = 0;
                    rightSpeed = 0;
                }
                else
                {
                    // Поворот налево на месте
                    leftSpeed = -this.speedForKeyboardControl;
                    rightSpeed = this.speedForKeyboardControl;
                }
            }
            else if (rightPressed)
            {
                if (leftPressed)
                {
                    // Остановка
                    leftSpeed = 0;
                    rightSpeed = 0;
                }
                else
                {
                    // Поворот направо на месте
                    leftSpeed = this.speedForKeyboardControl;
                    rightSpeed = -this.speedForKeyboardControl;
                }
            }

            this.CorrectMotorsSpeedForTurboMode(ref leftSpeed, ref rightSpeed);
        }

        /// <summary>
        /// Формирование команд остановки двигателей.
        /// </summary>
        /// <param name="leftMotorCommand">Команда остановки левых двигателей.</param>
        /// <param name="rightMotorCommand">Команда остановки правых двигателей.</param>
        public void GenerateStopMotorCommands(out string leftMotorCommand, out string rightMotorCommand)
        {
            this.GenerateMotorCommands(0, 0, out leftMotorCommand, out rightMotorCommand);
        }

        /// <summary>
        /// Формирование команд на двигатели исходя из координат джойстика движения.
        /// </summary>
        /// <param name="leftSpeed">Скорость левого мотора в интервале [-255, 255].</param>
        /// <param name="rightSpeed">Скорость правого мотора в интервале [-255, 255].</param>
        /// <param name="leftMotorCommand">Команда на левые двигатели.</param>
        /// <param name="rightMotorCommand">Команда на правые двигатели.</param>
        public void GenerateMotorCommands(int leftSpeed, int rightSpeed, out string leftMotorCommand, out string rightMotorCommand)
        {
            leftMotorCommand = this.SpeedToMotorCommand('L', leftSpeed);
            rightMotorCommand = this.SpeedToMotorCommand('R', rightSpeed);
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
        /// Организует движение робота в соответсятвии с заданными координатами джойстика движения.
        /// </summary>
        /// <param name="x">Координата x джойстика в интервале [-1, 1].</param>
        /// <param name="y">Координата y джойстика в интервале [-1, 1].</param>
        public void Drive(double x, double y)
        {
            this.CheckCommunicationHelper();

            int leftSpeed;
            int rightSpeed;
            this.CalculateMotorsSpeed(x, y, out leftSpeed, out rightSpeed);
            this.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            this.communicationHelper.SendMessageToRobot(this.leftMotorCommand);
            this.communicationHelper.SendMessageToRobot(this.rightMotorCommand);
        }

        /// <summary>
        /// Организует движение робота в соответсятвии с состояниями клавиш клавиатуры.
        /// </summary>
        /// <param name="forwardPressed">Нажата клавиша "Вперёд".</param>
        /// <param name="backwardPressed">Нажата клавиша "Назад".</param>
        /// <param name="leftPressed">Нажата клавиша "Влево".</param>
        /// <param name="rightPressed">Нажата клавиша "Вправо".</param>
        public void Drive(bool forwardPressed, bool backwardPressed, bool leftPressed, bool rightPressed)
        {
            this.CheckCommunicationHelper();

            int leftSpeed;
            int rightSpeed;
            this.CalculateMotorsSpeed(forwardPressed, backwardPressed, leftPressed, rightPressed, out leftSpeed, out rightSpeed);
            this.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            this.communicationHelper.SendMessageToRobot(this.leftMotorCommand);
            this.communicationHelper.SendMessageToRobot(this.rightMotorCommand);
        }

        /// <summary>
        /// Остановка движения робота.
        /// </summary>
        public void Stop()
        {
            this.Drive(0, 0);
        }

        /// <summary>
        /// Переключение турбо-режима.
        /// </summary>
        public void SwitchTurboMode()
        {
            this.turboModeOn = !this.turboModeOn;
        }

        /// <summary>
        /// Проверка инициализации экземпляра класса для взаимодействия с роботом.
        /// </summary>
        private void CheckCommunicationHelper()
        {
            if (this.communicationHelper == null)
            {
                throw new NullReferenceException("DriveHelper не инициализирован.");
            }
        }

        /// <summary>
        /// Масштабирование значения в интервале [0, 1] на заданный целочисленный интервал.
        /// </summary>
        /// <param name="value">Значение в интервале [0, 1].</param>
        /// <param name="minResult">Начало целевого интервала.</param>
        /// <param name="maxResult">Конец целевого интервала.</param>
        /// <returns>Целое значение на целевом интервале.</returns>
        private int Map(double value, int minResult, int maxResult)
        {
            value = value < 0 ? 0 : value;
            value = value > 1 ? 1 : value;
            double result = minResult + (value * (maxResult - minResult));
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Корректировка скорости двигателей.
        /// </summary>
        /// <param name="leftSpeed">Скорость левых двигателей.</param>
        /// <param name="rightSpeed">Скорость правых двигателей.</param>
        private void CorrectMotorsSpeedForTurboMode(ref int leftSpeed, ref int rightSpeed)
        {
            // Линейное снижение скорости (если не включен режим Турбо) - берегу двигатели.
            int coef = this.turboModeOn ? this.controlSettings.DriveModeTurboMaxSpeed : this.controlSettings.DriveModeNormalMaxSpeed;
            leftSpeed = leftSpeed * coef / 255;
            rightSpeed = rightSpeed * coef / 255;
        }

        /// <summary>
        /// Формирование команды роботу.
        /// </summary>
        /// <param name="motor">Определяет для левого ('L') или правого ('R') двигателя формируется команда. Ввводить enum не стал пока.</param>
        /// <param name="signedSpeed">Скорость двигателя со знаком на интервале [0, 255].</param>
        /// <returns>Команда роботу.</returns>
        private string SpeedToMotorCommand(char motor, int signedSpeed)
        {
            string result = motor.ToString();
            result += MessageHelper.IntToMessageValue(signedSpeed);
            return result;
        }
    }
}
