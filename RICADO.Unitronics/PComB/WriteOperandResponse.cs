using System;
using System.Linq;

namespace RICADO.Unitronics.PComB
{
    internal class WriteOperandResponse : Response
    {
        #region Constructor

        protected WriteOperandResponse(Request request, Memory<byte> responseMessage, bool disableChecksum = false) : base(request, responseMessage, disableChecksum)
        {
        }

        #endregion


        #region Public Methods

        public static void ValidateResponseMessage(WriteOperandRequest request, Memory<byte> responseMessage, bool disableChecksum = false)
        {
            _ = new WriteOperandResponse(request, responseMessage, disableChecksum);
        }

        #endregion


        #region Protected Methods

        protected override void UnpackMessageData(Memory<byte> messageData)
        {
            if (messageData.Span.SequenceEqual(new byte[4]) == false)
            {
                throw new PComBException("The Write Response Data was Invalid");
            }
        }

        #endregion
    }
}
