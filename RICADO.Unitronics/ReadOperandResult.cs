using System;

namespace RICADO.Unitronics
{
    public class ReadOperandResult : RequestResult
    {
        #region Private Fields

        private readonly object _value;

        #endregion


        #region Public Properties

        public object Value => _value;

        #endregion


        #region Constructor

        internal ReadOperandResult(Channels.ProcessMessageResult result, object value) : base(result)
        {
            _value = value;
        }

        #endregion


        #region Public Methods

        public bool TryGetValue<T>(out T value) where T : struct
        {
            return _value.TryConvertValue(out value);
        }

        #endregion
    }
}
