using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Unitronics.Protocols;

namespace RICADO.Unitronics.Requests
{
    public class WriteOperandsRequest : Request
    {
        #region Private Locals

        private readonly Dictionary<OperandType, Dictionary<ushort, object>> _operandAddressValues = new Dictionary<OperandType, Dictionary<ushort, object>>();

        #endregion


        #region Internal Properties

        internal Dictionary<OperandType, Dictionary<ushort, object>> OperandAddressValues => _operandAddressValues;

        #endregion


        #region Constructor

        internal WriteOperandsRequest(UnitronicsPLC plc) : base(plc)
        {
        }

        #endregion


        #region Public Methods

        public void Add<T>(OperandType type, ushort address, T value)
        {
            if(value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if(Nullable.GetUnderlyingType(typeof(T)) != null)
            {
                
            }
            
            Add(type, address, (object)value);
        }

        public void Add(OperandType type, ushort address, object value)
        {
            // TODO: Convert the Object Value into the expected Type for the given Operand Type
        }

        #endregion


        #region Internal Methods

        internal override Task<ProcessRequestResult> ProcessRequest(UnitronicsPLC plc, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
