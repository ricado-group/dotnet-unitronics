using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Unitronics.Protocols;

namespace RICADO.Unitronics.Requests
{
    public class ReadOperandsRequest : Request
    {
        #region Private Fields

        private readonly Dictionary<OperandType, HashSet<ushort>> _operandAddresses;

        #endregion


        #region Internal Properties

        internal Dictionary<OperandType, HashSet<ushort>> OperandAddresses => _operandAddresses;

        #endregion


        #region Constructor

        internal ReadOperandsRequest(UnitronicsPLC plc) : base(plc)
        {
            _operandAddresses = new Dictionary<OperandType, HashSet<ushort>>();
        }

        #endregion


        #region Public Methods

        public void Add(OperandType type, ushort address)
        {
            AddRange(type, address, 1);
        }

        public void AddRange(OperandType type, ushort startAddress, ushort length)
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


        #region Internal Methods

        internal override Task<ProcessRequestResult> ProcessRequest(UnitronicsPLC plc, CancellationToken cancellationToken)
        {
            // TODO: Determine based on the PLC whether we will use PComA or PComB
            throw new NotImplementedException();
        }

        #endregion
    }
}
