using System;
using System.Text;

namespace RICADO.Unitronics.PComA
{
    internal abstract class Request
    {
        #region Constants

        public const string STX = "/";
        public const string ETX = "\r";

        #endregion


        #region Private Fields

        private readonly byte _unitId;
        private readonly string _commandCode;

        #endregion


        #region Public Properties

        public byte UnitID => _unitId;

        public string CommandCode => _commandCode;

        #endregion


        #region Constructor

        protected Request(byte unitId, string commandCode)
        {
            _unitId = unitId;
            _commandCode = commandCode;
        }

        #endregion


        #region Public Methods

        public ReadOnlyMemory<byte> BuildMessage()
        {
            StringBuilder messageBuilder = new StringBuilder();

            messageBuilder.AppendHexValue(_unitId);

            messageBuilder.Append(_commandCode);

            BuildMessageDetail(ref messageBuilder);

            messageBuilder.AppendChecksum();

            messageBuilder.Insert(0, STX);

            messageBuilder.Append(ETX);

            return Encoding.ASCII.GetBytes(messageBuilder.ToString());
        }

        #endregion


        #region Protected Methods

        protected abstract void BuildMessageDetail(ref StringBuilder messageBuilder);

        #endregion
    }
}
