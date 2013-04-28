// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LookHelperTest.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2011
// </copyright>
// <summary>
//   Класс, тестирующий класс LookHelper.
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
    /// Класс, тестирующий класс LookHelper.
    /// </summary>
    [TestFixture]
    public sealed class LookHelperTest
    {
        /// <summary>
        /// Опции управления роботом.
        /// </summary>
        private ControlSettings controlSettings = new ControlSettings();

        /// <summary>
        /// Тест поворота головы вперёд.
        /// </summary>
        [Test]
        public void LookForwardTest()
        {
            LookHelper lookHelper = this.CreateLookHelper();
            lookHelper.Look(0.5f, 0.5f); // (только чтобы сбить начальные координаты, иначе следующая команда не выполнится)
            lookHelper.Look(0, 0);
            Assert.AreEqual(
                "H" + MessageHelper.IntToMessageValue((this.controlSettings.HorizontalMaximumDegree - this.controlSettings.HorizontalMinimumDegree) / 2),
                lookHelper.HorizontalServoCommand);
        }
        
        /// <summary>
        /// Тест поворота головы влево.
        /// </summary>
        [Test]
        public void LookLeftTest()
        {
            LookHelper lookHelper = this.CreateLookHelper();

            lookHelper.Look(-1f, 0);
            Assert.AreEqual(
                "H" + MessageHelper.IntToMessageValue(this.controlSettings.HorizontalMaximumDegree),
                lookHelper.HorizontalServoCommand);

            lookHelper.Look(-0.5f, 0);
            Assert.AreEqual(
                "H" + MessageHelper.IntToMessageValue((this.controlSettings.HorizontalMaximumDegree - this.controlSettings.HorizontalMinimumDegree) * 3 / 4),
                lookHelper.HorizontalServoCommand);
        }

        /// <summary>
        /// Тест поворота головы вправо.
        /// </summary>
        [Test]
        public void LookRightTest()
        {
            LookHelper lookHelper = this.CreateLookHelper();

            lookHelper.Look(1f, 0);
            Assert.AreEqual(
                "H" + MessageHelper.IntToMessageValue(this.controlSettings.HorizontalMinimumDegree),
                lookHelper.HorizontalServoCommand);

            lookHelper.Look(0.5f, 0);
            Assert.AreEqual(
                "H" + MessageHelper.IntToMessageValue((this.controlSettings.HorizontalMaximumDegree - this.controlSettings.HorizontalMinimumDegree) * 1 / 4),
                lookHelper.HorizontalServoCommand);
        }

        /// <summary>
        /// Тест метода LookHelper.CorrectCoordinatesFromCyrcleToSquareArea.
        /// </summary>
        [Test]
        public void CorrectCoordinatesFromCyrcleToSquareAreaTest()
        {
            // Джойстик в центре:
            float x = 0;
            float y = 0;
            LookHelper.CorrectCoordinatesFromCyrcleToSquareArea(ref x, ref y);
            Assert.AreEqual(0, x);
            Assert.AreEqual(0, y);

            // Джойстик на 0 градусов на 100%:
            x = 1;
            y = 0;
            LookHelper.CorrectCoordinatesFromCyrcleToSquareArea(ref x, ref y);
            Assert.AreEqual(1, x);
            Assert.AreEqual(0, y);

            // Джойстик на 90 градусов на 100%:
            x = 0;
            y = 1;
            LookHelper.CorrectCoordinatesFromCyrcleToSquareArea(ref x, ref y);
            Assert.AreEqual(0, x);
            Assert.AreEqual(1, y);

            // Джойстик на 45 градусов на 100%:
            x = (float)Math.Sqrt(2f) / 2f;
            y = x;
            LookHelper.CorrectCoordinatesFromCyrcleToSquareArea(ref x, ref y);
            Assert.AreEqual(1, x);
            Assert.AreEqual(1, y);

            // Джойстик на 45 градусов на 50%:
            x = (float)Math.Sqrt(2f) / 2f / 2;
            y = x;
            LookHelper.CorrectCoordinatesFromCyrcleToSquareArea(ref x, ref y);
            Assert.Greater(0.000001, Math.Abs(0.5f - x));
            Assert.Greater(0.000001, Math.Abs(0.5f - y));

            // Джойстик на 30 градусов на 50%:
            x = (float)Math.Cos(Math.PI / 6) * 0.5f;
            y = (float)Math.Sin(Math.PI / 6) * 0.5f;
            LookHelper.CorrectCoordinatesFromCyrcleToSquareArea(ref x, ref y);
            Assert.Greater(0.000001, Math.Abs(0.5f - x));
            Assert.Greater(0.000001, Math.Abs((0.5f * (float)Math.Tan(Math.PI / 6)) - y));

            // Джойстик на 60 градусов на 50%:
            x = (float)Math.Cos(Math.PI / 3) * 0.5f;
            y = (float)Math.Sin(Math.PI / 3) * 0.5f;
            LookHelper.CorrectCoordinatesFromCyrcleToSquareArea(ref x, ref y);
            Assert.Greater(0.000001, Math.Abs((0.5f * (float)Math.Tan(Math.PI / 6)) - x));
            Assert.Greater(0.000001, Math.Abs(0.5f - y));

            // Джойстик на 135 градусов на 50%:
            x = -(float)Math.Sqrt(2f) / 2f / 2;
            y = Math.Abs(x);
            LookHelper.CorrectCoordinatesFromCyrcleToSquareArea(ref x, ref y);
            Assert.Greater(0.000001, Math.Abs(-0.5f - x));
            Assert.Greater(0.000001, Math.Abs(0.5f - y));

            // Джойстик на 225 градусов на 50%:
            x = -(float)Math.Sqrt(2f) / 2f / 2;
            y = x;
            LookHelper.CorrectCoordinatesFromCyrcleToSquareArea(ref x, ref y);
            Assert.Greater(0.000001, Math.Abs(-0.5f - x));
            Assert.Greater(0.000001, Math.Abs(-0.5f - y));

            // Джойстик на 315 градусов на 50%:
            x = (float)Math.Sqrt(2f) / 2f / 2;
            y = -x;
            LookHelper.CorrectCoordinatesFromCyrcleToSquareArea(ref x, ref y);
            Assert.Greater(0.000001, Math.Abs(0.5f - x));
            Assert.Greater(0.000001, Math.Abs(-0.5f - y));
        }

        /// <summary>
        /// Создание экземпляра класса LookHelper.
        /// </summary>
        /// <returns>
        /// Созданный и инициированный экземпляр.
        /// </returns>
        private LookHelper CreateLookHelper()
        {
            var mock = new Mock<ICommunicationHelper>();
            mock.Setup(x => x.SendMessageToRobot(It.IsAny<string>())).Returns(true);

            var result = new LookHelper(mock.Object, this.controlSettings);
            return result;
        }
    }
}
