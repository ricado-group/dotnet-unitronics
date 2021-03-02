using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RICADO.Unitronics.PComB
{
    internal class ReadOperandsRequest : Request
    {
        #region Constants

        private const byte MaximumOperandsLength = 255;

        #endregion


        #region Private Fields

        private readonly Dictionary<OperandType, IOperandRequest> _operandRequests = new Dictionary<OperandType, IOperandRequest>();

        #endregion


        #region Public Properties

        public Dictionary<OperandType, IOperandRequest> OperandRequests => _operandRequests;

        #endregion


        #region Constructor

        protected ReadOperandsRequest(byte unitId) : base(unitId, CommandCode.ReadOperands)
        {
        }

        #endregion


        #region Public Methods

        public ReadOperandsResponse UnpackResponseMessage(Memory<byte> responseMessage, bool disableChecksum = false)
        {
            return ReadOperandsResponse.UnpackResponseMessage(this, responseMessage, disableChecksum);
        }

        /*public static ReadOperandsRequest CreateNew(UnitronicsPLC plc, OperandType type, ushort startAddress, byte length)
        {
            if (length > MaximumOperandsLength)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The Number of Operands to Read cannot be greater than the Maximum Value '" + MaximumOperandsLength + "'");
            }

            byte operandLength = calculateOperandByteLength(operand);
            int maximumReceiveLength = plc.BufferSize - Response.HeaderLength - Response.FooterLength;

            if(length * operandLength > )

            return new ReadOperandsRequest(plc.UnitID, Commands.ReadOperands[type], type, startAddress, length);
        }*/

        public static HashSet<ReadOperandsRequest> CreateMultiple(UnitronicsPLC plc, Dictionary<OperandType, HashSet<ushort>> operandAddresses)
        {
            HashSet<ReadOperandsRequest> requests = new HashSet<ReadOperandsRequest>();

            ushort maximumBufferSize = (ushort)(plc.BufferSize - Response.HeaderLength - Response.FooterLength);

            ReadOperandsRequest request = null;

            foreach (OperandType operand in operandAddresses.Keys.OrderBy(type => (ushort)type))
            {
                foreach (ushort address in operandAddresses[operand].OrderBy(address => address))
                {
                    if(request == null)
                    {
                        request = new ReadOperandsRequest(plc.UnitID);
                    }
                    else if (request.newOperandAddressFitsBuffer(operand, address, maximumBufferSize) == false)
                    {
                        requests.Add(request);

                        request = new ReadOperandsRequest(plc.UnitID);
                    }

                    request.addOperandAddress(operand, address);
                }

                if (request != null && request.OperandRequests.Count > 0 && request.OperandRequests.Values.Any(request => request.Count > 0))
                {
                    requests.Add(request);

                    request = null;
                }
            }

            if (request != null && request.OperandRequests.Count > 0 && request.OperandRequests.Values.Any(request => request.Count > 0))
            {
                requests.Add(request);
            }

            return requests;
        }

        #endregion


        #region Protected Methods

        protected override ICollection<byte> BuildCommandDetails()
        {
            List<byte> commandDetails = new List<byte>(6);

            // Padding
            commandDetails.Add(0);
            commandDetails.Add(0);
            commandDetails.Add(0);
            commandDetails.Add(0);

            // Operand Requests Count
            commandDetails.AddRange(BitConverter.GetBytes((ushort)_operandRequests.Count));

            return commandDetails;
        }

        protected override ICollection<byte> BuildMessageData()
        {
            List<byte> messageData = new List<byte>();

            foreach(IOperandRequest request in _operandRequests.Values.Where(request => request.Count > 0).OrderBy(request => (ushort)request.Type))
            {
                messageData.AddRange(request.BuildDataStructure());
            }

            return messageData;
        }

        #endregion


        #region Private Methods

        private void addOperandAddress(OperandType type, ushort address)
        {
            if(isBitOperandType(type))
            {
                if (_operandRequests.ContainsKey(type) == false)
                {
                    //_operandRequests.Add(type, new VectorialOperandRequest(type, address, 1));
                    _operandRequests.Add(type, new VectorialOperandRequest(type, address, 8));
                    return;
                }

                /*if (_operandRequests.TryGetValue(type, out IOperandRequest genericRequest) && genericRequest is VectorialOperandRequest request && address >= request.StartAddress && request.Count < address - request.StartAddress + 1)
                {
                    request.Count = (ushort)(address - request.StartAddress + 1);
                }*/

                if (_operandRequests.TryGetValue(type, out IOperandRequest genericRequest) && genericRequest is VectorialOperandRequest request)
                {
                    if (address >= request.StartAddress && request.Count < address - request.StartAddress + 1)
                    {
                        request.Count = (ushort)(address - request.StartAddress + 1);
                    }

                    if(request.Count % 8 != 0)
                    {
                        request.Count = (ushort)(request.Count + (request.Count % 8));
                    }
                }
            }
            else
            {
                if (_operandRequests.ContainsKey(type) == false)
                {
                    _operandRequests.Add(type, new NonVectorialOperandRequest(type));
                }

                if (_operandRequests.TryGetValue(type, out IOperandRequest genericRequest) && genericRequest is NonVectorialOperandRequest request && request.Addresses.Contains(address) == false)
                {
                    request.Addresses.Add(address);
                }
            }
        }

        private bool newOperandAddressFitsBuffer(OperandType type, ushort address, ushort maximumBufferSize)
        {
            ushort requestBufferSize = calculateRequestBufferSize();
            ushort responseBufferSize = calculateResponseBufferSize();

            byte operandByteLength = calculateOperandByteLength(type);

            if(operandByteLength < 2)
            {
                operandByteLength = 2;
            }

            if (_operandRequests.ContainsKey(type) == false)
            {
                if(requestBufferSize + 6 > maximumBufferSize || responseBufferSize + operandByteLength > maximumBufferSize)
                {
                    return false;
                }

                return true;
            }

            if (_operandRequests.TryGetValue(type, out IOperandRequest genericRequest) == false)
            {
                return false;
            }

            if (isBitOperandType(type))
            {
                if(genericRequest is not VectorialOperandRequest)
                {
                    return false;
                }

                VectorialOperandRequest request = (VectorialOperandRequest)genericRequest;

                if(address < request.StartAddress)
                {
                    return false;
                }

                if(address >= request.StartAddress + request.Count)
                {
                    int additionalBitCount = address - request.StartAddress - request.Count + 1;

                    if(request.Count % 8 != 0)
                    {
                        additionalBitCount -= 8 - (request.Count % 8);
                    }

                    if(additionalBitCount <= 0)
                    {
                        return true;
                    }

                    int additionalBytes = additionalBitCount / 8;

                    if(additionalBitCount % 8 != 0)
                    {
                        additionalBytes += 1;
                    }

                    int currentBytes = request.Count / 8;

                    if (request.Count % 8 != 0)
                    {
                        currentBytes += 1;
                    }

                    if ((currentBytes + additionalBytes) % 2 != 0)
                    {
                        additionalBytes += 1;
                    }

                    if(responseBufferSize + additionalBytes > maximumBufferSize)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (genericRequest is not NonVectorialOperandRequest)
                {
                    return false;
                }

                NonVectorialOperandRequest request = (NonVectorialOperandRequest)genericRequest;

                if(request.Addresses.Contains(address))
                {
                    return true;
                }

                if(requestBufferSize + 2 > maximumBufferSize || responseBufferSize + operandByteLength > maximumBufferSize)
                {
                    return false;
                }
            }

            return true;
        }

        private ushort calculateRequestBufferSize() => (ushort)_operandRequests.Values.Where(request => request.Count > 0).Sum(request => (request.Count * 2) + 4);

        private ushort calculateResponseBufferSize()
        {
            ushort bufferSize = 0;

            foreach (IOperandRequest request in _operandRequests.Values.Where(request => request.Count > 0))
            {
                if (isBitOperandType(request.Type))
                {
                    bufferSize += (ushort)(request.Count / 8);

                    if(request.Count % 8 != 0)
                    {
                        bufferSize += 1;
                    }
                }
                else
                {
                    bufferSize += (ushort)(calculateOperandByteLength(request.Type) * request.Count);
                }
            }

            if(bufferSize % 2 != 0)
            {
                bufferSize += 1;
            }

            return bufferSize;
        }

        private static bool isBitOperandType(OperandType type)
        {
            if((ushort)type <= 19)
            {
                return true;
            }

            return false;
        }

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


        #region Supporting Classes

        public interface IOperandRequest
        {
            public OperandType Type { get; }
            public ushort Count { get; }

            public IEnumerable<byte> BuildDataStructure();
        }

        public class NonVectorialOperandRequest : IOperandRequest
        {
            #region Private Fields

            private readonly OperandType _type;
            private readonly List<ushort> _addresses;

            #endregion


            #region Public Properties

            public OperandType Type => _type;

            public List<ushort> Addresses => _addresses;

            public ushort Count => (ushort)_addresses.Count;

            #endregion


            #region Constructor

            public NonVectorialOperandRequest(OperandType type)
            {
                _type = type;
                _addresses = new List<ushort>();
            }

            #endregion


            #region Public Methods

            public IEnumerable<byte> BuildDataStructure()
            {
                List<byte> dataStructure = new List<byte>(4 + (Count * 2));

                // Number of Addresses (UInt16)
                dataStructure.AddRange(BitConverter.GetBytes(Count));

                // Operand Type
                dataStructure.Add(BinaryOperandTypes.ReadOnly[_type]);

                // Reserved (Always 0xFF)
                dataStructure.Add(byte.MaxValue);

                // Addresses
                dataStructure.AddRange(_addresses.OrderBy(address => address).SelectMany(address => BitConverter.GetBytes(address)));

                return dataStructure;
            }

            #endregion
        }

        public class VectorialOperandRequest : IOperandRequest
        {
            #region Private Fields

            private readonly OperandType _type;
            private ushort _startAddress;
            private ushort _count;

            #endregion


            #region Public Properties

            public OperandType Type => _type;

            public ushort StartAddress => _startAddress;

            public ushort Count
            {
                get
                {
                    return _count;
                }
                set
                {
                    _count = value;
                }
            }

            #endregion


            #region Constructor

            public VectorialOperandRequest(OperandType type, ushort startAddress, ushort count)
            {
                _type = type;
                _startAddress = startAddress;
                _count = count;
            }

            #endregion


            #region Public Methods

            public IEnumerable<byte> BuildDataStructure()
            {
                List<byte> dataStructure = new List<byte>(6);

                // Number of Addresses (UInt16)
                dataStructure.AddRange(BitConverter.GetBytes(Count));

                // Operand Type
                dataStructure.Add(BinaryOperandTypes.ReadOnlyVectorial[_type]);

                // Reserved (Always 0xFF)
                dataStructure.Add(byte.MaxValue);

                // Start Address
                dataStructure.AddRange(BitConverter.GetBytes(_startAddress));

                return dataStructure;
            }

            #endregion
        }

        #endregion
    }
}
