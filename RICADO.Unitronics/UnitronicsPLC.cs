using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Unitronics.Channels;

namespace RICADO.Unitronics
{
    public class UnitronicsPLC : IDisposable
    {
        #region Constants

        internal const uint MaximumTimerValue = 359999990; // Maximum Timer Value - 99 Hours, 59 Minutes, 59 Seconds, 990 Milliseconds

        #endregion


        #region Private Fields

        private readonly byte _unitId;
        private readonly ConnectionMethod _connectionMethod;
        private readonly string _remoteHost;
        private readonly int _port;
        private int _timeout;
        private int _retries;

        private bool _isInitialized = false;
        private readonly object _isInitializedLock = new object();

        private IChannel _channel;

        private readonly Guid _internalUniqueId;

        private PLCModel _model;
        private Version _version;

        #endregion


        #region Internal Properties

        internal IChannel Channel => _channel;

        internal Guid InternalUniqueID => _internalUniqueId;

        internal ushort BufferSize => IsEnhanced ? (ushort)1000 : (ushort)496;

        internal bool IsBasic => _model switch
        {
            PLCModel.M90 => true,
            PLCModel.M91 => true,
            PLCModel.Jazz => true,
            _ => false,
        };

        internal bool IsStandard => _model switch
        {
            PLCModel.V120 => true,
            PLCModel.V230 => true,
            PLCModel.V260 => true,
            PLCModel.V280 => true,
            PLCModel.V290 => true,
            PLCModel.V530 => true,
            PLCModel.EX_RC1 => true,
            _ => false,
        };

        internal bool IsEnhanced => _model switch
        {
            PLCModel.V130 => true,
            PLCModel.V350 => true,
            PLCModel.V430 => true,
            PLCModel.V560 => true,
            PLCModel.V570 => true,
            PLCModel.V700 => true,
            PLCModel.V1040 => true,
            PLCModel.V1210 => true,
            PLCModel.Samba35 => true,
            PLCModel.Samba43 => true,
            PLCModel.Samba70 => true,
            PLCModel.EXF_RC15 => true,
            _ => false,
        };

        #endregion


        #region Public Properties

        public byte UnitID => _unitId;

        public ConnectionMethod ConnectionMethod => _connectionMethod;

        public string RemoteHost => _remoteHost;

