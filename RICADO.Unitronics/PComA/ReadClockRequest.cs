using System;
using System.Text;

namespace RICADO.Unitronics.PComA
{
    internal class ReadClockRequest : Request
    {
        #region Constructor

        protected ReadClockRequest(byte unitId, string commandCode) : base(unitId, commandCode)
        {
        }

        #endregion


        #region Public Methods

        public ReadClockResponse UnpackResponseMessage(Memory<byte> responseMessage)
        {
            return ReadClockResponse.UnpackResponseMessage(this, responseMessage);
        }

        public static ReadClockRequest CreateNew(UnitronicsPLC plc)
        {
            return new ReadClockRequest(plc.UnitID, Commands.ReadClock);
        }

        #endregion


        #region Protected Methods

        protected override void BuildMessageDetail(ref StringBuilder messageBuilder)
        {
        }

        #endregion
    }
}
