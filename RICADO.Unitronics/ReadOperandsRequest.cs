using System;
using System.Collections.Generic;

namespace RICADO.Unitronics
{
    public class ReadOperandsRequest
    {
        #region Private Fields

        private readonly Dictionary<OperandType, HashSet<ushort>> _operandAddresses = new Dictionary<OperandType, HashSet<ushort>>();

        #endregion


        #region Public Properties

        public Dictionary<OperandType, HashSet<ushort>> OperandAddresses => _operandAddresses;

        #endregion


        #region Public Methods

        public void Add(OperandType type, ushort address)
        {
            Add(type, address, 1);
        }

        public void Add(OperandType type, ushort startAddress, ushort length)
        {
            if(_operandAddresses.ContainsKey(type) == false)
            {
                _operandAddresses.Add(type, new HashSet<ushort>());
            }
            
            for(ushort i = startAddress; i < startAddress + length; i++)
            {
                if(_operandAddresses[type].Contains(i) == false)
                {
                    _operandAddresses[type].Add(i);
                }
            }
        }

        #endregion
    }
}
