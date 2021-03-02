using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RICADO.Unitronics.PComA
{
    internal static class Commands
    {
        #region Private Fields

        private static readonly Dictionary<OperandType, OperandCommandCode> _operandCommandCodes;

        #endregion


        #region Public Properties

        public static readonly string GetIdentification = "ID";
        public static readonly string ReadClock = "RC";
        public static readonly string WriteClock = "SC";

        public static IReadOnlyDictionary<OperandType, string> ReadOperands => _operandCommandCodes.Where(item => item.Value.ReadCode != null && item.Value.ReadCode.Length > 0).ToDictionary(item => item.Key, item => item.Value.ReadCode);
        public static IReadOnlyDictionary<OperandType, string> WriteOperands => _operandCommandCodes.Where(item => item.Value.WriteCode != null && item.Value.WriteCode.Length > 0).ToDictionary(item => item.Key, item => item.Value.WriteCode);

        #endregion


        #region Static Constructor

        static Commands()
        {
            _operandCommandCodes = new Dictionary<OperandType, OperandCommandCode>();

            _operandCommandCodes.Add(OperandType.Input, new OperandCommandCode
            {
                ReadCode = "RE",
            });

            _operandCommandCodes.Add(OperandType.Output, new OperandCommandCode
            {
                ReadCode = "RA",
                WriteCode = "SA",
            });

            _operandCommandCodes.Add(OperandType.MB, new OperandCommandCode
            {
                ReadCode = "RB",
                WriteCode = "SB",
            });

            _operandCommandCodes.Add(OperandType.SB, new OperandCommandCode
            {
                ReadCode = "GS",
                WriteCode = "SS",
            });

            _operandCommandCodes.Add(OperandType.CounterRunBit, new OperandCommandCode
            {
                ReadCode = "RM",
            });

            _operandCommandCodes.Add(OperandType.TimerRunBit, new OperandCommandCode
            {
                ReadCode = "RT",
            });

            _operandCommandCodes.Add(OperandType.MI, new OperandCommandCode
            {
                ReadCode = "RW",
                WriteCode = "SW",
            });

            _operandCommandCodes.Add(OperandType.ML, new OperandCommandCode
            {
                ReadCode = "RNL",
                WriteCode = "SNL",
            });

            _operandCommandCodes.Add(OperandType.DW, new OperandCommandCode
            {
                ReadCode = "RND",
                WriteCode = "SND",
            });

            _operandCommandCodes.Add(OperandType.MF, new OperandCommandCode
            {
                ReadCode = "RNF",
                WriteCode = "SNF",
            });

            _operandCommandCodes.Add(OperandType.SI, new OperandCommandCode
            {
                ReadCode = "GF",
                WriteCode = "SF",
            });

            _operandCommandCodes.Add(OperandType.SL, new OperandCommandCode
            {
                ReadCode = "RNH",
                WriteCode = "SNH",
            });

            _operandCommandCodes.Add(OperandType.SDW, new OperandCommandCode
            {
                ReadCode = "RNJ",
                WriteCode = "SNJ",
            });

            _operandCommandCodes.Add(OperandType.CounterCurrent, new OperandCommandCode
            {
                ReadCode = "GX",
                WriteCode = "SK",
            });

            _operandCommandCodes.Add(OperandType.CounterPreset, new OperandCommandCode
            {
                ReadCode = "GY",
                WriteCode = "SJ",
            });

            _operandCommandCodes.Add(OperandType.TimerCurrent, new OperandCommandCode
            {
                ReadCode = "GT",
                WriteCode = "SNK",
            });

            _operandCommandCodes.Add(OperandType.TimerPreset, new OperandCommandCode
            {
                ReadCode = "GP",
                WriteCode = "SNT",
            });

            _operandCommandCodes.Add(OperandType.XB, new OperandCommandCode
            {
                ReadCode = "RZB",
                WriteCode = "SZB",
            });

            _operandCommandCodes.Add(OperandType.XI, new OperandCommandCode
            {
                ReadCode = "RZI",
                WriteCode = "SZI",
            });

            _operandCommandCodes.Add(OperandType.XL, new OperandCommandCode
            {
                ReadCode = "RZL",
                WriteCode = "SZL",
            });

            _operandCommandCodes.Add(OperandType.XDW, new OperandCommandCode
            {
                ReadCode = "RZD",
                WriteCode = "SZD",
            });
        }

        #endregion


        #region Structs

        internal struct OperandCommandCode
        {
            public string ReadCode;
            public string WriteCode;
        }

        #endregion
    }
}
