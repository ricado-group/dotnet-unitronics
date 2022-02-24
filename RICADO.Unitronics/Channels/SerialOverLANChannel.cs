using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Sockets;

namespace RICADO.Unitronics.Channels
{
    internal class SerialOverLANChannel : IChannel
    {
        #region Private Fields

        private readonly string _remoteHost;
        private readonly int _port;

        private TcpClient _client;

        private bool _isInitialized = false;
        private readonly object _isInitializedLock = new object();

        private readonly HashSet<Guid> _registeredPLCs = new HashSet<Guid>();
        private readonly object _registeredPLCsLock = new object();

        private readonly SemaphoreSlim _initializeSemaphore;
        private readonly SemaphoreSlim _requestSemaphore;

        private DateTime _lastInitializeAttempt = DateTime.MinValue;

        #endregion


        #region Internal Properties

        internal string RemoteHost => _remoteHost;

        internal int Port => _port;

        internal bool IsInitialized
        {
            get
            {
                lock (_isInitializedLock)
                {
                    return _isInitialized;
                }
            }
        }

        internal IReadOnlySet<Guid> RegisteredPLCs
        {
            get
            {
                lock (_registeredPLCsLock)
                {
                    return _registeredPLCs;
                }
            }
        }

        #endregion


        #region Constructors

        internal SerialOverLANChannel(string remoteHost, int port)
        {
            _remoteHost = remoteHost;
            _port = port;

            _initializeSemaphore = new SemaphoreSlim(1, 1);
            _requestSemaphore = new SemaphoreSlim(1, 1);
        }

        #endregion


        #region Public Methods

        public void Dispose()
        {
            try
            {
                _client?.Dispose();
            }
            catch
            {
            }
            finally
            {
                _client = null;
            }

            _initializeSemaphore?.Dispose();
            _requestSemaphore?.Dispose();

            lock (_isInitializedLock)
            {
                _isInitialized = false;
            }
        }

        #endregion


        #region Internal Methods

        public async Task InitializeAsync(int timeout, CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized)
                {
                    return;
                }
            }

            if (!_initializeSemaphore.Wait(0))
            {
                await _initializeSemaphore.WaitAsync(cancellationToken);
            }

            try
            {
                if (IsInitialized)
                {
                    return;
                }

                int retrySeconds = RegisteredPLCs.Count < 10 ? RegisteredPLCs.Count : 10;

                if (RegisteredPLCs.Count == 1 || DateTime.UtcNow.Subtract(_lastInitializeAttempt).TotalSeconds >= retrySeconds)
                {
                    _lastInitializeAttempt = DateTime.UtcNow;

                    cancellationToken.ThrowIfCancellationRequested();

                    destroyClient();

                    await initializeClient(timeout, cancellationToken);
                }
                else
                {
                    throw new UnitronicsException("Too Many Initialize Attempts for the Serial Over LAN Channel '" + RemoteHost + ":" + Port + "' - Retry after " + retrySeconds + " seconds");
                }
            }
            finally
            {
                _initializeSemaphore.Release();
            }

