using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Unitronics.PComB
{
    internal abstract class Request
    {
        #region Private Fields

        private readonly byte _unitId;
        private readonly CommandCode _commandCode;

        #endregion


        #region Public Properties

        public static readonly ushort HeaderLength = 24;

        public static readonly ushort FooterLength = 3;

        public static readonly byte[] STX = new byte[] { (byte)'/', (byte)'_', (byte)'O', (byte)'P', (byte)'L', (byte)'C' };

        public static readonly byte ETX = (byte)'\\';

        public byte UnitID => _unitId;

        public CommandCode CommandCode => _commandCode;

        #endregion


        #region Constructor

        protected Request(byte unitId, CommandCode commandCode)
        {
            _unitId = unitId;
            _commandCode = commandCode;
        }

        #endregion


        #region Public Methods

        public ReadOnlyMemory<byte> BuildMessage()
        {
            ICollection<byte> commandDetails = BuildCommandDetails();

            while(commandDetails.Count < 6)
            {
                commandDetails.Add(0);
            }
            
            ICollection<byte> messageData = BuildMessageData();

            List<byte> message = new List<byte>(HeaderLength + messageData.Count + FooterLength);

            // STX
            message.AddRange(STX);

            // Destination Unit ID
            message.Add(_unitId);

            // Source Unit ID
            message.Add(254);

            // Reserved
            message.Add(1);
            message.Add(0);
            message.Add(0);
            message.Add(0);

            // Command Code
            message.Add((byte)_commandCode);

            // Sub Command Code
            message.Add(0);

            // Command Details
            message.AddRange(commandDetails.Take(6));

            // Data Length
            message.AddRange(BitConverter.GetBytes(Convert.ToUInt16(messageData.Count)));

            // Header Checksum
            message.AddRange(BitConverter.GetBytes(message.CalculateChecksum()));

            // Message Data
            message.AddRange(messageData);

            // Message Data Checksum
            message.AddRange(BitConverter.GetBytes(messageData.CalculateChecksum()));

            // ETX
            message.Add(ETX);

            return message.ToArray();
        }

        #endregion


        #region Protected Methods

        protected abstract ICollection<byte> BuildCommandDetails();

        protected abstract ICollection<byte> BuildMessageData();

        #endregion
    }
}
