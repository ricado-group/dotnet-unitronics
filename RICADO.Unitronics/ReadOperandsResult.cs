using System;

namespace RICADO.Unitronics
{
    public struct ReadOperandsResult
    {
        public int BytesSent;
        public int PacketsSent;
        public int BytesReceived;
        public int PacketsReceived;
        public double Duration;
        // TODO: Add a List or Dict or similar with Operand Type, Address and then Value
    }
}
