using System;
using System.Text;

namespace RICADO.Unitronics.PComA
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

        protected WriteOperandRequest(byte unitId, string commandCode, OperandType type, ushort address, object value) : base(unitId, commandCode)
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
            return new WriteOperandRequest(plc.UnitID, Commands.WriteOperands[type], type, address, value);
        }

        #endregion


        #region Protected Methods

        protected override void BuildMessageDetail(ref StringBuilder messageBuilder)
        {
            // Start Address
            messageBuilder.AppendHexValue(_address);

            // Operand Count
            messageBuilder.AppendHexValue((byte)1);

            switch (_type)
            {
                case OperandType.Output:
                case OperandType.MB:
                case OperandType.SB:
                case OperandType.XB:
                    if (tryGetValue(out bool boolValue))
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
                    if (tryGetValue(out short shortValue))
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
                    if (tryGetValue(out int intValue))
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
                    if (tryGetValue(out int uintValue))
                    {
                        messageBuilder.AppendHexValue(uintValue);
                    }
                    else
                    {
                        messageBuilder.AppendHexValue((uint)0);
                    }
                    break;

                case OperandType.MF:
                    if (tryGetValue(out float floatValue))
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

                    messageBuilder.AppendHexValue(timerValue);
                    break;
            }
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
    }
}
