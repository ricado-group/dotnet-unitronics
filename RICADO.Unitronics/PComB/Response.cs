using System;
using System.Linq;

namespace RICADO.Unitronics.PComB
{
    internal abstract class Response
    {
        #region Constants

        private const int ChecksumLength = 2;
        private const int ETXLength = 1;

        #endregion


        #region Private Fields

        private readonly Request _request;

        #endregion


        #region Protected Properties

        protected virtual Request Request => _request;

        #endregion


        #region Public Properties

        public static ushort HeaderLength => Request.HeaderLength;

        public static ushort FooterLength => Request.FooterLength;

        public static byte[] STX => Request.STX;

        public static byte ETX => Request.ETX;

        #endregion


        #region Constructor

        protected Response(Request request, Memory<byte> responseMessage)
        {
            _request = request;

            if (responseMessage.Length < STX.Length || responseMessage.Slice(0, STX.Length).Span.SequenceEqual(STX) == false)
            {
                throw new PComBException("Invalid or Missing STX");
            }

            if (responseMessage.Span[responseMessage.Length - ETXLength] != ETX)
            {
                throw new PComBException("Invalid or Missing ETX");
            }

            if (responseMessage.Length < HeaderLength + FooterLength)
            {
                throw new PComBException("The PComB Response Message length was too short");
            }

            ushort headerChecksum = BitConverter.ToUInt16(responseMessage.Slice(HeaderLength - ChecksumLength, ChecksumLength).Span);

            if (headerChecksum != responseMessage.Slice(0, HeaderLength - ChecksumLength).ToArray().CalculateChecksum())
            {
                throw new PComBException("Header Checksum Verification Failure");
            }

            if (responseMessage.Span[6] != 254)
            {
                throw new PComBException("Invalid PComB Response Header");
            }

            if (responseMessage.Span[7] != request.UnitID)
            {
                throw new PComBException("The Unit ID for the PComB Request '" + request.UnitID + "' did not match the PComB Response '" + responseMessage.Span[7] + "'");
            }

            byte responseCommand = responseMessage.Span[12];

            if (responseCommand > 0x80)
            {
                responseCommand -= 0x80;
            }

            if (responseCommand != (byte)request.CommandCode)
            {
                throw new PComBException("The Command Code for the PComB Request '" + (byte)request.CommandCode + "' did not match the PComB Response '" + responseCommand + "'");
            }

            ushort messageDataLength = BitConverter.ToUInt16(responseMessage.Slice(HeaderLength - ChecksumLength - 2, 2).Span);

            if (responseMessage.Length < HeaderLength + messageDataLength + FooterLength)
            {
                throw new PComBException("The PComB Response Message length was too short");
            }

            responseMessage = responseMessage.Slice(HeaderLength, responseMessage.Length - HeaderLength);

            ushort dataChecksum = BitConverter.ToUInt16(responseMessage.Slice(messageDataLength, ChecksumLength).Span);

            if (dataChecksum != responseMessage.Slice(0, messageDataLength).ToArray().CalculateChecksum())
            {
                throw new PComBException("Message Data Checksum Verification Failure");
            }

            UnpackMessageData(responseMessage.Slice(0, messageDataLength));
        }

        #endregion


        #region Protected Methods

        protected abstract void UnpackMessageData(Memory<byte> messageData);

        #endregion
    }
}
