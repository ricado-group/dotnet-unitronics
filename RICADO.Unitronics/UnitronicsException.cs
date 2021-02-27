using System;

namespace RICADO.Unitronics
{
    public class UnitronicsException : Exception
    {
        #region Constructors

        internal UnitronicsException(string message) : base(message)
        {
        }

        internal UnitronicsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion
    }
}
