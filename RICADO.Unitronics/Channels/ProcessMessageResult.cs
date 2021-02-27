using System;

namespace RICADO.Unitronics.Channels
{
    internal struct ProcessMessageResult
    {
        internal int BytesSent;
        internal int PacketsSent;
        internal int BytesReceived;
        internal int PacketsReceived;
        internal double Duration;
        internal Memory<byte> ResponseMessage;
    }
}
