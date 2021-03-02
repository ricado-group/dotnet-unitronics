using System;
using System.Text;

namespace RICADO.Unitronics.PComA
{
    internal class GetIdentificationRequest : Request
    {
        #region Constructor

        protected GetIdentificationRequest(byte unitId, string commandCode) : base(unitId, commandCode)
        {
        }

        #endregion


        #region Public Methods

        public GetIdentificationResponse UnpackResponseMessage(Memory<byte> responseMessage)
        {
            return GetIdentificationResponse.UnpackResponseMessage(this, responseMessage);
        }

        public static GetIdentificationRequest CreateNew(UnitronicsPLC plc)
        {
            return new GetIdentificationRequest(plc.UnitID, Commands.GetIdentification);
        }

        #endregion


        #region Protected Methods

        protected override void BuildMessageDetail(ref StringBuilder messageBuilder)
        {
        }

        #endregion
    }
}
