using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RICADO.Unitronics.Protocols
{
    internal static class PComB
    {
        #region Constants

        public const ushort HeaderLength = 24;
        public const ushort FooterLength = 3;

        #endregion


        #region Public Properties

        public static readonly byte[] STX = new byte[] { (byte)'/', (byte)'_', (byte)'O', (byte)'P', (byte)'L', (byte)'C' };

        public static readonly byte ETX = (byte)'\\';

        #endregion


        #region Public Methods

        

        #endregion


        #region Private Methods

        

        #endregion
    }
}