            lock (_isInitializedLock)
            {
                _isInitialized = true;
            }
        }

        public async Task<ProcessMessageResult> ProcessMessageAsync(ReadOnlyMemory<byte> requestMessage, ProtocolType protocol, byte unitId, int timeout, int retries, CancellationToken cancellationToken)
        {
            int attempts = 0;
            Memory<byte> responseMessage = new Memory<byte>();
            int bytesSent = 0;
            int packetsSent = 0;
            int bytesReceived = 0;
            int packetsReceived = 0;
            DateTime startTimestamp = DateTime.UtcNow;

            while (attempts <= retries)
            {
                if (!_requestSemaphore.Wait(0))
                {
                    await _requestSemaphore.WaitAsync(cancellationToken);
                }

                try
                {
                    if (attempts > 0)
                    {
                        await destroyAndInitializeClient(unitId, timeout, cancellationToken);
                    }

                    // Send the Message
                    SendMessageResult sendResult = await sendMessageAsync(requestMessage, protocol, unitId, timeout, cancellationToken);

                    bytesSent += sendResult.Bytes;
                    packetsSent += sendResult.Packets;

                    // Receive a Response
                    ReceiveMessageResult receiveResult = await receiveMessageAsync(protocol, unitId, timeout, cancellationToken);

                    bytesReceived += receiveResult.Bytes;
                    packetsReceived += receiveResult.Packets;
                    responseMessage = receiveResult.Message;

                    break;
                }
                catch (Exception)
                {
                    if (attempts >= retries)
                    {
                        throw;
                    }
                }
                finally
                {
                    _requestSemaphore.Release();
                }

                // Increment the Attempts
                attempts++;
            }

            return new ProcessMessageResult
            {
                BytesSent = bytesSent,
                PacketsSent = packetsSent,
                BytesReceived = bytesReceived,
                PacketsReceived = packetsReceived,
                Duration = DateTime.UtcNow.Subtract(startTimestamp).TotalMilliseconds,
                ResponseMessage = responseMessage,
            };
        }

        public void RegisterPLC(Guid uniqueId)
        {
            lock (_registeredPLCsLock)
            {
                if (_registeredPLCs.Contains(uniqueId) == false)
                {
                    _registeredPLCs.Add(uniqueId);
                }
            }
        }

        public void UnregisterPLC(Guid uniqueId)
        {
            lock (_registeredPLCsLock)
            {
                _registeredPLCs.RemoveWhere(id => id == uniqueId);
            }
        }

        #endregion


        #region Private Methods

        private Task initializeClient(int timeout, CancellationToken cancellationToken)
        {
            _client = new TcpClient(RemoteHost, Port);

            return _client.ConnectAsync(timeout, cancellationToken);
        }

        private void destroyClient()
        {
            try
            {
                _client?.Dispose();
            }
            finally
            {
                _client = null;
            }
        }

        private async Task destroyAndInitializeClient(byte unitId, int timeout, CancellationToken cancellationToken)
        {
            destroyClient();

            try
            {
                await initializeClient(timeout, cancellationToken);
            }
            catch (ObjectDisposedException)
            {
                throw new UnitronicsException("Failed to Re-Connect to Unitronics PLC ID '" + unitId + "' on '" + RemoteHost + ":" + Port + "' - The underlying Socket Connection has been Closed");
            }
            catch (TimeoutException)
            {
                throw new UnitronicsException("Failed to Re-Connect within the Timeout Period to Unitronics PLC ID '" + unitId + "' on '" + RemoteHost + ":" + Port + "'");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new UnitronicsException("Failed to Re-Connect to Unitronics PLC ID '" + unitId + "' on '" + RemoteHost + ":" + Port + "'", e);
            }
        }

        private async Task<SendMessageResult> sendMessageAsync(ReadOnlyMemory<byte> message, ProtocolType protocol, byte unitId, int timeout, CancellationToken cancellationToken)
        {
            SendMessageResult result = new SendMessageResult
            {
                Bytes = 0,
                Packets = 0,
            };

            try
            {
                result.Bytes += await _client.SendAsync(message, timeout, cancellationToken);
                result.Packets += 1;
            }
            catch (ObjectDisposedException)
            {
                throw new UnitronicsException("Failed to Send " + protocol + " Message to Unitronics PLC ID '" + unitId + "' on '" + RemoteHost + ":" + Port + "' - The underlying Socket Connection has been Closed");
            }
            catch (TimeoutException)
            {
                throw new UnitronicsException("Failed to Send " + protocol + " Message within the Timeout Period to Unitronics PLC ID '" + unitId + "' on '" + RemoteHost + ":" + Port + "'");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new UnitronicsException("Failed to Send " + protocol + " Message to Unitronics PLC ID '" + unitId + "' on '" + RemoteHost + ":" + Port + "'", e);
            }

            return result;
        }

        private async Task<ReceiveMessageResult> receiveMessageAsync(ProtocolType protocol, byte unitId, int timeout, CancellationToken cancellationToken)
        {
            ReceiveMessageResult result = new ReceiveMessageResult
            {
                Bytes = 0,
                Packets = 0,
                Message = new Memory<byte>(),
            };

            try
            {
                List<byte> receivedData = new List<byte>();
                DateTime startTimestamp = DateTime.UtcNow;

                bool receiveCompleted = false;

                while (DateTime.UtcNow.Subtract(startTimestamp).TotalMilliseconds < timeout && receiveCompleted == false)
                {
                    Memory<byte> buffer = new byte[1500];
                    TimeSpan receiveTimeout = TimeSpan.FromMilliseconds(timeout).Subtract(DateTime.UtcNow.Subtract(startTimestamp));

                    if (receiveTimeout.TotalMilliseconds >= 50)
                    {
                        int receivedBytes = await _client.ReceiveAsync(buffer, receiveTimeout, cancellationToken);

                        if (receivedBytes > 0)
                        {
                            receivedData.AddRange(buffer.Slice(0, receivedBytes).ToArray());

                            result.Bytes += receivedBytes;
                            result.Packets += 1;
                        }
                    }

                    receiveCompleted = isReceiveCompleted(protocol, receivedData);
                }

                if (receivedData.Count == 0)
                {
                    throw new UnitronicsException("Failed to Receive " + protocol + " Message from Unitronics PLC ID '" + unitId + "' on '" + RemoteHost + ":" + Port + "' - No Data was Received");
                }

                if (receiveCompleted == false)
                {
                    throw new UnitronicsException("Failed to Receive " + protocol + " Message within the Timeout Period from Unitronics PLC ID '" + unitId + "' on '" + RemoteHost + ":" + Port + "'");
                }

                result.Message = trimReceivedData(protocol, receivedData);
            }
            catch (ObjectDisposedException)
            {
                throw new UnitronicsException("Failed to Receive " + protocol + " Message from Unitronics PLC ID '" + unitId + "' on '" + RemoteHost + ":" + Port + "' - The underlying Socket Connection has been Closed");
            }
            catch (TimeoutException)
            {
                throw new UnitronicsException("Failed to Receive " + protocol + " Message within the Timeout Period from Unitronics PLC ID '" + unitId + "' on '" + RemoteHost + ":" + Port + "'");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new UnitronicsException("Failed to Receive " + protocol + " Message from Unitronics PLC ID '" + unitId + "' on '" + RemoteHost + ":" + Port + "'", e);
            }

            return result;
        }

        private bool isReceiveCompleted(ProtocolType protocol, List<byte> receivedData)
        {
            if(receivedData.Count == 0)
            {
                return false;
            }

            ReadOnlySpan<byte> stxPattern = protocol == ProtocolType.PComA ? PComA.Response.STX : PComB.Response.STX;

            if(receivedData.ContainsPattern(stxPattern) == false)
            {
                return false;
            }

            byte etxByte = protocol == ProtocolType.PComA ? PComA.Response.ETX : PComB.Response.ETX;

            if(receivedData.Contains(etxByte) == false)
            {
                return false;
            }

            if(protocol == ProtocolType.PComA)
            {
                return true;
            }

            int stxIndex = receivedData.IndexOf(stxPattern);

            if(receivedData.Count < stxIndex + PComB.Response.HeaderLength)
            {
                return false;
            }

            ushort messageDataLength = BitConverter.ToUInt16(receivedData.GetRange(stxIndex + 20, 2).ToArray());

            if(receivedData.Count < messageDataLength + PComB.Response.HeaderLength + PComB.Response.FooterLength)
            {
                return false;
            }

            if(receivedData.ElementAt(stxIndex + PComB.Response.HeaderLength + messageDataLength + PComB.Response.FooterLength - 1) != PComB.Response.ETX)
            {
                return false;
            }

            return true;
        }

        private Memory<byte> trimReceivedData(ProtocolType protocol, List<byte> receivedData)
        {
            if (receivedData.Count == 0)
            {
                return Memory<byte>.Empty;
            }

            ReadOnlySpan<byte> stxPattern = protocol == ProtocolType.PComA ? PComA.Response.STX : PComB.Response.STX;

            int stxIndex = receivedData.IndexOf(stxPattern);

            if (protocol == ProtocolType.PComA)
            {
                int etxIndex = receivedData.IndexOf(PComA.Response.ETX);

                return receivedData.GetRange(stxIndex, etxIndex - stxIndex + 1).ToArray();
            }
            else
            {
                ushort messageDataLength = BitConverter.ToUInt16(receivedData.GetRange(stxIndex + 20, 2).ToArray());

                return receivedData.GetRange(stxIndex, PComB.Response.HeaderLength + messageDataLength + PComB.Response.FooterLength).ToArray();
            }
        }

        #endregion
    }
}
