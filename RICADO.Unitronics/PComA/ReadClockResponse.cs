using System;
using System.Text.RegularExpressions;

namespace RICADO.Unitronics.PComA
{
    internal class ReadClockResponse : Response
    {
        #region Constants

        private const ushort ClockMessageLength = 14;
        private const string ClockRegexPattern = "^(\\d{2})(\\d{2})(\\d{2})(\\d{2})(\\d{2})(\\d{2})(\\d{2})$";

        #endregion


        #region Private Fields

        private DateTime _dateTime;

        #endregion


        #region Public Properties

        public DateTime DateTime => _dateTime;

        #endregion


        #region Constructor

        protected ReadClockResponse(Request request, Memory<byte> responseMessage) : base(request, responseMessage)
        {
        }

        #endregion


        #region Public Methods

        public static ReadClockResponse UnpackResponseMessage(ReadClockRequest request, Memory<byte> responseMessage)
        {
            return new ReadClockResponse(request, responseMessage);
        }

        #endregion


        #region Protected Methods

        protected override void UnpackMessageDetail(string messageDetail)
        {
            if (messageDetail.Length < ClockMessageLength)
            {
                throw new PComAException("The Response Data Length of '" + messageDetail.Length + "' was too short - Expecting a Length of '" + ClockMessageLength + "'");
            }

            if (Regex.IsMatch(messageDetail, ClockRegexPattern) == false)
            {
                throw new PComAException("Invalid Clock Response Format");
            }

            string[] splitMessage = Regex.Split(messageDetail, ClockRegexPattern);

            if (int.TryParse(splitMessage[1], out int second) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Second");
            }

            if (int.TryParse(splitMessage[2], out int minute) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Minute");
            }

            if (int.TryParse(splitMessage[3], out int hour) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Hour");
            }

            if (int.TryParse(splitMessage[4], out int dayOfWeek) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Day of Week");
            }

            if (int.TryParse(splitMessage[5], out int day) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Day");
            }

            if (int.TryParse(splitMessage[6], out int month) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Month");
            }

            if (int.TryParse(splitMessage[7], out int year) == false)
            {
                throw new PComAException("Invalid Clock Response Value for Year");
            }

            _dateTime = new DateTime(year + 2000, month, day, hour, minute, second);
        }

        #endregion
    }
}
