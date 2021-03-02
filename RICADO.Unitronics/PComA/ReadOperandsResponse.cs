using System;
using System.Collections.Generic;
using System.Globalization;

namespace RICADO.Unitronics.PComA
{
    internal class ReadOperandsResponse : Response
    {
        #region Private Fields

        private List<object> _values = new List<object>();

        #endregion


        #region Protected Properties

        protected override ReadOperandsRequest Request => base.Request as ReadOperandsRequest;

        #endregion


        #region Public Properties

        public List<object> Values => _values;

        #endregion


        #region Constructor

        protected ReadOperandsResponse(Request request, Memory<byte> responseMessage) : base(request, responseMessage)
        {
            //_values = new List<object>((request as ReadOperandsRequest).Length);
        }

        #endregion


        #region Public Methods

        public static ReadOperandsResponse UnpackResponseMessage(ReadOperandsRequest request, Memory<byte> responseMessage)
        {
            return new ReadOperandsResponse(request, responseMessage);
        }

        #endregion


        #region Protected Methods

        protected override void UnpackMessageDetail(string messageDetail)
        {
            int operandLength = calculateOperandMessageLength(Request.Type);

            int expectedLength = Request.Length * operandLength;

            if (messageDetail.Length < expectedLength)
            {
                throw new PComAException("The Response Data Length of '" + messageDetail.Length + "' was too short - Expecting a Length of '" + expectedLength + "'");
            }

            if(messageDetail.Length > expectedLength)
            {
                messageDetail = messageDetail.Substring(0, expectedLength);
            }

            while(messageDetail.Length > 0)
            {
                string valueString = messageDetail.Substring(0, operandLength);

                switch (Request.Type)
                {
                    case OperandType.Input:
                    case OperandType.Output:
                    case OperandType.MB:
                    case OperandType.SB:
                    case OperandType.XB:
                        if (byte.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out byte bitValue) == false)
                        {
                            throw new PComAException("Failed to Extract Values from the Response Data for the Operand Type '" + Request.Type + "'");
                        }

                        _values.Add(bitValue > 0 ? true : false);
                        break;

                    case OperandType.MI:
                    case OperandType.SI:
                    case OperandType.XI:
                    case OperandType.CounterCurrent:
                    case OperandType.CounterPreset:
                        if (short.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out short shortValue) == false)
                        {
                            throw new PComAException("Failed to Extract Values from the Response Data for the Operand Type '" + Request.Type + "'");
                        }

                        _values.Add(shortValue);
                        break;

                    case OperandType.ML:
                    case OperandType.SL:
                    case OperandType.XL:
                        if (int.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out int intValue) == false)
                        {
                            throw new PComAException("Failed to Extract Values from the Response Data for the Operand Type '" + Request.Type + "'");
                        }

                        _values.Add(intValue);
                        break;

                    case OperandType.DW:
                    case OperandType.SDW:
                    case OperandType.XDW:
                        if (uint.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out uint uintValue) == false)
                        {
                            throw new PComAException("Failed to Extract Values from the Response Data for the Operand Type '" + Request.Type + "'");
                        }

                        _values.Add(uintValue);
                        break;

                    case OperandType.MF:
                        if (valueString.Length != 8)
                        {
                            throw new PComAException("Failed to Extract Values from the Response Data for the Operand Type '" + Request.Type + "'");
                        }

                        byte[] hexBytes = new byte[4];

                        hexBytes[1] = byte.Parse(valueString.Substring(0, 2), NumberStyles.HexNumber);
                        hexBytes[0] = byte.Parse(valueString.Substring(2, 2), NumberStyles.HexNumber);
                        hexBytes[3] = byte.Parse(valueString.Substring(4, 2), NumberStyles.HexNumber);
                        hexBytes[2] = byte.Parse(valueString.Substring(6, 2), NumberStyles.HexNumber);

                        _values.Add(BitConverter.ToSingle(hexBytes));
                        break;

                    case OperandType.TimerCurrent:
                    case OperandType.TimerPreset:
                        if (uint.TryParse(valueString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out uint timerValue) == false)
                        {
                            throw new PComAException("Failed to Extract Values from the Response Data for the Operand Type '" + Request.Type + "'");
                        }

                        _values.Add(timerValue * 10);
                        break;
                }

                messageDetail = messageDetail.Remove(0, operandLength);
            }
        }

        #endregion


        #region Private Methods

        private static byte calculateOperandMessageLength(OperandType type)
        {
            switch (type)
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
    }
}
