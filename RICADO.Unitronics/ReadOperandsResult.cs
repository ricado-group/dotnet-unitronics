using System;
using System.Collections.Generic;

namespace RICADO.Unitronics
{
    public class ReadOperandsResult
    {
        #region Private Fields

        private int _bytesSent;
        private int _packetsSent;
        private int _bytesReceived;
        private int _packetsReceived;
        private double _duration;

        private readonly Dictionary<OperandType, Dictionary<ushort, object>> _operandAddressValues = new Dictionary<OperandType, Dictionary<ushort, object>>();

        #endregion


        #region Public Properties

        public int BytesSent => _bytesSent;

        public int PacketsSent => _packetsSent;

        public int BytesReceived => _bytesReceived;

        public int PacketsReceived => _packetsReceived;

        public double Duration => _duration;

        public Dictionary<OperandType, Dictionary<ushort, object>> OperandAddressValues => _operandAddressValues;

        #endregion


        #region Public Methods

        public bool TryGetValue<T>(OperandType type, ushort address, out T value) where T : struct
        {
            value = default(T);
            
            if (_operandAddressValues.ContainsKey(type) == false || _operandAddressValues[type].ContainsKey(address) == false || _operandAddressValues[type][address] == null)
            {
                return false;
            }

            return _operandAddressValues[type][address].TryGetValue(out value);
        }

        public bool TryGetValue(OperandType type, ushort address, out object value)
        {
            value = default;

            if (_operandAddressValues.ContainsKey(type) == false || _operandAddressValues[type].ContainsKey(address) == false || _operandAddressValues[type][address] == null)
            {
                return false;
            }

            value = _operandAddressValues[type][address];

            return true;
        }

        #endregion


        #region Internal Methods

        internal void AddValue(OperandType type, ushort address, object value)
        {
            if (_operandAddressValues.ContainsKey(type) == false)
            {
                _operandAddressValues.Add(type, new Dictionary<ushort, object>());
            }

            if (_operandAddressValues[type].ContainsKey(address) == false)
            {
                _operandAddressValues[type].Add(address, value);
            }
        }

        internal void AddMessageResult(Channels.ProcessMessageResult result)
        {
            _bytesSent += result.BytesSent;
            _packetsSent += result.PacketsSent;
            _bytesReceived += result.BytesReceived;
            _packetsReceived += result.PacketsReceived;
            _duration += result.Duration;
        }

        #endregion
    }
}
