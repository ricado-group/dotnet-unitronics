using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RICADO.Unitronics.Responses
{
    public class ReadOperandsResponse : Response
    {
        #region Private Fields

        private readonly Dictionary<OperandType, Dictionary<ushort, object>> _operandAddressValues;

        #endregion


        #region Internal Properties

        internal Dictionary<OperandType, Dictionary<ushort, object>> OperandAddressValues => _operandAddressValues;

        #endregion


        #region Constructor

        internal ReadOperandsResponse() : base()
        {
            _operandAddressValues = new Dictionary<OperandType, Dictionary<ushort, object>>();
        }

        #endregion


        #region Public Methods

        public bool TryGetValue<T>(OperandType type, ushort address, out T value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(OperandType type, ushort address, out object value)
        {
            throw new NotImplementedException();
        }

        public T GetValue<T>(OperandType type, ushort address)
        {
            throw new NotImplementedException();
        }

        public object GetValue(OperandType type, ushort address)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
