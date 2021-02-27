using System;

namespace RICADO.Unitronics.Channels
{
    internal struct ReceiveMessageResult
    {
        internal Memory<byte> Message;
        internal int Bytes;
        internal int Packets;
    }
}
