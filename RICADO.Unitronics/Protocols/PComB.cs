using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RICADO.Unitronics.Protocols
{
    internal static class PComB
    {
        #region Commands

        public enum CommandCode : byte
        {
            ReadPLCName = 12,
            ReadOperands = 77,
            ReadWriteOperands = 80
        }

        #endregion


        #region Constants

        public const ushort HeaderLength = 24;
        public const ushort FooterLength = 3;

        #endregion


        #region Private Fields

        private static readonly Dictionary<OperandType, OperandBinaryType> _operandBinaryTypes;

        #endregion


        #region Public Properties

        public static readonly byte[] STX = new byte[] { (byte)'/', (byte)'_', (byte)'O', (byte)'P', (byte)'L', (byte)'C' };

        public static readonly byte ETX = (byte)'\\';

        #endregion


        #region Static Constructor

        static PComB()
        {
            _operandBinaryTypes = new Dictionary<OperandType, OperandBinaryType>();

            _operandBinaryTypes.Add(OperandType.Input, new OperandBinaryType
            {
                ReadOnly = 9,
                ReadWrite = 9,
            });

            _operandBinaryTypes.Add(OperandType.Output, new OperandBinaryType
            {
                ReadOnly = 10,
                ReadWrite = 10,
            });

            _operandBinaryTypes.Add(OperandType.MB, new OperandBinaryType
            {
                ReadOnly = 1,
                ReadWrite = 1,
            });

            _operandBinaryTypes.Add(OperandType.SB, new OperandBinaryType
            {
                ReadOnly = 2,
                ReadWrite = 2,
            });

            _operandBinaryTypes.Add(OperandType.CounterRunBit, new OperandBinaryType
            {
                ReadOnly = 12,
                ReadWrite = 12,
            });

            _operandBinaryTypes.Add(OperandType.TimerRunBit, new OperandBinaryType
            {
                ReadOnly = 11,
                ReadWrite = 11,
            });

            _operandBinaryTypes.Add(OperandType.MI, new OperandBinaryType
            {
                ReadOnly = 3,
                ReadWrite = 3,
            });

            _operandBinaryTypes.Add(OperandType.ML, new OperandBinaryType
            {
                ReadOnly = 5,
                ReadWrite = 5,
            });

            _operandBinaryTypes.Add(OperandType.DW, new OperandBinaryType
            {
                ReadOnly = 16,
                ReadWrite = 16,
            });

            _operandBinaryTypes.Add(OperandType.MF, new OperandBinaryType
            {
                ReadOnly = 7,
                ReadWrite = 7,
            });

            _operandBinaryTypes.Add(OperandType.SI, new OperandBinaryType
            {
                ReadOnly = 4,
                ReadWrite = 4,
            });

            _operandBinaryTypes.Add(OperandType.SL, new OperandBinaryType
            {
                ReadOnly = 6,
                ReadWrite = 6,
            });

            _operandBinaryTypes.Add(OperandType.SDW, new OperandBinaryType
            {
                ReadOnly = 17,
                ReadWrite = 36,
            });

            _operandBinaryTypes.Add(OperandType.CounterCurrent, new OperandBinaryType
            {
                ReadOnly = 18,
                ReadWrite = 145,
            });

            _operandBinaryTypes.Add(OperandType.CounterPreset, new OperandBinaryType
            {
                ReadOnly = 19,
                ReadWrite = 144,
            });

            _operandBinaryTypes.Add(OperandType.TimerCurrent, new OperandBinaryType
            {
                ReadOnly = 20,
                ReadWrite = 129,
            });

            _operandBinaryTypes.Add(OperandType.TimerPreset, new OperandBinaryType
            {
                ReadOnly = 21,
                ReadWrite = 128,
            });

            _operandBinaryTypes.Add(OperandType.XB, new OperandBinaryType
            {
                ReadOnly = 26,
                ReadWrite = 64,
            });

            _operandBinaryTypes.Add(OperandType.XI, new OperandBinaryType
            {
                ReadOnly = 27,
                ReadWrite = 65,
            });

            _operandBinaryTypes.Add(OperandType.XL, new OperandBinaryType
            {
                ReadOnly = 28,
                ReadWrite = 66,
            });

            _operandBinaryTypes.Add(OperandType.XDW, new OperandBinaryType
            {
                ReadOnly = 29,
                ReadWrite = 67,
            });
        }

        #endregion


        #region Public Methods

        // ReadWrite Operands Request Structure

        //
        // Command Details
        //

        // Byte 0-1 = Total Operands to Read UInt16
        // Byte 2-3 = Total Operands to Write UInt16

        //
        // Message Data
        //
        // Byte 0-1 = Read Blocks UInt16
        // Byte 2-3 = Write Blocks UInt16
        //
        // Blocks of Read Operand Requests and Write Operand Requests
        //
        // The Read Blocks must be first and the Write Blocks afterwards
        //

        // Read Block Structure
        //
        // Byte 0 = Operand Type
        // Byte 1 = Operand Count (Max 255)
        // Byte 2-3 = Start Address UInt16

        // Write Block Structure
        //
        // Byte 0 = Operand Type
        // Byte 1 = Operand Count (Max 255)
        // Byte 2-3 = Start Address UInt16
        // Byte 4+ = Write Values
        //
        // NOTE: Write Values Length must be divisible by 2 - When dealing with Bit Operands, use Mod 2 to determine if we add an extra 0 byte on the end of the Values Data

        public static ReadOnlyMemory<byte> BuildWriteOperandMessage(byte unitId, OperandType type, ushort address, object value)
        {
            List<byte> commandDetails = new List<byte>(4);

            // Read Operands Count (None)
            commandDetails.AddRange(BitConverter.GetBytes((ushort)0));

            // Write Operands Count
            commandDetails.AddRange(BitConverter.GetBytes((ushort)1));

            List<byte> messageData = new List<byte>();

            // Read Blocks Count (None)
            messageData.AddRange(BitConverter.GetBytes((ushort)0));

            // Write Blocks Count
            messageData.AddRange(BitConverter.GetBytes((ushort)1));

            // Operand Type
            messageData.Add(_operandBinaryTypes[type].ReadWrite);

            // Operand Count
            messageData.Add(1);

            // Start Address
            messageData.AddRange(BitConverter.GetBytes(address));

            // Write Value
            switch (type)
            {
                case OperandType.Output:
                case OperandType.MB:
                case OperandType.SB:
                case OperandType.XB:
                    if (value.TryGetValue(out bool boolValue))
                    {
                        messageData.Add(boolValue == true ? 1 : 0);
                    }
                    else
                    {
                        messageData.Add(0);
                    }

                    // Padding Byte
                    messageData.Add(0);
                    break;

                case OperandType.MI:
                case OperandType.SI:
                case OperandType.XI:
                case OperandType.CounterCurrent:
                case OperandType.CounterPreset:
                    if (value.TryGetValue(out short shortValue))
                    {
                        messageData.AddRange(BitConverter.GetBytes(shortValue));
                    }
                    else
                    {
                        messageData.AddRange(BitConverter.GetBytes((short)0));
                    }
                    break;

                case OperandType.ML:
                case OperandType.SL:
                case OperandType.XL:
                    if (value.TryGetValue(out int intValue))
                    {
                        messageData.AddRange(BitConverter.GetBytes(intValue));
                    }
                    else
                    {
                        messageData.AddRange(BitConverter.GetBytes((int)0));
                    }
                    break;

                case OperandType.DW:
                case OperandType.SDW:
                case OperandType.XDW:
                    if (value.TryGetValue(out int uintValue))
                    {
                        messageData.AddRange(BitConverter.GetBytes(uintValue));
                    }
                    else
                    {
                        messageData.AddRange(BitConverter.GetBytes((uint)0));
                    }
                    break;

                case OperandType.MF:
                    float floatValue;

                    if (value.TryGetValue(out floatValue) == false)
                    {
                        floatValue = 0;
                    }

                    messageData.AddRange(BitConverter.GetBytes(floatValue).Reverse());
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

                    if (timerValue > 359999990)
                    {
                        timerValue = 359999990; // Maximum Timer Value - 99 Hours, 59 Minutes, 59 Seconds, 990 Milliseconds
                    }

                    if (timerValue > 0)
                    {
                        timerValue /= 10;
                    }

                    messageData.AddRange(BitConverter.GetBytes(timerValue));
                    break;
            }

            return buildBinaryMessage(unitId, CommandCode.ReadWriteOperands, commandDetails, messageData);
        }

        public static void ValidateWriteOperandMessage(byte unitId, OperandType type, Memory<byte> message)
        {
            ReadOnlyMemory<byte> messageData = extractReceivedMessageData(unitId, CommandCode.ReadWriteOperands, message);

            if(messageData.Span.SequenceEqual(new byte[4]) == false)
            {
                throw new PComBException("The Write Response Data was Invalid");
            }
        }

        

        #endregion


        #region Private Methods

        private static ReadOnlyMemory<byte> buildBinaryMessage(byte unitId, CommandCode command, ICollection<byte> commandDetails, ICollection<byte> messageData)
        {
            List<byte> message = new List<byte>(HeaderLength + messageData.Count + FooterLength);

            // STX
            message.AddRange(STX);

            // Unit ID
            message.Add(unitId);

            // Reserved
            message.Add(254);
            message.Add(1);
            message.Add(0);
            message.Add(0);
            message.Add(0);

            // Command Code
            message.Add((byte)command);

            // Reserved
            message.Add(0);

            // Command Details
            message.AddRange(commandDetails.Take(commandDetails.Count < 6 ? commandDetails.Count : 6));
            
            while(message.Count < 20)
            {
                message.Add(0);
            }

            // Data Length
            message.AddRange(BitConverter.GetBytes(Convert.ToUInt16(messageData.Count)));

            // Header Checksum
            message.AddRange(BitConverter.GetBytes(message.CalculateChecksum()));

            // Message Data
            message.AddRange(messageData);

            // Message Data Checksum
            message.AddRange(BitConverter.GetBytes(messageData.CalculateChecksum()));

            // ETX
            message.Add(ETX);

            return message.ToArray();
        }

        private static ReadOnlyMemory<byte> extractReceivedMessageData(byte unitId, CommandCode command, Memory<byte> message)
        {
            if (message.Length < STX.Length || message.Slice(0, STX.Length).Span.SequenceEqual(STX) == false)
            {
                throw new PComBException("Invalid or Missing STX");
            }

            if (message.Span[message.Length - 1] != ETX)
            {
                throw new PComBException("Invalid or Missing ETX");
            }

            if (message.Length < HeaderLength + FooterLength)
            {
                throw new PComBException("The PComB Response Message length was too short");
            }

            ushort headerChecksum = BitConverter.ToUInt16(message.Slice(HeaderLength - 2, 2).Span);

            if(headerChecksum != message.Slice(0, HeaderLength - 2).ToArray().CalculateChecksum())
            {
                throw new PComBException("Header Checksum Verification Failure");
            }

            if(message.Span[6] != 254)
            {
                throw new PComBException("Invalid PComB Response Header");
            }

            if(message.Span[7] != unitId)
            {
                throw new PComBException("The Unit ID for the PComB Request '" + unitId + "' did not match the PComB Response '" + message.Span[7] + "'");
            }

            byte responseCommand = message.Span[12];

            if(responseCommand > 0x80)
            {
                responseCommand -= 0x80;
            }

            if(responseCommand != (byte)command)
            {
                throw new PComBException("The Command Code for the PComB Request '" + (byte)command + "' did not match the PComB Response '" + responseCommand + "'");
            }

            ushort messageDataLength = BitConverter.ToUInt16(message.Slice(20, 2).Span);

            if(message.Length < HeaderLength + messageDataLength + FooterLength)
            {
                throw new PComBException("The PComB Response Message length was too short");
            }

            message = message.Slice(HeaderLength, message.Length - HeaderLength);

            ushort dataChecksum = BitConverter.ToUInt16(message.Slice(messageDataLength, 2).Span);

            if(dataChecksum != message.Slice(0, messageDataLength).ToArray().CalculateChecksum())
            {
                throw new PComBException("Message Data Checksum Verification Failure");
            }

            return message.Slice(0, messageDataLength);
        }

        #endregion


        #region Structs

        private struct OperandBinaryType
        {
            public byte ReadOnly;
            public byte ReadWrite;

            public byte ReadOnlyVectorial => (byte)(ReadOnly + 0x80);
        }

        #endregion


        #region Classes

        /*public class LegacyReadOperandsMessage
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

                for (int i = 0; i < Length; i++)
                {
                    string valueString = messageString.Substring(0, operandLength);

                    switch (Type)
                    {
                        case OperandType.Input:
                        case OperandType.Output:
                        case OperandType.MB:
                        case OperandType.SB:
                        case OperandType.XB:
                            if (byte.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out byte bitValue) == false)
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
                            if (short.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out short shortValue) == false)
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
                            if (valueString.Length != 8)
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

                for (int i = 0; i < Length; i++)
                {
                    string valueString = messageString.Substring(0, operandLength);

                    switch (Type)
                    {
                        case OperandType.Input:
                        case OperandType.Output:
                        case OperandType.MB:
                        case OperandType.SB:
                        case OperandType.XB:
                            if (byte.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out byte bitValue) == false)
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
                            if (short.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out short shortValue) == false)
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
                            if (valueString.Length != 8)
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
        }*/

        #endregion
    }
}
