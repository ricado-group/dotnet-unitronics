using System;
using System.Text;

namespace RICADO.Unitronics.PComA
{
    internal class WriteClockRequest : Request
    {
        #region Private Fields

        private readonly DateTime _newDateTime;

        #endregion


        #region Public Properties

        public DateTime NewDateTime => _newDateTime;

        #endregion


        #region Constructor

        protected WriteClockRequest(byte unitId, string commandCode, DateTime newDateTime) : base(unitId, commandCode)
        {
            _newDateTime = newDateTime;
        }

        #endregion


        #region Public Methods

        public void ValidateResponseMessage(Memory<byte> responseMessage)
        {
            WriteClockResponse.ValidateResponseMessage(this, responseMessage);
        }

        public static WriteClockRequest CreateNew(UnitronicsPLC plc, DateTime newDateTime)
        {
            return new WriteClockRequest(plc.UnitID, Commands.GetIdentification, newDateTime);
        }

        #endregion


        #region Protected Methods

        protected override void BuildMessageDetail(ref StringBuilder messageBuilder)
        {
            messageBuilder.Append(_newDateTime.Second.ToString().PadLeft(2, '0'));
            messageBuilder.Append(_newDateTime.Minute.ToString().PadLeft(2, '0'));
            messageBuilder.Append(_newDateTime.Hour.ToString().PadLeft(2, '0'));

            int dayOfWeek = (int)_newDateTime.DayOfWeek + 1;
            messageBuilder.Append(dayOfWeek.ToString().PadLeft(2, '0'));

            messageBuilder.Append(_newDateTime.Day.ToString().PadLeft(2, '0'));
            messageBuilder.Append(_newDateTime.Month.ToString().PadLeft(2, '0'));

            string yearString = _newDateTime.Year.ToString().PadLeft(2, '0');

            if (yearString.Length > 2)
            {
                yearString = yearString.Substring(2);
            }

            messageBuilder.Append(yearString);
        }

        #endregion
    }
}
