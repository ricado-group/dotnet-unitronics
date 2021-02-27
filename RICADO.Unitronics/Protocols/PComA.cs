using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Unitronics.Requests;

namespace RICADO.Unitronics.Protocols
{
    internal static class PComA
    {
        #region Commands

        public static class Commands
        {
            public static string GetIdentification = "ID";
            public static string ReadClock = "RC";
            public static string WriteClock = "SC";
        }

        #endregion


        #region Constants

        public const string STXRequest = "/";

        public const ushort MaximumOperandsLength = 255;
        public const ushort CommandLength = 2;
        public const ushort UnitIDLength = 2;
        public const ushort CRCLength = 2;
        public const ushort AddressLength = 4;
        public const ushort ReadCountLength = 2;

        #endregion


        #region Private Fields

        private static readonly Dictionary<OperandType, OperandCommandCode> _operandCommandCodes;

        #endregion


        #region Public Methods

        public static readonly byte[] STXResponse = Encoding.ASCII.GetBytes("/A");

        public static readonly char ETX = '\r';

        public static readonly string ClockRegexPattern = "^(\\d{2})(\\d{2})(\\d{2})(\\d{2})(\\d{2})(\\d{2})(\\d{2})$";

        #endregion


        #region Static Constructor

        static PComA()
        {
            _operandCommandCodes = new Dictionary<OperandType, OperandCommandCode>();

            _operandCommandCodes.Add(OperandType.Input, new OperandCommandCode
            {
                ReadCode = "RE",
            });

            _operandCommandCodes.Add(OperandType.Output, new OperandCommandCode
            {
                ReadCode = "RA",
                WriteCode = "SA",
            });

            _operandCommandCodes.Add(OperandType.InputForce, new OperandCommandCode
            {
                WriteCode = "SD",
            });

            _operandCommandCodes.Add(OperandType.OutputForce, new OperandCommandCode
            {
                WriteCode = "SE",
            });

            _operandCommandCodes.Add(OperandType.MB, new OperandCommandCode
            {
                ReadCode = "RB",
                WriteCode = "SB",
            });

            _operandCommandCodes.Add(OperandType.SB, new OperandCommandCode
            {
                ReadCode = "GS",
                WriteCode = "SS",
            });

            _operandCommandCodes.Add(OperandType.CounterRunBit, new OperandCommandCode
            {
                ReadCode = "RM",
            });

            _operandCommandCodes.Add(OperandType.TimerRunBit, new OperandCommandCode
            {
                ReadCode = "RT",
            });

            _operandCommandCodes.Add(OperandType.MI, new OperandCommandCode
            {
                ReadCode = "RW",
                WriteCode = "SW",
            });

            _operandCommandCodes.Add(OperandType.ML, new OperandCommandCode
            {
                ReadCode = "RNL",
                WriteCode = "SNL",
            });

            _operandCommandCodes.Add(OperandType.DW, new OperandCommandCode
            {
                ReadCode = "RND",
                WriteCode = "SND",
            });

            _operandCommandCodes.Add(OperandType.MF, new OperandCommandCode
            {
                ReadCode = "RNF",
                WriteCode = "SNF",
            });

            _operandCommandCodes.Add(OperandType.SI, new OperandCommandCode
            {
                ReadCode = "GF",
                WriteCode = "SF",
            });

            _operandCommandCodes.Add(OperandType.SL, new OperandCommandCode
            {
                ReadCode = "RNH",
                WriteCode = "SNH",
            });

            _operandCommandCodes.Add(OperandType.SDW, new OperandCommandCode
            {
                ReadCode = "RNJ",
                WriteCode = "SNJ",
            });

            _operandCommandCodes.Add(OperandType.CounterCurrent, new OperandCommandCode
            {
                ReadCode = "GX",
                WriteCode = "SK",
            });

            _operandCommandCodes.Add(OperandType.CounterPreset, new OperandCommandCode
            {
                ReadCode = "GY",
                WriteCode = "SJ",
            });

            _operandCommandCodes.Add(OperandType.TimerCurrent, new OperandCommandCode
            {
                ReadCode = "GT",
                WriteCode = "SNK",
            });

            _operandCommandCodes.Add(OperandType.TimerPreset, new OperandCommandCode
            {
                ReadCode = "GP",
                WriteCode = "SNT",
            });

            _operandCommandCodes.Add(OperandType.XB, new OperandCommandCode
            {
                ReadCode = "RZB",
                WriteCode = "SZB",
            });

            _operandCommandCodes.Add(OperandType.XI, new OperandCommandCode
            {
                ReadCode = "RZI",
                WriteCode = "SZI",
            });

            _operandCommandCodes.Add(OperandType.XL, new OperandCommandCode
            {
                ReadCode = "RZL",
                WriteCode = "SZL",
            });

            _operandCommandCodes.Add(OperandType.XDW, new OperandCommandCode
            {
                ReadCode = "RZD",
                WriteCode = "SZD",
            });
        }

        #endregion


        #region Public Methods

        public static ReadOnlyMemory<byte> BuildGetIdentificationMessage(byte unitId)
        {
            StringBuilder messageBuilder = createMessageBuilder(unitId, Commands.GetIdentification);

            return finalizeMessageBuilder(messageBuilder);
        }

        public static string UnpackGetIdentificationMessage(byte unitId, Memory<byte> message)
        {
            return extractReceivedMessageData(unitId, Commands.GetIdentification, message);
        }

