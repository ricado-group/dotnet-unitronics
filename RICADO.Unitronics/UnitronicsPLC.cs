using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Unitronics.Requests;
using RICADO.Unitronics.Responses;
using RICADO.Unitronics.Channels;
using RICADO.Unitronics.Protocols;

namespace RICADO.Unitronics
{
    public class UnitronicsPLC : IDisposable
    {
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

        public ReadOperandsRequest CreateReadOperandsRequest() => new ReadOperandsRequest(this);

        public async Task<ReadOperandsResult> ReadOperandsAsync(ReadOperandsRequest request, CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == false)
                {
                    throw new UnitronicsException("This Unitronics PLC must be Initialized first before any Requests can be Processed");
                }
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // TODO: Validate the Request is within the Bounds (and Operand Types) for this PLC Model

            throw new NotImplementedException();
        }

        public WriteOperandsRequest CreateWriteOperandsRequest() => new WriteOperandsRequest(this);

        public async Task<WriteOperandsResult> WriteOperandsAsync(WriteOperandsRequest request, CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == false)
                {
                    throw new UnitronicsException("This Unitronics PLC must be Initialized first before any Requests can be Processed");
                }
            }

            if(request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // TODO: Validate the Request is within the Bounds (and Operand Types) for this PLC Model

            throw new NotImplementedException();
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

            ReadOnlyMemory<byte> requestMessage = PComA.BuildReadClockMessage(_unitId);

            ProcessMessageResult result = await _channel.ProcessMessageAsync(requestMessage, ProtocolType.PComA, _unitId, _timeout, _retries, cancellationToken);

            DateTime dateTime = PComA.UnpackReadClockMessage(_unitId, result.ResponseMessage);

            return new ReadClockResult
            {
                BytesSent = result.BytesSent,
                PacketsSent = result.PacketsSent,
                BytesReceived = result.BytesReceived,
                PacketsReceived = result.PacketsReceived,
                Duration = result.Duration,
                Clock = dateTime,
            };
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

            ReadOnlyMemory<byte> requestMessage = PComA.BuildWriteClockMessage(_unitId, newDateTime);

            ProcessMessageResult result = await _channel.ProcessMessageAsync(requestMessage, ProtocolType.PComA, _unitId, _timeout, _retries, cancellationToken);

            PComA.ValidateWriteClockMessage(_unitId, result.ResponseMessage);

            return new WriteClockResult
            {
                BytesSent = result.BytesSent,
                PacketsSent = result.PacketsSent,
                BytesReceived = result.BytesReceived,
                PacketsReceived = result.PacketsReceived,
                Duration = result.Duration,
            };
        }

        #endregion


        #region Private Methods

        private async Task requestControllerInformation(CancellationToken cancellationToken)
        {
            ReadOnlyMemory<byte> requestMessage = PComA.BuildGetIdentificationMessage(_unitId);

            ProcessMessageResult result = await _channel.ProcessMessageAsync(requestMessage, ProtocolType.PComA, _unitId, _timeout, _retries, cancellationToken);

            try
            {
                string oldVersionRegex = "^(.{4})(.)(.{3})(.{3})(.{2})B(.{3})(.{3})(.{2})P(.{3})(.{3})(.{2})F(.)(.)(.{2}).{2}(.{2})(FT(.{5})(.{5}))?$";
                string newVersionRegex = "^(.{6})(.)(.{3})(.{3})(.{2})B(.{3})(.{3})(.{2})P(.{3})(.{3})(.{2})F(.)(.)(.{2}).{2}(.{2})(FT(.{5})(.{5}))?$";
                string shortVersionRegex = "^(.{4})(.)(.)(.{2})(.{2})$";
                
                string informationString = PComA.UnpackGetIdentificationMessage(_unitId, result.ResponseMessage);

                string[] splitInformation;

                if(Regex.IsMatch(informationString, oldVersionRegex))
                {
                    splitInformation = Regex.Split(informationString, oldVersionRegex);
                }
                else if(Regex.IsMatch(informationString, newVersionRegex))
                {
                    splitInformation = Regex.Split(informationString, newVersionRegex);
                }
                else if(Regex.IsMatch(informationString, shortVersionRegex))
                {
                    splitInformation = Regex.Split(informationString, shortVersionRegex);
                }
                else
                {
                    throw new PComAException("The Controller Information String Format was Invalid");
                }

                if(int.TryParse(splitInformation[3], out int majorVersion) == false || int.TryParse(splitInformation[4], out int minorVersion) == false || int.TryParse(splitInformation[5], out int buildVersion) == false)
                {
                    throw new PComAException("The Controller Information Version Strings were Invalid");
                }

                _version = new Version(majorVersion, minorVersion, buildVersion);

                _model = extractPLCModel(splitInformation[1]);
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

        private PLCModel extractPLCModel(string modelString)
        {
            if(modelString == null || modelString.Length < 4)
            {
                return PLCModel.Unknown;
            }

            if(modelString.Contains("BOOT"))
            {
                return PLCModel.Unknown;
            }
            
            switch(modelString)
            {
                case "B1  ":
                case "B1A ":
                case "R1  ":
                case "R1C ":
                case "R2C ":
                case "T   ":
                case "T1  ":
                case "T1C ":
                case "TA2C":
                case "TA3C":
                case "7B1 ":
                case "7B1A":
                case "7R1":
                case "7R1C":
                case "7T  ":
                case "7T1 ":
                case "7T1C":
                case "7TA2":
                case "7TA3":
                    return PLCModel.M90;

                case "1TC2":
                case "1UN2":
                case "1R1 ":
                case "1R2 ":
                case "1R2C":
                case "1T1 ":
                case "1UA2":
                case "1T2C":
                case "8TC2":
                case "8UN2":
                case "8R1 ":
                case "8R2 ":
                case "8R2C":
                case "8T1 ":
                case "8UA2":
                case "8T38":
                case "8T2C":
                case "8R6C":
                case "8R34":
                case "8A19":
                case "8A22":
                case "1T38":
                case "8RZ ":
                    return PLCModel.M91;

                case "JR14":
                case "JR17":
                case "JR10":
                case "JR16":
                case "JT10":
                case "JT17":
                case "JEW1":
                case "JE10":
                case "JR31":
                case "JT40":
                case "JP15":
                case "JE13":
                case "JA24":
                case "JN20":
                case "NR10":
                case "NR16":
                case "NR31":
                case "NT10":
                case "NT18":
                case "NT20":
                case "NT40":
                    return PLCModel.Jazz;

                case "2320":
                    return PLCModel.V230;

                case "2620":
                    return PLCModel.V260;

                case "2820":
                    return PLCModel.V280;

                case "2920":
                    return PLCModel.V290;

                case "VUN2":
                case "VR1 ":
                case "VR2C":
                case "VUA2":
                case "VT1 ":
                case "VT40":
                case "VT2C":
                case "VT38":
                case "WUN2":
                case "WR1 ":
                case "WR2C":
                case "WUA2":
                case "WT1 ":
                case "WT40":
                case "WT2C":
                case "WT38":
                case "WR6C":
                case "WR34":
                case "WA19":
                case "WA22":
                    return PLCModel.V120;

                case "ERC1":
                    return PLCModel.EX_RC1;

                case "5320":
                    return PLCModel.V530;

                case "49C3":
                case "57C3":
                case "49T3":
                case "57T3":
                case "49T2":
                case "57T2":
                case "49T4":
                case "57T4":
                    return PLCModel.V570;

                case "56C3":
                case "56T4":
                case "56T3":
                case "56T2":
                    return PLCModel.V560;

                case "13R2  ":
                case "13R34 ":
                case "13T2  ":
                case "13T38 ":
                case "13RA22":
                case "13TA24":
                case "13B1  ":
                case "13T40 ":
                case "13R6  ":
                case "13TR34":
                case "13TR22":
                case "13TR20":
                case "13TR6 ":
                case "13TU24":
                case "13XXXX":
                    return PLCModel.V130;

                case "35R2  ":
                case "35R34 ":
                case "35T2  ":
                case "35T38 ":
                case "35RA22":
                case "35TA24":
                case "35B1  ":
                case "35T40 ":
                case "35R6  ":
                case "35TR34":
                case "35TR22":
                case "35TR20":
                case "35TR6 ":
                case "35TU24":
                case "35XXXX":
                    return PLCModel.V350;

                case "43RH2 ":
                    return PLCModel.V430;

                case "S3T20 ":
                case "S3TA2 ":
                case "S3R20 ":
                    return PLCModel.Samba35;

                case "S4T20 ":
                case "S4TA2 ":
                case "S4R20 ":
                    return PLCModel.Samba43;

                case "70T2":
                    return PLCModel.V700;

                case "EC15  ":
                    return PLCModel.EXF_RC15;

                case "10T2":
                    return PLCModel.V1040;

                case "12T2":
                    return PLCModel.V1210;
            }

            if(modelString.StartsWith("13"))
            {
                return PLCModel.V130;
            }

            if(modelString.StartsWith("35"))
            {
                return PLCModel.V350;
            }

            if(modelString.StartsWith("43"))
            {
                return PLCModel.V430;
            }

            if(modelString.StartsWith("S3"))
            {
                return PLCModel.Samba35;
            }

            if(modelString.StartsWith("S4"))
            {
                return PLCModel.Samba43;
            }

            if(modelString.StartsWith("S7") || modelString.StartsWith("SO"))
            {
                return PLCModel.Samba70;
            }

            return PLCModel.Unknown;
        }

        private bool validateReadOperandsRequest()
        {
            throw new NotImplementedException();
        }

        private bool validateWriteOperandsRequest()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
