using System;

namespace RICADO.Unitronics
{
    public class PComBException : Exception
    {
        #region Constructors

        internal PComBException(string message) : base(message)
        {
        }

        internal PComBException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion
    }
}
