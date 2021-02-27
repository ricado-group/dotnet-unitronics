using RICADO.Unitronics.Responses;

namespace RICADO.Unitronics.Requests
{
    internal struct ProcessRequestResult
    {
        internal int BytesSent;
        internal int PacketsSent;
        internal int BytesReceived;
        internal int PacketsReceived;
        internal double Duration;
        internal Response Response;
    }
}
