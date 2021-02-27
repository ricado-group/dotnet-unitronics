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
        InputForce,
        OutputForce,
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

    public enum enOperandsExecuterType
    {
        None = 0,
        ASCII = 1,
        PartialBinary = 2,
        FullBinary = 3
    }

    public enum enPLCMode
    {
        Unknown,
        OS,
        BOOT,
        PreBOOT
    }

    public enum enBinaryCommand
    {
        ReadDataTables = 4,
        ReadFlashStatus = 7,
        ReadPLCName = 12,
        AccessSD = 42,
        ReadPartOfProjectDataTables = 75,
        WriteDataTables = 68,
        ReadOperands = 77,
        ReadWrite = 80
    }

    public enum enCOMPort
    {
        One = 1,
        Two = 2,
        Three = 3,
        CANBus = 'C'
    }

    internal enum enMessageDirection
    {
        Sent,
        Received,
        Unspecified
    }
}
