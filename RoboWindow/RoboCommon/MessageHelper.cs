// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageHelper.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2011
// </copyright>
// <summary>
//   Вспомогательный класс, предназначенный для формирования сообщений, передаваемых роботу.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboCommon
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Вспомогательный класс, предназначенный для формирования сообщений, передаваемых роботу.
    /// </summary>
    public static class MessageHelper
    {
        /// <summary>
        /// Identifier length constant.
        /// </summary>
        public const int IdentifierLength = 1;

        /// <summary>
        /// Value length constant.
        /// </summary>
        public const int ValueLength = 4;

        /// <summary>
        /// Error text when the parsing message is empty.
        /// </summary>
        public const string ErrorEmptyMessage = "Отсутствует сообщение.";

        /// <summary>
        /// Error text when message identifier is not valid.
        /// </summary>
        public const string ErrorBadIdentifier = "Неверный идентификатор сообщения.";

        /// <summary>
        /// Error text when message value is impossible to parse.
        /// </summary>
        public const string ErrorBadValue = "Неверно задано значение сообщения.";

        /// <summary>
        /// Преобразование числового значения в строковое представление параметра сообщения.
        /// </summary>
        /// <param name="value">Преобразуемое числовое значение. Допустимые значения от -32 768 до 32 767.</param>
        /// <returns>Строка из четырёх шестнадцатиричных цифр. При необходимо левая часть дополняется символами '0' до достижения дляины строки в четыре символа.</returns>
        public static string IntToMessageValue(int value)
        {
            if ((value < -32768) || (value > 32767))
            {
                throw new ArgumentException("Параметр сообщения должен находиться в интервале от -32 768 до 32 767.");
            }

            short shortValue = Convert.ToInt16(value);
            return shortValue.ToString("X4");
        }

        /// <summary>
        /// Check and parse message.
        /// </summary>
        /// <param name="message">Message can be shortened. Message's value length could be less than 4 symbols. There could be '-' or '+' in the value.</param>
        /// <param name="identifier">Idenfifier extracted from the message.</param>
        /// <param name="value">Value extrected from the message.</param>
        public static void ParseMessage(string message, out string identifier, out short value)
        {
            identifier = MessageHelper.ExtractIdentifier(message);
            string valueText = MessageHelper.ExtractValueText(message).ToUpper();

            if (valueText.Length == 0)
            {
                valueText = "0";
            }

            bool isNegative = false;
            if (valueText[0] == '-')
            {
                isNegative = true;
                valueText = valueText.Remove(0, 1);
            }
            else if (valueText[0] == '+')
            {
                valueText = valueText.Remove(0, 1);
            }

            if (!short.TryParse(
                valueText,
                System.Globalization.NumberStyles.HexNumber,
                CultureInfo.CurrentCulture.NumberFormat,
                out value))
            {
                throw new RoboMessageException(MessageHelper.ErrorBadValue);
            }

            if (isNegative)
            {
                value = (short)(-value);
            }
        }

        /// <summary>
        /// Makes the message from identifier and value.
        /// </summary>
        /// <param name="identifier">Message identifier.</param>
        /// <param name="value">Message value.</param>
        /// <returns>Full message with identifier and 4 hex digits.</returns>
        public static string MakeMessage(string identifier, short value)
        {
            return identifier + value.ToString("X4");
        }

        /// <summary>
        /// Check and correct message. Message will become 5 symbols length (1 for the identifier and 4 for the value). 
        /// The value will be converted in HEX format (symbols 0..9, A..F).
        /// </summary>
        /// <param name="message">Message to be corrected. Message's value length could be less than 4 symbols. There could be '-' or '+' in the value.</param>
        /// <returns>Corrected message. 1 symbol for the identifier and 4 symbols for the value.</returns>
        public static string CorrectMessage(string message)
        {
            string identifier;
            short value;
            ParseMessage(message, out identifier, out value);
            return MakeMessage(identifier, value);
        }

        /// <summary>
        /// Extracts an identifier from the message.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <returns>An identifier.</returns>
        public static string ExtractIdentifier(string message)
        {
            if (message.Length == 0)
            {
                throw new RoboMessageException(MessageHelper.ErrorEmptyMessage);
            }

            char identifier = message.Substring(0, IdentifierLength)[0];

            if (((identifier >= '0') && (identifier <= '9')) ||
                ((identifier >= 'A') && (identifier <= 'F')) ||
                ((identifier >= 'a') && (identifier <= 'f')))
            {
                throw new RoboMessageException(MessageHelper.ErrorBadIdentifier);
            }

            return identifier.ToString();
        }

        /// <summary>
        /// Extracts value from the message. This is just a text value, it is not converted into integer yet.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <returns>Value text.</returns>
        public static string ExtractValueText(string message)
        {
            if (message.Length == 0)
            {
                throw new RoboMessageException(MessageHelper.ErrorEmptyMessage);
            }

            return message.Substring(IdentifierLength, message.Length - IdentifierLength);
        }
    }
}
