using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RICADO.Unitronics.PComA
{
    internal class ReadOperandsRequest : Request
    {
        #region Constants

        private const byte MaximumOperandsLength = 255;

        #endregion


        #region Private Fields

        private readonly OperandType _type;
        private readonly ushort _startAddress;
        private byte _length;

        #endregion


        #region Public Properties

        public OperandType Type => _type;
        public ushort StartAddress => _startAddress;
        public byte Length
        {
            get
            {
                return _length;
            }
            private set
            {
                _length = value;
            }
        }

        #endregion


        #region Constructor

        protected ReadOperandsRequest(byte unitId, string commandCode, OperandType type, ushort startAddress, byte length) : base(unitId, commandCode)
        {
            _type = type;
            _startAddress = startAddress;
            _length = length;
        }

        #endregion


        #region Public Methods

        public ReadOperandsResponse UnpackResponseMessage(Memory<byte> responseMessage)
        {
            return ReadOperandsResponse.UnpackResponseMessage(this, responseMessage);
        }

        public static ReadOperandsRequest CreateNew(UnitronicsPLC plc, OperandType type, ushort startAddress, byte length)
        {
            if(length > MaximumOperandsLength)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The Number of Operands to Read cannot be greater than the Maximum Value '" + MaximumOperandsLength + "'");
            }
            
            return new ReadOperandsRequest(plc.UnitID, Commands.ReadOperands[type], type, startAddress, length);
        }

        public static HashSet<ReadOperandsRequest> CreateMultiple(UnitronicsPLC plc, Dictionary<OperandType, HashSet<ushort>> operandAddresses)
        {
            HashSet<ReadOperandsRequest> requests = new HashSet<ReadOperandsRequest>();

            foreach (OperandType operand in operandAddresses.Keys)
            {
                byte operandLength = calculateOperandMessageLength(operand);
                int maximumReceiveLength = plc.BufferSize - Response.STX.Length - Response.UnitIDLength - Response.CommandLength - Response.CRCLength - Response.ETXLength;

                ReadOperandsRequest request = null;

                foreach (ushort address in operandAddresses[operand].OrderBy(address => address))
                {
                    if (request != null)
                    {
                        int newLength = address - request.StartAddress + 1;

                        if (newLength * operandLength > maximumReceiveLength || newLength > MaximumOperandsLength)
                        {
                            requests.Add(request);

                            request = CreateNew(plc, operand, address, 1);
                        }
                        else
                        {
                            request.Length = (byte)newLength;
                        }
                    }
                    else
                    {
                        request = CreateNew(plc, operand, address, 1);
                    }
                }

                if (request != null && request.Length > 0)
                {
                    requests.Add(request);
                }
            }

            return requests;
        }

        #endregion


        #region Protected Methods

        protected override void BuildMessageDetail(ref StringBuilder messageBuilder)
        {
            messageBuilder.AppendHexValue(_startAddress);

            messageBuilder.AppendHexValue(_length);
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
