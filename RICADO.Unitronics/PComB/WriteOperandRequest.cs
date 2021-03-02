using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Unitronics.PComB
{
    internal class WriteOperandRequest : Request
    {
        #region Private Fields

        private readonly OperandType _type;
        private readonly ushort _address;
        private readonly object _value;

        #endregion


        #region Public Properties

        public OperandType Type => _type;
        public ushort Address => _address;
        public object Value => _value;

        #endregion


        #region Constructor

        protected WriteOperandRequest(byte unitId, OperandType type, ushort address, object value) : base(unitId, CommandCode.ReadWriteOperands)
        {
            _type = type;
            _address = address;
            _value = value;
        }

        #endregion


        #region Public Methods

        public void ValidateResponseMessage(Memory<byte> responseMessage)
        {
            WriteOperandResponse.ValidateResponseMessage(this, responseMessage);
        }

        public static WriteOperandRequest CreateNew(UnitronicsPLC plc, OperandType type, ushort address, object value)
        {
            return new WriteOperandRequest(plc.UnitID, type, address, value);
        }

        #endregion


        #region Protected Methods

        protected override ICollection<byte> BuildCommandDetails()
        {
            List<byte> commandDetails = new List<byte>(6);

            // Read Operands Count (None)
            commandDetails.AddRange(BitConverter.GetBytes((ushort)0));

            // Write Operands Count
            commandDetails.AddRange(BitConverter.GetBytes((ushort)1));

            // Padding
            commandDetails.Add(0);
            commandDetails.Add(0);

            return commandDetails;
        }

        protected override ICollection<byte> BuildMessageData()
        {
            List<byte> messageData = new List<byte>();

            // Read Blocks Count (None)
            messageData.AddRange(BitConverter.GetBytes((ushort)0));

            // Write Blocks Count
            messageData.AddRange(BitConverter.GetBytes((ushort)1));

            // Operand Type
            messageData.Add(BinaryOperandTypes.ReadWrite[_type]);

            // Operand Count
            messageData.Add(1);

            // Start Address
            messageData.AddRange(BitConverter.GetBytes(_address));

            // Write Value
            switch (_type)
            {
                case OperandType.Output:
                case OperandType.MB:
                case OperandType.SB:
                case OperandType.XB:
                    if (tryGetValue(out bool boolValue))
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
                    if (tryGetValue(out short shortValue))
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
                    if (tryGetValue(out int intValue))
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
                    if (tryGetValue(out int uintValue))
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

                    if (tryGetValue(out floatValue) == false)
                    {
                        floatValue = 0;
                    }

                    byte[] floatBytes = BitConverter.GetBytes(floatValue);

                    Array.Reverse(floatBytes);
                    Array.Reverse(floatBytes, 0, 2);
                    Array.Reverse(floatBytes, 2, 2);

                    messageData.AddRange(floatBytes);
                    break;

                case OperandType.TimerCurrent:
                case OperandType.TimerPreset:
                    uint timerValue = 0;

                    if (_value is TimeSpan timeSpanValue)
                    {
                        timerValue = Convert.ToUInt32(timeSpanValue.TotalMilliseconds);
                    }
                    else if (tryGetValue(out uint uintTimerValue))
                    {
                        timerValue = uintTimerValue;
                    }

                    if (timerValue > UnitronicsPLC.MaximumTimerValue)
                    {
                        timerValue = UnitronicsPLC.MaximumTimerValue;
                    }

                    if (timerValue > 0)
                    {
                        timerValue /= 10;
                    }

                    messageData.AddRange(BitConverter.GetBytes(timerValue));
                    break;
            }

            return messageData;
        }

        #endregion


        #region Private Methods

        private bool tryGetValue<T>(out T value)
        {
            value = default(T);

            if (_value == null)
            {
                return false;
            }

            Type objectType = _value.GetType();

            Type valueType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (objectType == valueType || objectType.IsSubclassOf(valueType))
            {
                value = (T)_value;
                return true;
            }

            try
            {
                value = (T)Convert.ChangeType(_value, valueType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

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
    }
}
