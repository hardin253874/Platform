// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EDC.Syslog
{
    /// <summary>
    ///     Represents a syslog message serializer.
    /// </summary>
    public class SyslogMessageSerializer : ISyslogMessageSerializer
    {
        /// <summary>
        /// Non visible white space
        /// </summary>
        private readonly Regex _nonVisibleWhiteSpaceRegEx = new Regex("[\f\t\v]", RegexOptions.Multiline);


        /// <summary>
        /// New line
        /// </summary>
        private readonly Regex _newLineRegex = new Regex("\n|\r\n", RegexOptions.Multiline);
        

        /// <summary>
        /// The message separator.
        /// </summary>
        private const string MessageSeparator = "\n";


        /// <summary>
        ///     The nil value.
        ///     See NILVALUE in https://tools.ietf.org/html/rfc5424#section-6
        /// </summary>
        private const string NilValue = "-";


        /// <summary>
        ///     The timestamp format.
        ///     See TIMESTAMP in https://tools.ietf.org/html/rfc5424#section-6
        /// </summary>
        private const string TimestampFormat = "yyyy-MM-ddTHH:mm:ss.ffffffK";


        /// <summary>
        ///     The host name maximum length.
        ///     See HOST_NAME in https://tools.ietf.org/html/rfc5424#section-6
        /// </summary>
        private const int HostNameMaxLength = 255;


        /// <summary>
        ///     The application name maximum length.
        ///     See APP_NAME in https://tools.ietf.org/html/rfc5424#section-6
        /// </summary>
        private const int AppNameMaxLength = 48;


        /// <summary>
        ///     The proc identifier maximum length.
        ///     See PROCID in https://tools.ietf.org/html/rfc5424#section-6
        /// </summary>
        private const int ProcIdMaxLength = 128;


        /// <summary>
        ///     The msg identifier maximum length.
        ///     See MSGID in https://tools.ietf.org/html/rfc5424#section-6
        /// </summary>
        private const int MsgIdMaxLength = 32;


        /// <summary>
        ///     The sd identifier maximum length.
        ///     See SD-ID in https://tools.ietf.org/html/rfc5424#section-6
        /// </summary>
        private const int SdIdMaxLength = 32;


        /// <summary>
        ///     The param name maximum length.
        ///     See PARAM-NAME in https://tools.ietf.org/html/rfc5424#section-6
        /// </summary>
        private const int ParamNameMaxLength = 32;


        #region ISyslogMessageSerializer Members


        /// <summary>
        ///     Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="stream">The stream.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     message
        ///     or
        ///     stream
        /// </exception>
        public void Serialize(SyslogMessage message, Stream stream)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var messageBuilder = new StringBuilder();

            WriteHeader(message, messageBuilder);
            WriteStructuredData(message, messageBuilder);
            WriteMessage(message, messageBuilder);

            messageBuilder.Append(MessageSeparator);

            WriteToStream(messageBuilder, stream);
        }


        #endregion


        /// <summary>
        ///     Writes the message to the stream.
        /// </summary>
        /// <param name="messageBuilder">The message builder.</param>
        /// <param name="stream">The stream.</param>
        private void WriteToStream(StringBuilder messageBuilder, Stream stream)
        {
            byte[] encodedBytes = Encoding.UTF8.GetBytes(messageBuilder.ToString());
            stream.Write(encodedBytes, 0, encodedBytes.Length);
        }


        /// <summary>
        ///     Sanitizes the syslog value so that it only contains valid characters.
        ///     See https://tools.ietf.org/html/rfc5424#section-6
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns></returns>
        private string SanitizeSyslogValue(string value, int maxLength)
        {
            return SanitizeSyslogValue(value, maxLength, new HashSet<byte>());
        }


        /// <summary>
        ///     Sanitizes the syslog value so that it only contains valid characters.
        ///     See https://tools.ietf.org/html/rfc5424#section-6
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <param name="invalidBytes">The invalid bytes.</param>
        /// <returns></returns>
        private string SanitizeSyslogValue(string value, int maxLength, ISet<byte> invalidBytes)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return NilValue;
            }

            byte[] validBytes = Encoding.UTF8.GetBytes(value).Where(b => b >= 33 && b <= 126 && !invalidBytes.Contains(b)).ToArray();

            string valueString = Encoding.UTF8.GetString(validBytes);

            if (valueString.Length > maxLength)
            {
                valueString = valueString.Substring(0, maxLength);
            }

            return valueString;
        }


        /// <summary>
        ///     Writes the header.
        ///     See https://tools.ietf.org/html/rfc5424#section-6
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageBuilder">The message builder.</param>
        private void WriteHeader(SyslogMessage message, StringBuilder messageBuilder)
        {
            messageBuilder.AppendFormat(@"<{0}>{1} {2} {3} {4} {5} {6} ",
                message.Priority,
                message.Version,
                message.Timestamp?.UtcDateTime.ToString(TimestampFormat) ?? NilValue,
                SanitizeSyslogValue(message.HostName, HostNameMaxLength),
                SanitizeSyslogValue(message.AppName, AppNameMaxLength),
                SanitizeSyslogValue(message.ProcId, ProcIdMaxLength),
                SanitizeSyslogValue(message.MsgId, MsgIdMaxLength));
        }


        /// <summary>
        ///     Escapes the structured data parameter value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private string EscapeStructuredDataParameterValue(string value)
        {
            // Escape as per http://tools.ietf.org/html/rfc5424#section-6.3.3
            var escapedValue = new StringBuilder();

            foreach (char ch in value)
            {
                switch (ch)
                {
                    case ']':
                    case '\\':
                    case '"':
                        escapedValue.Append('\\');
                        break;
                }
                escapedValue.Append(ch);
            }

            return escapedValue.ToString();
        }


        /// <summary>
        ///     Writes the structured data.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageBuilder">The message builder.</param>
        private void WriteStructuredData(SyslogMessage message, StringBuilder messageBuilder)
        {
            if (message.StructuredDataElements.Count <= 0 || message.StructuredDataElements.All(sd => sd.Parameters.All(p => string.IsNullOrWhiteSpace(p.Value))))
            {
                messageBuilder.Append(NilValue);
            }
            else
            {
                // See SD-NAME in http://tools.ietf.org/html/rfc5424#section-6
                var sdIdInvalidBytes = new HashSet<byte>
                {
                    0x5D, // ]
                    0x22, // "
                    0x3D // =
                };

                foreach (SyslogSdElement sd in message.StructuredDataElements.Where(sd => sd.Parameters.Any(p => !string.IsNullOrWhiteSpace(p.Value))))
                {
                    messageBuilder.Append("[");
                    messageBuilder.Append(SanitizeSyslogValue(sd.SdId, SdIdMaxLength, sdIdInvalidBytes));

                    foreach (SyslogSdParameter parameter in sd.Parameters.Where(p => !string.IsNullOrWhiteSpace(p.Value)))
                    {
                        messageBuilder.AppendFormat(" {0}=\"{1}\"", SanitizeSyslogValue(parameter.Name, ParamNameMaxLength, sdIdInvalidBytes), EscapeStructuredDataParameterValue(parameter.Value));
                    }

                    messageBuilder.Append("]");
                }
            }
        }


        /// <summary>
        ///     Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageBuilder">The message builder.</param>
        private void WriteMessage(SyslogMessage message, StringBuilder messageBuilder)
        {
            if (string.IsNullOrWhiteSpace(message.Message))
            {
                return;
            }
            
            messageBuilder.Append(" ");

            // Remove all non visible white space
            var serializedMsg = _nonVisibleWhiteSpaceRegEx.Replace(message.Message, "");
            // Convert new lines to spaces to maintain readability of message
            serializedMsg = _newLineRegex.Replace(serializedMsg, " ");
            
            messageBuilder.Append(serializedMsg);
        }
    }
}