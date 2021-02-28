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


        #region Public Properties

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

        public static ReadOnlyMemory<byte> BuildWriteOperandMessage(byte unitId, OperandType type, ushort address, object value)
        {
            StringBuilder messageBuilder = createMessageBuilder(unitId, _operandCommandCodes[type].WriteCode);

            // Start Address
            messageBuilder.AppendHexValue(address);

            // Operand Count
            messageBuilder.AppendHexValue((byte)1);

            switch(type)
            {
                case OperandType.Output:
                case OperandType.MB:
                case OperandType.SB:
                case OperandType.XB:
                    if(value.TryGetValue(out bool boolValue))
                    {
                        messageBuilder.AppendHexValue(boolValue == true ? 1 : 0, 1);
                    }
                    else
                    {
                        messageBuilder.AppendHexValue(0, 1);
                    }
                    break;

                case OperandType.MI:
                case OperandType.SI:
                case OperandType.XI:
                case OperandType.CounterCurrent:
                case OperandType.CounterPreset:
                    if (value.TryGetValue(out short shortValue))
                    {
                        messageBuilder.AppendHexValue(shortValue);
                    }
                    else
                    {
                        messageBuilder.AppendHexValue((short)0);
                    }
                    break;

                case OperandType.ML:
                case OperandType.SL:
                case OperandType.XL:
                    if (value.TryGetValue(out int intValue))
                    {
                        messageBuilder.AppendHexValue(intValue);
                    }
                    else
                    {
                        messageBuilder.AppendHexValue((int)0);
                    }
                    break;

                case OperandType.DW:
                case OperandType.SDW:
                case OperandType.XDW:
                    if (value.TryGetValue(out int uintValue))
                    {
                        messageBuilder.AppendHexValue(uintValue);
                    }
                    else
                    {
                        messageBuilder.AppendHexValue((uint)0);
                    }
                    break;

                case OperandType.MF:
                    if(value.TryGetValue(out float floatValue))
                    {
                        messageBuilder.AppendHexValue(floatValue);
                    }
                    else
                    {
                        messageBuilder.AppendHexValue((float)0);
                    }
                    break;

                case OperandType.TimerCurrent:
                case OperandType.TimerPreset:
                    uint timerValue = 0;

                    if (value is TimeSpan timeSpanValue)
                    {
                        timerValue = Convert.ToUInt32(timeSpanValue.TotalMilliseconds);
                    }
                    else if (value.TryGetValue(out uint uintTimerValue))
                    {
                        timerValue = uintTimerValue;
                    }

                    if(timerValue > 359999990)
                    {
                        timerValue = 359999990; // Maximum Timer Value - 99 Hours, 59 Minutes, 59 Seconds, 990 Milliseconds
                    }
                    
                    if (timerValue > 0)
                    {
                        timerValue /= 10;
                    }

                    messageBuilder.AppendHexValue(timerValue);
                    break;
            }

            return finalizeMessageBuilder(messageBuilder);
        }

        public static void ValidateWriteOperandMessage(byte unitId, OperandType type, Memory<byte> message)
        {
            extractReceivedMessageData(unitId, _operandCommandCodes[type].WriteCode, message);
        }

        public static HashSet<ReadOperandsMessage> BuildReadOperandsMessages(byte unitId, Dictionary<OperandType, HashSet<ushort>> operandAddresses, ushort bufferSize)
        {
            HashSet<ReadOperandsMessage> messages = new HashSet<ReadOperandsMessage>();
            
            foreach(OperandType operand in operandAddresses.Keys)
            {
                int operandLength = calculateOperandMessageLength(operand);
                int maximumReceiveLength = bufferSize - STXResponse.Length - UnitIDLength - CommandLength - 1;
                
                ReadOperandsMessage readMessage = null;

                foreach(ushort address in operandAddresses[operand].OrderBy(address => address))
                {
                    if(readMessage != null)
                    {
                        int newLength = address - readMessage.StartAddress + 1;
                        
                        if(newLength * operandLength > maximumReceiveLength || newLength > MaximumOperandsLength)
                        {
                            messages.Add(readMessage);

                            readMessage = new ReadOperandsMessage
                            {
                                UnitID = unitId,
                                Type = operand,
                                StartAddress = address,
                                Length = 1,
                            };
                        }
                        else
                        {
                            readMessage.Length = (byte)newLength;
                        }
                    }
                    else
                    {
                        readMessage = new ReadOperandsMessage
                        {
                            UnitID = unitId,
                            Type = operand,
                            StartAddress = address,
                            Length = 1,
                        };
                    }
                }

                if(readMessage != null && readMessage.Length > 0)
                {
                    messages.Add(readMessage);
                }
            }

            return messages;
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

        private static int calculateOperandMessageLength(OperandType type)
        {
            switch(type)
            {
                case OperandType.Input:
                case OperandType.Output:
                case OperandType.MB:
                case OperandType.SB:
                case OperandType.XB:
                case OperandType.CounterRunBit:
                case OperandType.TimerRunBit:
                    return 1;

                case OperandType.MI:
                case OperandType.SI:
                case OperandType.XI:
                case OperandType.CounterCurrent:
                case OperandType.CounterPreset:
                    return 4;

                case OperandType.ML:
                case OperandType.SL:
                case OperandType.XL:
                case OperandType.DW:
                case OperandType.SDW:
                case OperandType.XDW:
                case OperandType.MF:
                case OperandType.TimerCurrent:
                case OperandType.TimerPreset:
                    return 8;
            }

            return 1;
        }

        #endregion


        #region Structs

        private struct OperandCommandCode
        {
            public string ReadCode;
            public string WriteCode;
        }

        #endregion


        #region Classes

        public class ReadOperandsMessage
        {
            public byte UnitID;
            public OperandType Type;
            public ushort StartAddress;
            public byte Length;

            public ReadOnlyMemory<byte> BuildRequestMessage()
            {
                StringBuilder messageBuilder = createMessageBuilder(UnitID, _operandCommandCodes[Type].ReadCode);

                messageBuilder.AppendHexValue(StartAddress);

                messageBuilder.AppendHexValue(Length);

                return finalizeMessageBuilder(messageBuilder);
            }

            public object[] UnpackResponseMessage(Memory<byte> message)
            {
                string messageString = extractReceivedMessageData(UnitID, _operandCommandCodes[Type].ReadCode, message);

                int operandLength = calculateOperandMessageLength(Type);

                int expectedLength = Length * operandLength;

                if (messageString.Length < expectedLength)
                {
                    throw new PComAException("The Response Data Length of '" + messageString.Length + "' was too short - Expecting a Length of '" + expectedLength + "'");
                }

                object[] values = new object[Length];

                for(int i = 0; i < Length; i++)
                {
                    string valueString = messageString.Substring(0, operandLength);

                    switch(Type)
                    {
                        case OperandType.Input:
                        case OperandType.Output:
                        case OperandType.MB:
                        case OperandType.SB:
                        case OperandType.XB:
                            if(byte.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out byte bitValue) == false)
                            {
                                throw new PComAException("Failed to Extract Values from the Response Data for the Operand Type '" + Type + "'");
                            }

                            values[i] = bitValue > 0 ? true : false;
                            break;

                        case OperandType.MI:
                        case OperandType.SI:
                        case OperandType.XI:
                        case OperandType.CounterCurrent:
                        case OperandType.CounterPreset:
                            if(short.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out short shortValue) == false)
                            {
                                throw new PComAException("Failed to Extract Values from the Response Data for the Operand Type '" + Type + "'");
                            }

                            values[i] = shortValue;
                            break;

                        case OperandType.ML:
                        case OperandType.SL:
                        case OperandType.XL:
                            if (int.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out int intValue) == false)
                            {
                                throw new PComAException("Failed to Extract Values from the Response Data for the Operand Type '" + Type + "'");
                            }

                            values[i] = intValue;
                            break;

                        case OperandType.DW:
                        case OperandType.SDW:
                        case OperandType.XDW:
                            if (uint.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out uint uintValue) == false)
                            {
                                throw new PComAException("Failed to Extract Values from the Response Data for the Operand Type '" + Type + "'");
                            }

                            values[i] = uintValue;
                            break;

                        case OperandType.MF:
                            if(valueString.Length != 8)
                            {
                                throw new PComAException("Failed to Extract Values from the Response Data for the Operand Type '" + Type + "'");
                            }

                            byte[] hexBytes = new byte[4];

                            hexBytes[1] = byte.Parse(valueString.Substring(0, 2), NumberStyles.HexNumber);
                            hexBytes[0] = byte.Parse(valueString.Substring(2, 2), NumberStyles.HexNumber);
                            hexBytes[3] = byte.Parse(valueString.Substring(4, 2), NumberStyles.HexNumber);
                            hexBytes[2] = byte.Parse(valueString.Substring(6, 2), NumberStyles.HexNumber);

                            values[i] = BitConverter.ToSingle(hexBytes);
                            break;

                        case OperandType.TimerCurrent:
                        case OperandType.TimerPreset:
                            if (uint.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out uint timerValue) == false)
                            {
                                throw new PComAException("Failed to Extract Values from the Response Data for the Operand Type '" + Type + "'");
                            }

                            values[i] = timerValue * 10;
                            break;
                    }

                    messageString = messageString.Remove(0, operandLength);
                }

                return values;
            }
        }

        #endregion
    }
}
