using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RICADO.Unitronics.PComB
{
    internal class ReadOperandsResponse : Response
    {
        #region Private Fields

        private Dictionary<OperandType, Dictionary<ushort, object>> _operandAddressValues = new Dictionary<OperandType, Dictionary<ushort, object>>();

        #endregion


        #region Protected Properties

        protected override ReadOperandsRequest Request => base.Request as ReadOperandsRequest;

        #endregion


        #region Public Properties

        public Dictionary<OperandType, Dictionary<ushort, object>> OperandAddressValues => _operandAddressValues;

        #endregion


        #region Constructor

        protected ReadOperandsResponse(Request request, Memory<byte> responseMessage, bool disableChecksum = false) : base(request, responseMessage, disableChecksum)
        {
        }

        #endregion


        #region Public Methods

        public static ReadOperandsResponse UnpackResponseMessage(ReadOperandsRequest request, Memory<byte> responseMessage, bool disableChecksum = false)
        {
            return new ReadOperandsResponse(request, responseMessage, disableChecksum);
        }

        #endregion


        #region Protected Methods

        protected override void UnpackMessageData(Memory<byte> messageData)
        {
            int index = 0;
            
            foreach(ReadOperandsRequest.IOperandRequest request in Request.OperandRequests.Values.Where(request => request.Count > 0).OrderBy(request => (ushort)request.Type))
            {
                if(_operandAddressValues.ContainsKey(request.Type) == false)
                {
                    _operandAddressValues.Add(request.Type, new Dictionary<ushort, object>());
                }
                
                if(request is ReadOperandsRequest.NonVectorialOperandRequest nonVectorialRequest)
                {
                    index += index % 2;

                    byte operandByteLength = calculateOperandByteLength(request.Type);

                    foreach(ushort address in nonVectorialRequest.Addresses.OrderBy(address => address))
                    {
                        switch(request.Type)
                        {
                            case OperandType.MI:
                            case OperandType.SI:
                            case OperandType.XI:
                            case OperandType.CounterCurrent:
                            case OperandType.CounterPreset:
                                try
                                {
                                    _operandAddressValues[request.Type].TryAdd(address, BitConverter.ToInt16(messageData.Slice(index, operandByteLength).Span));
                                }
                                catch (Exception e)
                                {
                                    throw new PComAException("Failed to Unpack Value for Operand Type '" + request.Type + "'", e);
                                }
                                break;

                            case OperandType.ML:
                            case OperandType.SL:
                            case OperandType.XL:
                                try
                                {
                                    _operandAddressValues[request.Type].TryAdd(address, BitConverter.ToInt32(messageData.Slice(index, operandByteLength).Span));
                                }
                                catch (Exception e)
                                {
                                    throw new PComAException("Failed to Unpack Value for Operand Type '" + request.Type + "'", e);
                                }
                                break;

                            case OperandType.DW:
                            case OperandType.SDW:
                            case OperandType.XDW:
                                try
                                {
                                    _operandAddressValues[request.Type].TryAdd(address, BitConverter.ToUInt32(messageData.Slice(index, operandByteLength).Span));
                                }
                                catch (Exception e)
                                {
                                    throw new PComAException("Failed to Unpack Value for Operand Type '" + request.Type + "'", e);
                                }
                                break;

                            case OperandType.MF:
                                try
                                {
                                    byte[] floatBytes = messageData.Slice(index, operandByteLength).ToArray();

                                    Array.Reverse(floatBytes);
                                    Array.Reverse(floatBytes, 0, 2);
                                    Array.Reverse(floatBytes, 2, 2);

                                    _operandAddressValues[request.Type].TryAdd(address, BitConverter.ToSingle(floatBytes));
                                }
                                catch (Exception e)
                                {
                                    throw new PComAException("Failed to Unpack Value for Operand Type '" + request.Type + "'", e);
                                }
                                break;

                            case OperandType.TimerCurrent:
                            case OperandType.TimerPreset:
                                try
                                {
                                    _operandAddressValues[request.Type].TryAdd(address, BitConverter.ToUInt32(messageData.Slice(index, operandByteLength).Span) * 10);
                                }
                                catch (Exception e)
                                {
                                    throw new PComAException("Failed to Unpack Value for Operand Type '" + request.Type + "'", e);
                                }
                                break;
                        }

                        index += operandByteLength;
                    }
                }
                else if(request is ReadOperandsRequest.VectorialOperandRequest vectorialRequest)
                {
                    try
                    {
                        int byteCount = vectorialRequest.Count / 8;

                        if (vectorialRequest.Count % 8 != 0)
                        {
                            byteCount += 1;
                        }

                        BitArray bitArray = new BitArray(messageData.Slice(index, byteCount).ToArray());

                        ushort address = vectorialRequest.StartAddress;
                        int bitIndex = 0;

                        while (address < vectorialRequest.StartAddress + vectorialRequest.Count && bitIndex < bitArray.Count)
                        {
                            _operandAddressValues[request.Type].TryAdd(address, bitArray[bitIndex]);

                            bitIndex++;
                            address++;
                        }

                        index += byteCount;
                    }
                    catch (Exception e)
                    {
                        throw new PComAException("Failed to Unpack Value for Operand Type '" + request.Type + "'", e);
                    }
                }
            }
        }

        #endregion


        #region Private Methods

        private static byte calculateOperandByteLength(OperandType type)
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
                    return 2;

                case OperandType.ML:
                case OperandType.SL:
                case OperandType.XL:
                case OperandType.DW:
                case OperandType.SDW:
                case OperandType.XDW:
                case OperandType.MF:
                case OperandType.TimerCurrent:
                case OperandType.TimerPreset:
                    return 4;
            }

            return 1;
        }

        #endregion
    }
}