        public static ReadOnlyMemory<byte> BuildReadClockMessage(byte unitId)
        {
            StringBuilder messageBuilder = createMessageBuilder(unitId, Commands.ReadClock);

            return finalizeMessageBuilder(messageBuilder);
        }

        public static DateTime UnpackReadClockMessage(byte unitId, Memory<byte> message)
        {
            string messageString = extractReceivedMessageData(unitId, Commands.ReadClock, message);

            if(messageString.Length < 14)
            {
                throw new PComAException("The Response Data Length of '" + messageString.Length + "' was too short - Expecting a Length of '14'");
            }

            if(Regex.IsMatch(messageString, ClockRegexPattern) == false)
            {
                throw new PComAException("Invalid Clock Response Format");
            }

            string[] splitMessage = Regex.Split(messageString, ClockRegexPattern);

            if(int.TryParse(splitMessage[1], out int second) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Second");
            }

            if (int.TryParse(splitMessage[2], out int minute) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Minute");
            }

            if (int.TryParse(splitMessage[3], out int hour) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Hour");
            }

            if (int.TryParse(splitMessage[4], out int dayOfWeek) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Day of Week");
            }

            if (int.TryParse(splitMessage[5], out int day) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Day");
            }

            if (int.TryParse(splitMessage[6], out int month) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Month");
            }

            if (int.TryParse(splitMessage[7], out int year) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Year");
            }

            return new DateTime(year + 2000, month, day, hour, minute, second);
        }

        public static ReadOnlyMemory<byte> BuildWriteClockMessage(byte unitId, DateTime dateTime)
        {
            StringBuilder messageBuilder = createMessageBuilder(unitId, Commands.WriteClock);

            messageBuilder.Append(dateTime.Second.ToString().PadLeft(2, '0'));
            messageBuilder.Append(dateTime.Minute.ToString().PadLeft(2, '0'));
            messageBuilder.Append(dateTime.Hour.ToString().PadLeft(2, '0'));

            int dayOfWeek = (int)dateTime.DayOfWeek + 1;
            messageBuilder.Append(dayOfWeek.ToString().PadLeft(2, '0'));

            messageBuilder.Append(dateTime.Day.ToString().PadLeft(2, '0'));
            messageBuilder.Append(dateTime.Month.ToString().PadLeft(2, '0'));

            string yearString = dateTime.Year.ToString().PadLeft(2, '0');

            if(yearString.Length > 2)
            {
                yearString = yearString.Substring(2);
            }

            messageBuilder.Append(yearString);

            return finalizeMessageBuilder(messageBuilder);
        }

        public static void ValidateWriteClockMessage(byte unitId, Memory<byte> message)
        {
            extractReceivedMessageData(unitId, Commands.WriteClock, message);
        }

        #endregion


        #region Private Methods

        private static StringBuilder createMessageBuilder(byte unitId, string commandCode)
        {
            StringBuilder messageBuilder = new StringBuilder();

            messageBuilder.Append(unitId.ToString("X").PadLeft(2, '0'));

            messageBuilder.Append(commandCode);

            return messageBuilder;
        }

        private static ReadOnlyMemory<byte> finalizeMessageBuilder(StringBuilder messageBuilder)
        {
            messageBuilder.AppendChecksum();

            messageBuilder.Insert(0, STXRequest);

            messageBuilder.Append(ETX);

            return Encoding.ASCII.GetBytes(messageBuilder.ToString());
        }

        private static string extractReceivedMessageData(byte unitId, string commandCode, Memory<byte> message)
        {
            if (message.Length < STXRequest.Length || message.Slice(0, STXResponse.Length).Span.SequenceEqual(STXResponse) == false)
            {
                throw new PComAException("Invalid or Missing STX");
            }

            if(message.Span[message.Length - 1] != (byte)ETX)
            {
                throw new PComAException("Invalid or Missing ETX");
            }

            if(message.Length < STXRequest.Length + UnitIDLength + CommandLength + CRCLength + 1)
            {
                throw new PComAException("The PComA Response Message length was too short");
            }

            string checksum = Encoding.ASCII.GetString(message.Slice(message.Length - 3, CRCLength).ToArray());

            string messageString = Encoding.ASCII.GetString(message.Slice(STXResponse.Length, message.Length - STXResponse.Length - 3).ToArray());

            if(checksum != messageString.CalculateChecksum())
            {
                throw new PComAException("Checksum Verification Failure");
            }

            string responseUnitIdString = messageString.Substring(0, UnitIDLength);

            if(byte.TryParse(responseUnitIdString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out byte responseUnitId) == false || unitId != responseUnitId)
            {
                throw new PComAException("The Unit ID for the PComA Request '" + unitId.ToString("X").PadLeft(UnitIDLength, '0') + "' did not match the PComA Response '" + responseUnitIdString + "'");
            }

            messageString = messageString.Remove(0, UnitIDLength);

            string responseCommandCode = messageString.Substring(0, CommandLength);

            if (responseCommandCode != commandCode.Substring(0, CommandLength))
            {
                throw new PComAException("The Command Code for the PComA Request '" + commandCode.Substring(0, CommandLength) + "' did not match the PComA Response '" + responseCommandCode + "'");
            }

            messageString = messageString.Remove(0, CommandLength);

            return messageString;
        }

        #endregion


        #region Structs

        private struct OperandCommandCode
        {
            public string ReadCode;
            public string WriteCode;
        }

        #endregion
    }
}
