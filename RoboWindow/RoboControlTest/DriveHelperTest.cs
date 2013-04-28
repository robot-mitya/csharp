// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DriveHelperTest.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2011
// </copyright>
// <summary>
//   Класс, тестирующий класс DriveHelper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboControlTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Moq;
    using NUnit.Framework;

    using RoboCommon;
    using RoboControl;

    /// <summary>
    /// Класс, тестирующий класс DriveHelper.
    /// </summary>
    [TestFixture]
    public sealed class DriveHelperTest
    {
        /// <summary>
        /// Объект для взаимодействия с роботом.
        /// </summary>
        private ICommunicationHelper communicationHelper;

        /// <summary>
        /// Опции управления роботом.
        /// </summary>
        private ControlSettings controlSettings;

        /// <summary>
        /// Текст команды для левого двигателя.
        /// </summary>
        private string leftMotorCommand;

        /// <summary>
        /// Текст команды для правого двигателя.
        /// </summary>
        private string rightMotorCommand;

        /// <summary>
        /// Initializes a new instance of the DriveHelperTest class.
        /// </summary>
        public DriveHelperTest()
        {
            var mock = new Mock<ICommunicationHelper>();
            mock.Setup(x => x.SendMessageToRobot(It.IsAny<string>())).Returns(true);
            this.communicationHelper = mock.Object;

            this.controlSettings = new ControlSettings();

            // Специально для тестов переопределяю максимальные скорости нормального и турбо режимов моторов.
            this.controlSettings.DriveModeNormalMaxSpeed = 190;
            this.controlSettings.DriveModeTurboMaxSpeed = 255;
        }

        /// <summary>
        /// Тест движения вперёд и назад в турбо-режиме.
        /// </summary>
        [Test]
        public void TurboDriveTest()
        {
            var driveHelper = new DriveHelper(this.communicationHelper, this.controlSettings);
            driveHelper.TurboModeOn = true;

            int leftSpeed;
            int rightSpeed;

            driveHelper.CalculateMotorsSpeed(0, 1, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L00FF", this.leftMotorCommand);
            Assert.AreEqual("R00FF", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(0, 0.5, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L00DD", this.leftMotorCommand);
            Assert.AreEqual("R00DD", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(0, -1, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("LFF01", this.leftMotorCommand);
            Assert.AreEqual("RFF01", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(0, -0.5, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("LFF23", this.leftMotorCommand);
            Assert.AreEqual("RFF23", this.rightMotorCommand);
        }

        /// <summary>
        /// Тест движения вперёд и назад без турбо-режима.
        /// </summary>
        [Test]
        public void DriveTest()
        {
            var driveHelper = new DriveHelper(this.communicationHelper, this.controlSettings);
            driveHelper.TurboModeOn = false;

            int leftSpeed;
            int rightSpeed;

            driveHelper.CalculateMotorsSpeed(0, 1, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L00BE", this.leftMotorCommand);
            Assert.AreEqual("R00BE", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(0, 0.5, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L00A4", this.leftMotorCommand);
            Assert.AreEqual("R00A4", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(0, -1, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("LFF42", this.leftMotorCommand);
            Assert.AreEqual("RFF42", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(0, -0.5, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("LFF5C", this.leftMotorCommand);
            Assert.AreEqual("RFF5C", this.rightMotorCommand);
        }

        /// <summary>
        /// Тест остановки движения.
        /// </summary>
        [Test]
        public void StopTest()
        {
            var driveHelper = new DriveHelper(this.communicationHelper, this.controlSettings);
            driveHelper.GenerateMotorCommands(0, 0, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L0000", this.leftMotorCommand);
            Assert.AreEqual("R0000", this.rightMotorCommand);
        }
    
        /// <summary>
        /// Тест движения вперёд.
        /// </summary>
        [Test]
        public void ForwardTest()
        {
            var driveHelper = new DriveHelper(this.communicationHelper, this.controlSettings);
            driveHelper.TurboModeOn = false;

            int leftSpeed;
            int rightSpeed;

            driveHelper.CalculateMotorsSpeed(0, 1, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L" + MessageHelper.IntToMessageValue(this.controlSettings.DriveModeNormalMaxSpeed), this.leftMotorCommand);
            Assert.AreEqual("R" + MessageHelper.IntToMessageValue(this.controlSettings.DriveModeNormalMaxSpeed), this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(0, 0.5, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            int speed = Convert.ToInt32(255.0 * 0.5);
            speed = DriveHelper.NonlinearSpeedCorrection(speed);
            speed = driveHelper.TurboModeOn ? speed : speed * this.controlSettings.DriveModeNormalMaxSpeed / 255;
            Assert.AreEqual("L" + MessageHelper.IntToMessageValue(speed), this.leftMotorCommand);
            Assert.AreEqual("R" + MessageHelper.IntToMessageValue(speed), this.rightMotorCommand);
        }

        /// <summary>
        /// Тест движения назад.
        /// </summary>
        [Test]
        public void BackwardTest()
        {
            var driveHelper = new DriveHelper(this.communicationHelper, this.controlSettings);
            driveHelper.TurboModeOn = false;

            int leftSpeed;
            int rightSpeed;

            driveHelper.CalculateMotorsSpeed(0, -1, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L" + MessageHelper.IntToMessageValue(-this.controlSettings.DriveModeNormalMaxSpeed), this.leftMotorCommand);
            Assert.AreEqual("R" + MessageHelper.IntToMessageValue(-this.controlSettings.DriveModeNormalMaxSpeed), this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(0, -0.5, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            int speed = Convert.ToInt32(255.0 * 0.5);
            speed = DriveHelper.NonlinearSpeedCorrection(speed);
            speed = driveHelper.TurboModeOn ? speed : speed * this.controlSettings.DriveModeNormalMaxSpeed / 255;
            Assert.AreEqual("L" + MessageHelper.IntToMessageValue(-speed), this.leftMotorCommand);
            Assert.AreEqual("R" + MessageHelper.IntToMessageValue(-speed), this.rightMotorCommand);
        }

        /// <summary>
        /// Тест разворота.
        /// </summary>
        [Test]
        public void RotationTest1()
        {
            var driveHelper = new DriveHelper(this.communicationHelper, this.controlSettings);

            driveHelper.TurboModeOn = true;

            int leftSpeed;
            int rightSpeed;

            driveHelper.CalculateMotorsSpeed(1, 0, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L00FF", this.leftMotorCommand);
            Assert.AreEqual("R0000", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(0.5, 0, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L00DD", this.leftMotorCommand);
            Assert.AreEqual("R0000", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(-1, 0, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L0000", this.leftMotorCommand);
            Assert.AreEqual("R00FF", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(-0.5, 0, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L0000", this.leftMotorCommand);
            Assert.AreEqual("R00DD", this.rightMotorCommand);
        }

        /// <summary>
        /// Тест разворота на месте.
        /// </summary>
        [Test]
        public void RotationTest2()
        {
            var driveHelper = new DriveHelper(this.communicationHelper, this.controlSettings);

            driveHelper.TurboModeOn = true;
            driveHelper.RotationModeOn = true;

            int leftSpeed;
            int rightSpeed;

            driveHelper.CalculateMotorsSpeed(1, 0, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L00FF", this.leftMotorCommand);
            Assert.AreEqual("RFF01", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(0.5, 0, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L00DD", this.leftMotorCommand);
            Assert.AreEqual("RFF23", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(-1, 0, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("LFF01", this.leftMotorCommand);
            Assert.AreEqual("R00FF", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(-0.5, 0, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("LFF23", this.leftMotorCommand);
            Assert.AreEqual("R00DD", this.rightMotorCommand);
        }

        /// <summary>
        /// Тест плавного поворота вперёд-влево.
        /// </summary>
        [Test]
        public void HalfLeftForward()
        {
            var driveHelper = new DriveHelper(this.communicationHelper, this.controlSettings);

            driveHelper.TurboModeOn = true;

            int leftSpeed;
            int rightSpeed;

            driveHelper.CalculateMotorsSpeed(-0.7071, 0.7071, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L004A", this.leftMotorCommand);
            Assert.AreEqual("R00FF", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(-0.3536, 0.3536, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L0037", this.leftMotorCommand);
            Assert.AreEqual("R00DD", this.rightMotorCommand);
        }

        /// <summary>
        /// Тест плавного поворота вперёд-вправо.
        /// </summary>
        [Test]
        public void HalfRightForward()
        {
            var driveHelper = new DriveHelper(this.communicationHelper, this.controlSettings);

            driveHelper.TurboModeOn = true;

            int leftSpeed;
            int rightSpeed;

            driveHelper.CalculateMotorsSpeed(0.7071, 0.7071, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L00FF", this.leftMotorCommand);
            Assert.AreEqual("R004A", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(0.3536, 0.3536, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("L00DD", this.leftMotorCommand);
            Assert.AreEqual("R0037", this.rightMotorCommand);
        }

        /// <summary>
        /// Тест плавного поворота назад-влево.
        /// </summary>
        [Test]
        public void HalfLeftBackward()
        {
            var driveHelper = new DriveHelper(this.communicationHelper, this.controlSettings);

            driveHelper.TurboModeOn = true;

            int leftSpeed;
            int rightSpeed;

            driveHelper.CalculateMotorsSpeed(-0.7071, -0.7071, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("LFFB6", this.leftMotorCommand);
            Assert.AreEqual("RFF01", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(-0.3536, -0.3536, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("LFFC9", this.leftMotorCommand);
            Assert.AreEqual("RFF23", this.rightMotorCommand);
        }

        /// <summary>
        /// Тест плавного поворота назад-вправо.
        /// </summary>
        [Test]
        public void HalfRightBackward()
        {
            var driveHelper = new DriveHelper(this.communicationHelper, this.controlSettings);

            driveHelper.TurboModeOn = true;

            int leftSpeed;
            int rightSpeed;

            driveHelper.CalculateMotorsSpeed(0.7071, -0.7071, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("LFF01", this.leftMotorCommand);
            Assert.AreEqual("RFFB6", this.rightMotorCommand);

            driveHelper.CalculateMotorsSpeed(0.3536, -0.3536, out leftSpeed, out rightSpeed);
            driveHelper.GenerateMotorCommands(leftSpeed, rightSpeed, out this.leftMotorCommand, out this.rightMotorCommand);
            Assert.AreEqual("LFF23", this.leftMotorCommand);
            Assert.AreEqual("RFFC9", this.rightMotorCommand);
        }
    }
}
