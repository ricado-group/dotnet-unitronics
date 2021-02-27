using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RICADO.Unitronics.Protocols
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
