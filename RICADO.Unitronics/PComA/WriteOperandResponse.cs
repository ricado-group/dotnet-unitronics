using System;

namespace RICADO.Unitronics.PComA
{
    internal class WriteOperandResponse : Response
    {
        #region Constructor

        protected WriteOperandResponse(Request request, Memory<byte> responseMessage) : base(request, responseMessage)
        {
        }

        #endregion


        #region Public Methods

        public static void ValidateResponseMessage(WriteOperandRequest request, Memory<byte> responseMessage)
        {
            _ = new WriteOperandResponse(request, responseMessage);
        }

        #endregion


        #region Protected Methods

        protected override void UnpackMessageDetail(string messageDetail)
        {
        }

        #endregion
    }
}
