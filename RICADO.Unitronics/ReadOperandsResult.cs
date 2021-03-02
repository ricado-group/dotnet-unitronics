using System.Collections.Generic;

namespace RICADO.Unitronics
{
    public class ReadOperandsResult : RequestResult
    {
        #region Private Fields

        private readonly Dictionary<OperandType, Dictionary<ushort, object>> _operandAddressValues = new Dictionary<OperandType, Dictionary<ushort, object>>();

        #endregion


        #region Public Properties

        public Dictionary<OperandType, Dictionary<ushort, object>> OperandAddressValues => _operandAddressValues;

        #endregion


        #region Constructor

        internal ReadOperandsResult() : base()
        {
        }

        #endregion


        #region Public Methods

        public bool TryGetValue<T>(OperandType type, ushort address, out T value) where T : struct
        {
            value = default(T);
            
            if (_operandAddressValues.ContainsKey(type) == false || _operandAddressValues[type].ContainsKey(address) == false || _operandAddressValues[type][address] == null)
            {
                return false;
            }

            return _operandAddressValues[type][address].TryConvertValue(out value);
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

            _operandAddressValues[type].TryAdd(address, value);
        }

        internal void AddValueRange(Dictionary<OperandType, Dictionary<ushort, object>> operandAddressValues)
        {
            foreach(OperandType type in operandAddressValues.Keys)
            {
                if(_operandAddressValues.ContainsKey(type) == false)
                {
                    _operandAddressValues.Add(type, new Dictionary<ushort, object>());
                }

                foreach(ushort address in operandAddressValues[type].Keys)
                {
                    _operandAddressValues[type].TryAdd(address, operandAddressValues[type][address]);
                }
            }
        }

        #endregion
    }
}
