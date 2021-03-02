using System;
using System.Collections.Generic;
using System.Linq;

namespace RICADO.Unitronics.PComB
{
    internal static class BinaryOperandTypes
    {
        #region Private Fields

        private static readonly Dictionary<OperandType, BinaryOperandType> _binaryOperandTypes;

        #endregion


        #region Public Properties

        public static IReadOnlyDictionary<OperandType, byte> ReadOnly => _binaryOperandTypes.ToDictionary(item => item.Key, item => item.Value.ReadOnly);

        public static IReadOnlyDictionary<OperandType, byte> ReadOnlyVectorial => _binaryOperandTypes.ToDictionary(item => item.Key, item => item.Value.ReadOnlyVectorial);

        public static IReadOnlyDictionary<OperandType, byte> ReadWrite => _binaryOperandTypes.ToDictionary(item => item.Key, item => item.Value.ReadWrite);

        #endregion


        #region Static Constructor

        static BinaryOperandTypes()
        {
            _binaryOperandTypes = new Dictionary<OperandType, BinaryOperandType>();

            _binaryOperandTypes.Add(OperandType.Input, new BinaryOperandType
            {
                ReadOnly = 9,
                ReadWrite = 9,
            });

            _binaryOperandTypes.Add(OperandType.Output, new BinaryOperandType
            {
                ReadOnly = 10,
                ReadWrite = 10,
            });

            _binaryOperandTypes.Add(OperandType.MB, new BinaryOperandType
            {
                ReadOnly = 1,
                ReadWrite = 1,
            });

            _binaryOperandTypes.Add(OperandType.SB, new BinaryOperandType
            {
                ReadOnly = 2,
                ReadWrite = 2,
            });

            _binaryOperandTypes.Add(OperandType.CounterRunBit, new BinaryOperandType
            {
                ReadOnly = 12,
                ReadWrite = 12,
            });

            _binaryOperandTypes.Add(OperandType.TimerRunBit, new BinaryOperandType
            {
                ReadOnly = 11,
                ReadWrite = 11,
            });

            _binaryOperandTypes.Add(OperandType.MI, new BinaryOperandType
            {
                ReadOnly = 3,
                ReadWrite = 3,
            });

            _binaryOperandTypes.Add(OperandType.ML, new BinaryOperandType
            {
                ReadOnly = 5,
                ReadWrite = 5,
            });

            _binaryOperandTypes.Add(OperandType.DW, new BinaryOperandType
            {
                ReadOnly = 16,
                ReadWrite = 16,
            });

            _binaryOperandTypes.Add(OperandType.MF, new BinaryOperandType
            {
                ReadOnly = 7,
                ReadWrite = 7,
            });

            _binaryOperandTypes.Add(OperandType.SI, new BinaryOperandType
            {
                ReadOnly = 4,
                ReadWrite = 4,
            });

            _binaryOperandTypes.Add(OperandType.SL, new BinaryOperandType
            {
                ReadOnly = 6,
                ReadWrite = 6,
            });

            _binaryOperandTypes.Add(OperandType.SDW, new BinaryOperandType
            {
                ReadOnly = 17,
                ReadWrite = 36,
            });

            _binaryOperandTypes.Add(OperandType.CounterCurrent, new BinaryOperandType
            {
                ReadOnly = 18,
                ReadWrite = 145,
            });

            _binaryOperandTypes.Add(OperandType.CounterPreset, new BinaryOperandType
            {
                ReadOnly = 19,
                ReadWrite = 144,
            });

            _binaryOperandTypes.Add(OperandType.TimerCurrent, new BinaryOperandType
            {
                ReadOnly = 20,
                ReadWrite = 129,
            });

            _binaryOperandTypes.Add(OperandType.TimerPreset, new BinaryOperandType
            {
                ReadOnly = 21,
                ReadWrite = 128,
            });

            _binaryOperandTypes.Add(OperandType.XB, new BinaryOperandType
            {
                ReadOnly = 26,
                ReadWrite = 64,
            });

            _binaryOperandTypes.Add(OperandType.XI, new BinaryOperandType
            {
                ReadOnly = 27,
                ReadWrite = 65,
            });

            _binaryOperandTypes.Add(OperandType.XL, new BinaryOperandType
            {
                ReadOnly = 28,
                ReadWrite = 66,
            });

            _binaryOperandTypes.Add(OperandType.XDW, new BinaryOperandType
            {
                ReadOnly = 29,
                ReadWrite = 67,
            });
        }

        #endregion


        #region Structs

        private struct BinaryOperandType
        {
            public byte ReadOnly;
            public byte ReadWrite;

            public byte ReadOnlyVectorial => (byte)(ReadOnly + 0x80);
        }

        #endregion
    }
}
