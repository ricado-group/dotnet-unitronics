﻿using System;

namespace RICADO.Unitronics
{
    public class PComAException : Exception
    {
        #region Constructors

        internal PComAException(string message) : base(message)
        {
        }

        internal PComAException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion
    }
}
