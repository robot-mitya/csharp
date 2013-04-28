// <copyright file="MessageHelperTest.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   MessageHelper tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboCommonTest
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    
    using NUnit.Framework;
    
    using RoboCommon;

    /// <summary>
    /// MessageHelper tests.
    /// </summary>
    [TestFixture]
    public sealed class MessageHelperTest
    {
        /// <summary>
        /// CorrectMessage test empty message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(RoboMessageException), ExpectedMessage = MessageHelper.ErrorEmptyMessage)]
        public void TestCorrectEmptyMessage()
        {
            MessageHelper.CorrectMessage(string.Empty);
        }

        /// <summary>
        /// CorrectMessage test big message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(RoboMessageException), ExpectedMessage = MessageHelper.ErrorBadValue)]
        public void TestCorrectBigMessage()
        {
            MessageHelper.CorrectMessage("I12345");
        }

        /// <summary>
        /// CorrectMessage test message with bad identifier.
        /// </summary>
        [Test]
        [ExpectedException(typeof(RoboMessageException), ExpectedMessage = MessageHelper.ErrorBadIdentifier)]
        public void TestCorrectMessageWithBadIdentifier()
        {
            MessageHelper.CorrectMessage("F0000");
        }

        /// <summary>
        /// CorrectMessage test message with bad value.
        /// </summary>
        [Test]
        [ExpectedException(typeof(RoboMessageException), ExpectedMessage = MessageHelper.ErrorBadValue)]
        public void TestCorrectMessageWithBadValue1()
        {
            MessageHelper.CorrectMessage("Ioooo");
        }

        /// <summary>
        /// CorrectMessage test message with bad value.
        /// </summary>
        [Test]
        [ExpectedException(typeof(RoboMessageException), ExpectedMessage = MessageHelper.ErrorBadValue)]
        public void TestCorrectMessageWithBadValue2()
        {
            MessageHelper.CorrectMessage("I00-00");
        }

        /// <summary>
        /// CorrectMessage test message with bad value.
        /// </summary>
        [Test]
        [ExpectedException(typeof(RoboMessageException), ExpectedMessage = MessageHelper.ErrorBadValue)]
        public void TestCorrectMessageWithBadValue3()
        {
            MessageHelper.CorrectMessage("I00+00");
        }

        /// <summary>
        /// Test CorrectMessage with positive hex value.
        /// </summary>
        [Test]
        public void TestCorrectNormalMessageWithPositiveValue()
        {
            string message = MessageHelper.CorrectMessage("I1234");
            Assert.AreEqual("I1234", message);
        }

        /// <summary>
        /// Test CorrectMessage with negative hex value.
        /// </summary>
        [Test]
        public void TestCorrectMessageWithNegativeValue()
        {
            string message = MessageHelper.CorrectMessage("IF84A");
            Assert.AreEqual("IF84A", message);
        }

        /// <summary>
        /// CorrectMessage test. Value should be corrected to upper case.
        /// </summary>
        [Test]
        public void TestCorrectMessageValueCase()
        {
            string message = MessageHelper.CorrectMessage("If84a");
            Assert.AreEqual("IF84A", message);
        }

        /// <summary>
        /// CorrectMessage test with positive signed value.
        /// </summary>
        [Test]
        public void TestCorrectMessageWithPositiveSignedValue()
        {
            string message = MessageHelper.CorrectMessage("I+1234");
            Assert.AreEqual("I1234", message);
        }

        /// <summary>
        /// CorrectMessage test with negative signed value.
        /// </summary>
        [Test]
        public void TestCorrectMessageWithNegativeSignedValue()
        {
            string message = MessageHelper.CorrectMessage("I-1974");
            Assert.AreEqual("IE68C", message);
        }

        /// <summary>
        /// CorrectMessage test with double negative signed value.
        /// </summary>
        [Test]
        public void TestCorrectMessageWithDoubleNegativeSignedValue()
        {
            string message = MessageHelper.CorrectMessage("I-E68C");
            Assert.AreEqual("I1974", message);
        }

        /// <summary>
        /// ExtractIdentifier test with good message.
        /// </summary>
        [Test]
        public void TestExtractIdentifierOnGoodMessage()
        {
            string identifier = MessageHelper.ExtractIdentifier("I1234");
            Assert.AreEqual("I", identifier);
        }

        /// <summary>
        /// ExtractIdentifier test with bad message. The identifier is not valid - it's a hex digit.
        /// </summary>
        [Test]
        [ExpectedException(typeof(RoboMessageException), ExpectedMessage = MessageHelper.ErrorBadIdentifier)]
        public void TestExtractIdentifierOnBadMessage()
        {
            MessageHelper.ExtractIdentifier("F1234");
        }

        /// <summary>
        /// ExtractIdentifier test with empty message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(RoboMessageException), ExpectedMessage = MessageHelper.ErrorEmptyMessage)]
        public void TestExtractIdentifierOnEmptyMessage()
        {
            MessageHelper.ExtractIdentifier(string.Empty);
        }

        /// <summary>
        /// ExtractValueText test with good message.
        /// </summary>
        [Test]
        public void TestExtractValueTextOnGoodMessage()
        {
            string value = MessageHelper.ExtractValueText("I1234");
            Assert.AreEqual("1234", value);
        }

        /// <summary>
        /// ExtractValueText test for good message with negative value.
        /// </summary>
        [Test]
        public void TestExtractNegativeValueText()
        {
            string value = MessageHelper.ExtractValueText("I-1234");
            Assert.AreEqual("-1234", value);
        }

        /// <summary>
        /// ExtractValueText test for good message with empty value.
        /// </summary>
        [Test]
        public void TestExtractValueTextOnShortMessage()
        {
            string value = MessageHelper.ExtractValueText("I");
            Assert.AreEqual(string.Empty, value);
        }

        /// <summary>
        /// ExtractValueText test with empty message.
        /// </summary>
        [Test]
        [ExpectedException(typeof(RoboMessageException), ExpectedMessage = MessageHelper.ErrorEmptyMessage)]
        public void TestExtractValueTextOnEmptyMessage()
        {
            MessageHelper.ExtractValueText(string.Empty);
        }
    }
}
