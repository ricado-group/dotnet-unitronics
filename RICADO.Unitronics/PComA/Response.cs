using System;
using System.Linq;
using System.Text;
using System.Globalization;

namespace RICADO.Unitronics.PComA
{
    internal abstract class Response
    {
        #region Constants

        public const ushort UnitIDLength = 2;
        public const ushort CommandLength = 2;
        public const ushort CRCLength = 2;
        public const ushort ETXLength = 1;

        #endregion


        #region Private Fields

        private readonly Request _request;

        #endregion


        #region Protected Properties

        protected virtual Request Request => _request;

        #endregion


        #region Public Properties

        public static readonly byte[] STX = Encoding.ASCII.GetBytes("/A");

        public static readonly byte ETX = (byte)'\r';

        #endregion


        #region Constructor

        protected Response(Request request, Memory<byte> responseMessage)
        {
            _request = request;

            if (responseMessage.Length < STX.Length || responseMessage.Slice(0, STX.Length).Span.SequenceEqual(STX) == false)
            {
                throw new PComAException("Invalid or Missing STX");
            }

            if (responseMessage.Span[responseMessage.Length - ETXLength] != ETX)
            {
                throw new PComAException("Invalid or Missing ETX");
            }

            if (responseMessage.Length < STX.Length + UnitIDLength + CommandLength + CRCLength + ETXLength)
            {
                throw new PComAException("The PComA Response Message length was too short");
            }

            string checksum = Encoding.ASCII.GetString(responseMessage.Slice(responseMessage.Length - CRCLength - ETXLength, CRCLength).ToArray());

            string messageString = Encoding.ASCII.GetString(responseMessage.Slice(STX.Length, responseMessage.Length - STX.Length - CRCLength - ETXLength).ToArray());

            if (checksum != messageString.CalculateChecksum())
            {
                throw new PComAException("Checksum Verification Failure");
            }

            string responseUnitIdString = messageString.Substring(0, UnitIDLength);

            if (byte.TryParse(responseUnitIdString, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out byte responseUnitId) == false || _request.UnitID != responseUnitId)
            {
                throw new PComAException("The Unit ID for the PComA Request '" + _request.UnitID.ToString("X").PadLeft(UnitIDLength, '0') + "' did not match the PComA Response '" + responseUnitIdString + "'");
            }

            messageString = messageString.Remove(0, UnitIDLength);

            string responseCommandCode = messageString.Substring(0, CommandLength);

            if (responseCommandCode != _request.CommandCode.Substring(0, CommandLength))
            {
                throw new PComAException("The Command Code for the PComA Request '" + _request.CommandCode.Substring(0, CommandLength) + "' did not match the PComA Response '" + responseCommandCode + "'");
            }

            messageString = messageString.Remove(0, CommandLength);

            UnpackMessageDetail(messageString);
        }

        #endregion


        #region Protected Methods

        protected abstract void UnpackMessageDetail(string messageDetail);

        #endregion
    }
}
