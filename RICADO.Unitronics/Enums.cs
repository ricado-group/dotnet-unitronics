using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RICADO.Unitronics
{
    public enum ConnectionMethod
    {
        Ethernet,
        SerialOverLAN,
    }

    public enum OperandType : ushort
    {
        // Bits
        MB = 10,
        SB = 11,
        XB = 12,
        //Input = 13,
        Input = 1,
        //Output = 14,
        Output = 2,
        TimerRunBit = 15,
        CounterRunBit = 16,

        // Integers
        MI = 20,
        SI = 21,
        XI = 22,
        CounterCurrent = 23,
        CounterPreset = 24,

        // Longs, Words and Floats
        ML = 40,
        SL = 41,
        XL = 42,
        DW = 43,
        SDW = 44,
        XDW = 45,
        TimerCurrent = 46,
        TimerPreset = 47,
        MF = 48,
    }

    public enum PLCModel
    {
        Unknown,
        
        // Basic
        M90,
        M91,
        Jazz,

        // Standard
        V120,
        V230,
        V260,
        V280,
        V290,
        V530,
        EX_RC1,

        // Enhanced
        V130,
        V350,
        V430,
        V560,
        V570,
        V700,
        V1040,
        V1210,
        Samba35,
        Samba43,
        Samba70,
        EXF_RC15,
    }

    public enum ProtocolType : byte
    {
        PComA = 101,
        PComB = 102,
    }
}
