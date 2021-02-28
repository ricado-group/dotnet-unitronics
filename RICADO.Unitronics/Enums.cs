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

    public enum OperandType
    {
        MB,
        SB,
        MI,
        SI,
        ML,
        SL,
        MF,
        Input,
        Output,
        TimerRunBit,
        CounterRunBit,
        DW,
        SDW,
        CounterCurrent,
        CounterPreset,
        TimerCurrent,
        TimerPreset,
        XB,
        XI,
        XDW,
        XL
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
