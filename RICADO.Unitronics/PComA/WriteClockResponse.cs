using System;

namespace RICADO.Unitronics.PComA
{
    internal class WriteClockResponse : Response
    {
        #region Constructor

        protected WriteClockResponse(Request request, Memory<byte> responseMessage) : base(request, responseMessage)
        {
        }

        #endregion


        #region Public Methods

        public static void ValidateResponseMessage(WriteClockRequest request, Memory<byte> responseMessage)
        {
            _ = new WriteClockResponse(request, responseMessage);
        }

        #endregion


        #region Protected Methods

        protected override void UnpackMessageDetail(string messageDetail)
        {
        }

        #endregion
    }
}