        public int Port => _port;

        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
            }
        }

        public int Retries
        {
            get
            {
                return _retries;
            }
            set
            {
                _retries = value;
            }
        }

        public bool IsInitialized
        {
            get
            {
                lock (_isInitializedLock)
                {
                    return _isInitialized;
                }
            }
        }

        public PLCModel Model => _model;

        public Version Version => _version;

        #endregion


        #region Constructors

        public UnitronicsPLC(byte unitId, ConnectionMethod connectionMethod, string remoteHost, int port, int timeout = 2000, int retries = 1)
        {
            if (connectionMethod == ConnectionMethod.SerialOverLAN && unitId > 127)
            {
                throw new ArgumentOutOfRangeException(nameof(unitId), "The Unit ID cannot be greater than '127'");
            }

            _unitId = unitId;

            _connectionMethod = connectionMethod;

            if (remoteHost == null)
            {
                throw new ArgumentNullException(nameof(remoteHost), "The Remote Host cannot be Null");
            }

            if (remoteHost.Length == 0)
            {
                throw new ArgumentException("The Remote Host cannot be Empty", nameof(remoteHost));
            }

            _remoteHost = remoteHost;

            if (port <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "The Port cannot be less than 1");
            }

            _port = port;

            if (timeout <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "The Timeout Value cannot be less than 1");
            }

            _timeout = timeout;

            if (retries < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retries), "The Retries Value cannot be Negative");
            }

            _retries = retries;

            _internalUniqueId = Guid.NewGuid();
        }

        #endregion


        #region Public Methods

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == true)
                {
                    return;
                }
            }

            // Initialize the Channel
            if (_connectionMethod == ConnectionMethod.Ethernet)
            {
                try
                {
                    _channel = new EthernetChannel(_remoteHost, _port);

                    await _channel.InitializeAsync(_timeout, cancellationToken);
                }
                catch (ObjectDisposedException)
                {
                    throw new UnitronicsException("Failed to Create the Ethernet Communication Channel for Unitronics PLC '" + _remoteHost + ":" + _port + "' - The underlying Socket Connection has been Closed");
                }
                catch (TimeoutException)
                {
                    throw new UnitronicsException("Failed to Create the Ethernet Communication Channel within the Timeout Period for Unitronics PLC '" + _remoteHost + ":" + _port + "'");
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    throw new UnitronicsException("Failed to Create the Ethernet Communication Channel for Unitronics PLC '" + _remoteHost + ":" + _port + "'", e);
                }
            }
            else if (_connectionMethod == ConnectionMethod.SerialOverLAN)
            {
                try
                {
                    _channel = await SerialOverLANFactory.Instance.GetOrCreate(_internalUniqueId, _remoteHost, _port, _timeout, cancellationToken);
                }
                catch (ObjectDisposedException)
                {
                    throw new UnitronicsException("Failed to Create the Serial Over LAN Communication Channel for Unitronics PLC ID '" + _unitId + "' on '" + _remoteHost + ":" + _port + "' - The underlying Socket Connection has been Closed");
                }
                catch (TimeoutException)
                {
                    throw new UnitronicsException("Failed to Create the Serial Over LAN Communication Channel within the Timeout Period for Unitronics PLC ID '" + _unitId + "' on '" + _remoteHost + ":" + _port + "'");
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    throw new UnitronicsException("Failed to Create the Serial Over LAN Communication Channel for Unitronics PLC ID '" + _unitId + "' on '" + _remoteHost + ":" + _port + "'", e);
                }
            }

            await requestControllerInformation(cancellationToken);

            lock (_isInitializedLock)
            {
                _isInitialized = true;
            }
        }

        public void Dispose()
        {
            if (_channel is EthernetChannel)
            {
                _channel.Dispose();

                _channel = null;
            }
            else if (_channel is SerialOverLANChannel)
            {
                _ = Task.Run(async () => { await SerialOverLANFactory.Instance.TryRemove(_internalUniqueId, _remoteHost, _port, CancellationToken.None); });
            }

            lock (_isInitializedLock)
            {
                _isInitialized = false;
            }
        }

        public async Task<ReadOperandsResult> ReadOperandsAsync(ReadOperandsRequest readRequest, CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == false)
                {
                    throw new UnitronicsException("This Unitronics PLC must be Initialized first before any Requests can be Processed");
                }
            }

            if (readRequest == null)
            {
                throw new ArgumentNullException(nameof(readRequest));
            }

            foreach (KeyValuePair<OperandType, HashSet<ushort>> operandAddresses in readRequest.OperandAddresses)
            {
                validateOperandsRequest(operandAddresses.Key, operandAddresses.Value);
            }

            ReadOperandsResult result = new ReadOperandsResult();

            //if((IsEnhanced && Version.Major >= 3) || (IsStandard && Version.Major >= 5 && Version.Minor >= 3))
            if(false)
            {
                // PComB Read/Write Operands Command for Reading
                //
                // NOTE: This mixed Reading Command is actually super inefficient! So we won't support it
            }
            else if(IsEnhanced || IsStandard)
            {
                foreach (PComB.ReadOperandsRequest request in PComB.ReadOperandsRequest.CreateMultiple(this, readRequest.OperandAddresses))
                {
                    ProcessMessageResult messageResult = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.PComB, _unitId, _timeout, _retries, cancellationToken);

                    PComB.ReadOperandsResponse response = request.UnpackResponseMessage(messageResult.ResponseMessage, _connectionMethod == ConnectionMethod.Ethernet);

                    result.AddMessageResult(messageResult);

                    result.AddValueRange(response.OperandAddressValues);
                }
            }
            else
            {
                foreach(PComA.ReadOperandsRequest request in PComA.ReadOperandsRequest.CreateMultiple(this, readRequest.OperandAddresses))
                {
                    ProcessMessageResult messageResult = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.PComA, _unitId, _timeout, _retries, cancellationToken);

                    PComA.ReadOperandsResponse response = request.UnpackResponseMessage(messageResult.ResponseMessage);

                    result.AddMessageResult(messageResult);

                    ushort address = request.StartAddress;

                    foreach(object value in response.Values)
                    {
                        result.AddValue(request.Type, address, value);

                        address++;
                    }
                }
            }

            return result;
        }

        public async Task<WriteOperandResult> WriteOperandAsync(OperandType type, ushort address, object value, CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == false)
                {
                    throw new UnitronicsException("This Unitronics PLC must be Initialized first before any Requests can be Processed");
                }
            }

            if(value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            validateOperandsRequest(type, new ushort[] { address });

            validateWriteOperandRequest(type, address, value);

            ProcessMessageResult result;

            if ((IsEnhanced && Version.Major >= 3) || (IsStandard && Version.Major >= 5 && Version.Minor >= 3))
            {
                PComB.WriteOperandRequest request = PComB.WriteOperandRequest.CreateNew(this, type, address, value);

                result = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.PComB, _unitId, _timeout, _retries, cancellationToken);

                request.ValidateResponseMessage(result.ResponseMessage, _connectionMethod == ConnectionMethod.Ethernet);
            }
            else
            {
                PComA.WriteOperandRequest request = PComA.WriteOperandRequest.CreateNew(this, type, address, value);

                result = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.PComA, _unitId, _timeout, _retries, cancellationToken);

                request.ValidateResponseMessage(result.ResponseMessage);
            }

            return new WriteOperandResult(result);
        }

        public async Task<ReadClockResult> ReadClockAsync(CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == false)
                {
                    throw new UnitronicsException("This Unitronics PLC must be Initialized first before any Requests can be Processed");
                }
            }

            PComA.ReadClockRequest request = PComA.ReadClockRequest.CreateNew(this);

            ProcessMessageResult result = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.PComA, _unitId, _timeout, _retries, cancellationToken);

            PComA.ReadClockResponse response = request.UnpackResponseMessage(result.ResponseMessage);

            return new ReadClockResult(result, response.DateTime);
        }

        public async Task<WriteClockResult> WriteClockAsync(DateTime newDateTime, CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == false)
                {
                    throw new UnitronicsException("This Unitronics PLC must be Initialized first before any Requests can be Processed");
                }
            }

            DateTime minDateTime = new DateTime(2000, 1, 1, 0, 0, 0);

            if (newDateTime < minDateTime)
            {
                throw new ArgumentOutOfRangeException(nameof(newDateTime), "The Date Time Value cannot be less than '" + minDateTime.ToString() + "'");
            }

            DateTime maxDateTime = new DateTime(2099, 12, 31, 23, 59, 59);

            if (newDateTime > maxDateTime)
            {
                throw new ArgumentOutOfRangeException(nameof(newDateTime), "The Date Time Value cannot be greater than '" + maxDateTime.ToString() + "'");
            }

            PComA.WriteClockRequest request = PComA.WriteClockRequest.CreateNew(this, newDateTime);

            ProcessMessageResult result = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.PComA, _unitId, _timeout, _retries, cancellationToken);

            request.ValidateResponseMessage(result.ResponseMessage);

            return new WriteClockResult(result);
        }

        #endregion


        #region Internal Methods

        internal ushort? GetMaximumOperandAddress(OperandType type)
        {
            if (_model == PLCModel.Unknown)
            {
                return null;
            }

            if (IsBasic)
            {
                switch (type)
                {
                    case OperandType.MB:
                    case OperandType.SB:
                    case OperandType.MI:
                    case OperandType.SI:
                        return 255;

                    case OperandType.Input:
                    case OperandType.Output:
                        return 159;

                    case OperandType.TimerCurrent:
                    case OperandType.TimerPreset:
                    case OperandType.TimerRunBit:
                        return 63;
                }
            }

            if (IsStandard)
            {
                switch (type)
                {
                    case OperandType.MB:
                        return 4095;

                    case OperandType.MI:
                        return 2047;

                    case OperandType.SB:
                    case OperandType.SI:
                        return 511;

                    case OperandType.ML:
                        return 255;

                    case OperandType.SL:
                        return 55;

                    case OperandType.MF:
                        return 23;

                    case OperandType.DW:
                    case OperandType.SDW:
                        return 63;

                    case OperandType.Input:
                    case OperandType.Output:
                        return 543;

                    case OperandType.TimerCurrent:
                    case OperandType.TimerPreset:
                    case OperandType.TimerRunBit:
                        return 191;

                    case OperandType.CounterCurrent:
                    case OperandType.CounterPreset:
                    case OperandType.CounterRunBit:
                        return 23;
                }
            }

            if (IsEnhanced)
            {
                if (_model == PLCModel.V130 || _model == PLCModel.EXF_RC15)
                {
                    switch (type)
                    {
                        case OperandType.MB:
                            return 4095;

                        case OperandType.MI:
                            return 2047;

                        case OperandType.SB:
                        case OperandType.SI:
                            return 511;

                        case OperandType.ML:
                            return 255;

                        case OperandType.SL:
                            return 55;

                        case OperandType.MF:
                            return 23;

                        case OperandType.DW:
                        case OperandType.SDW:
                            return 63;

                        case OperandType.Input:
                        case OperandType.Output:
                            return 543;

                        case OperandType.TimerCurrent:
                        case OperandType.TimerPreset:
                        case OperandType.TimerRunBit:
                            return 191;

                        case OperandType.CounterCurrent:
                        case OperandType.CounterPreset:
                        case OperandType.CounterRunBit:
                            return 23;

                        case OperandType.XB:
                            return 1023;

                        case OperandType.XI:
                            return 511;

                        case OperandType.XL:
                            return 255;

                        case OperandType.XDW:
                            return 63;
                    }
                }
                else if (_model == PLCModel.Samba35 || _model == PLCModel.Samba43 || _model == PLCModel.Samba70)
                {
                    switch (type)
                    {
                        case OperandType.MB:
                            return 511;

                        case OperandType.MI:
                            return 255;

                        case OperandType.SB:
                        case OperandType.SI:
                            return 511;

                        case OperandType.ML:
                            return 31;

                        case OperandType.SL:
                            return 55;

                        case OperandType.MF:
                            return 23;

                        case OperandType.DW:
                            return 31;

                        case OperandType.SDW:
                            return 63;

                        case OperandType.Input:
                        case OperandType.Output:
                            return 271;

                        case OperandType.TimerCurrent:
                        case OperandType.TimerPreset:
                        case OperandType.TimerRunBit:
                            return 31;

                        case OperandType.CounterCurrent:
                        case OperandType.CounterPreset:
                        case OperandType.CounterRunBit:
                            return 15;

                        case OperandType.XB:
                            return 63;

                        case OperandType.XI:
                            return 31;

                        case OperandType.XL:
                            return 15;

                        case OperandType.XDW:
                            return 15;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case OperandType.MB:
                            return 8191;

                        case OperandType.MI:
                            return 4095;

                        case OperandType.SB:
                        case OperandType.SI:
                        case OperandType.ML:
                            return 511;

                        case OperandType.SL:
                            return 55;

                        case OperandType.MF:
                        case OperandType.SDW:
                            return 63;

                        case OperandType.DW:
                            return 255;

                        case OperandType.Input:
                        case OperandType.Output:
                            return 543;

                        case OperandType.TimerCurrent:
                        case OperandType.TimerPreset:
                        case OperandType.TimerRunBit:
                            return 383;

                        case OperandType.CounterCurrent:
                        case OperandType.CounterPreset:
                        case OperandType.CounterRunBit:
                            return 31;

                        case OperandType.XB:
                            return 1023;

                        case OperandType.XI:
                            return 511;

                        case OperandType.XL:
                            return 255;

                        case OperandType.XDW:
                            return 63;
                    }
                }
            }

            return null;
        }

        #endregion


        #region Private Methods

        private async Task requestControllerInformation(CancellationToken cancellationToken)
        {
            PComA.GetIdentificationRequest request = PComA.GetIdentificationRequest.CreateNew(this);

            ProcessMessageResult result = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.PComA, _unitId, _timeout, _retries, cancellationToken);

            try
            {
                PComA.GetIdentificationResponse response = request.UnpackResponseMessage(result.ResponseMessage);

                _version = response.Version;

                _model = response.Model;
            }
            catch (Exception e)
            {
                if (_connectionMethod == ConnectionMethod.Ethernet)
                {
                    throw new UnitronicsException("Failed to Extract the Controller Information for Unitronics PLC '" + _remoteHost + ":" + _port + "'", e);
                }
                else
                {
                    throw new UnitronicsException("Failed to Extract the Controller Information for Unitronics PLC ID '" + _unitId + "' on '" + _remoteHost + ":" + _port + "'", e);
                }
            }
        }

        private void validateOperandsRequest(OperandType type, ICollection<ushort> addresses)
        {
            if(addresses.Count == 0 || _model == PLCModel.Unknown)
            {
                return;
            }

            ushort? maximumAddress = GetMaximumOperandAddress(type);

            if(maximumAddress.HasValue == false)
            {
                throw new UnitronicsException("The '" + type + "' Operand Type is not supported by this '" + _model + "' Unitronics PLC");
            }

            if(addresses.Max() > maximumAddress.Value)
            {
                throw new UnitronicsException("The Address for an '" + type + "' Operand cannot be greater than '" + maximumAddress.Value + "' for this '" + _model + "' Unitronics PLC");
            }
        }

        private void validateWriteOperandRequest(OperandType type, ushort address, object value)
        {
            if(value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            
            switch(type)
            {
                case OperandType.CounterRunBit:
                case OperandType.TimerRunBit:
                case OperandType.Input:
                    throw new UnitronicsException("The '" + type + "' Operand Type cannot be Written to");

                case OperandType.Output:
                case OperandType.MB:
                case OperandType.SB:
                case OperandType.XB:
                    if(value.GetType() != typeof(bool))
                    {
                        throw new UnitronicsException("Invalid Value Type '" + value.GetType() + "' for the '" + type + "' Operand Type");
                    }
                    break;

                case OperandType.MI:
                case OperandType.SI:
                case OperandType.XI:
                case OperandType.CounterCurrent:
                case OperandType.CounterPreset:
                    if (isNumeric(value) == false)
                    {
                        throw new UnitronicsException("Invalid Value Type '" + value.GetType() + "' for the '" + type + "' Operand Type");
                    }
                    else if(value.TryConvertValue<short>(out _) == false)
                    {
                        throw new UnitronicsException("The Value must be between '" + short.MinValue + "' and '" + short.MaxValue + "' for the '" + type + "' Operand Type");
                    }
                    break;

                case OperandType.ML:
                case OperandType.SL:
                case OperandType.XL:
                    if (isNumeric(value) == false)
                    {
                        throw new UnitronicsException("Invalid Value Type '" + value.GetType() + "' for the '" + type + "' Operand Type");
                    }
                    else if (value.TryConvertValue<int>(out _) == false)
                    {
                        throw new UnitronicsException("The Value must be between '" + int.MinValue + "' and '" + int.MaxValue + "' for the '" + type + "' Operand Type");
                    }
                    break;

                case OperandType.DW:
                case OperandType.SDW:
                case OperandType.XDW:
                    if (isNumeric(value) == false)
                    {
                        throw new UnitronicsException("Invalid Value Type '" + value.GetType() + "' for the '" + type + "' Operand Type");
                    }
                    else if (value.TryConvertValue<uint>(out _) == false)
                    {
                        throw new UnitronicsException("The Value must be between '" + uint.MinValue + "' and '" + uint.MaxValue + "' for the '" + type + "' Operand Type");
                    }
                    break;

                case OperandType.MF:
                    if (isNumeric(value) == false)
                    {
                        throw new UnitronicsException("Invalid Value Type '" + value.GetType() + "' for the '" + type + "' Operand Type");
                    }
                    else if (value.TryConvertValue<float>(out _) == false)
                    {
                        throw new UnitronicsException("The Value must be between '" + float.MinValue + "' and '" + float.MaxValue + "' for the '" + type + "' Operand Type");
                    }
                    break;

                case OperandType.TimerCurrent:
                case OperandType.TimerPreset:
                    if(value is TimeSpan timeSpanValue && timeSpanValue.TotalMilliseconds > MaximumTimerValue)
                    {
                        throw new UnitronicsException("The Value must be between '" + TimeSpan.FromMilliseconds(uint.MinValue) + "' and '" + TimeSpan.FromMilliseconds(MaximumTimerValue) + "' for the '" + type + "' Operand Type");
                    }
                    else if(isNumeric(value) == false)
                    {
                        throw new UnitronicsException("Invalid Value Type '" + value.GetType() + "' for the '" + type + "' Operand Type");
                    }
                    else if(value.TryConvertValue(out uint timerValue) == false || timerValue > MaximumTimerValue)
                    {
                        throw new UnitronicsException("The Value must be between '" + uint.MinValue + "' and '" + MaximumTimerValue + "' for the '" + type + "' Operand Type");
                    }
                    break;
            }
        }

        private bool isNumeric(object value)
        {
            if (value == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }

            return false;
        }

        #endregion
    }
}
