using System;

namespace RICADO.Unitronics
{
    public class ReadClockResult : RequestResult
    {
        #region Private Fields

        private DateTime _clock;

        #endregion


        #region Public Properties

        public DateTime Clock => _clock;

        #endregion


        #region Constructors

        internal ReadClockResult(Channels.ProcessMessageResult result, DateTime clock) : base(result)
        {
            _clock = clock;
        }

        #endregion
    }
}
