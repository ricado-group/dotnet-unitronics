using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RICADO.Sockets;

namespace RICADO.Unitronics.Channels
{
    internal class EthernetChannel : IChannel
    {
        #region Constants

        internal const int TCPHeaderLength = 6;

        #endregion


        #region Private Fields

        private readonly string _remoteHost;
        private readonly int _port;

        private TcpClient _client;

        private ushort _requestId = 0;

        private readonly SemaphoreSlim _semaphore;

        #endregion


        #region Internal Properties

        internal string RemoteHost => _remoteHost;

        internal int Port => _port;

        #endregion


        #region Constructors

        internal EthernetChannel(string remoteHost, int port)
        {
            _remoteHost = remoteHost;
            _port = port;

            _semaphore = new SemaphoreSlim(1, 1);
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

            _semaphore?.Dispose();
        }

        #endregion


        #region Internal Methods

        public async Task InitializeAsync(int timeout, CancellationToken cancellationToken)
        {
            if (!_semaphore.Wait(0))
            {
                await _semaphore.WaitAsync(cancellationToken);
            }

            try
            {
                destroyClient();

                await initializeClient(timeout, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
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
                if (!_semaphore.Wait(0))
                {
                    await _semaphore.WaitAsync(cancellationToken);
                }

                try
                {
                    if (attempts > 0)
                    {
                        await destroyAndInitializeClient(timeout, cancellationToken);
                    }

                    // Send the Message
                    SendMessageResult sendResult = await sendMessageAsync(requestMessage, protocol, timeout, cancellationToken);

                    bytesSent += sendResult.Bytes;
                    packetsSent += sendResult.Packets;

                    // Receive a Response
                    ReceiveMessageResult receiveResult = await receiveMessageAsync(protocol, timeout, cancellationToken);

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
                    _semaphore.Release();
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

        private async Task destroyAndInitializeClient(int timeout, CancellationToken cancellationToken)
        {
            destroyClient();

            try
            {
                await initializeClient(timeout, cancellationToken);
            }
            catch (ObjectDisposedException)
            {
                throw new UnitronicsException("Failed to Re-Connect to Unitronics PLC '" + RemoteHost + ":" + Port + "' - The underlying Socket Connection has been Closed");
            }
            catch (TimeoutException)
            {
                throw new UnitronicsException("Failed to Re-Connect within the Timeout Period to Unitronics PLC '" + RemoteHost + ":" + Port + "'");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new UnitronicsException("Failed to Re-Connect to Unitronics PLC '" + RemoteHost + ":" + Port + "'", e);
            }
        }

        private async Task<SendMessageResult> sendMessageAsync(ReadOnlyMemory<byte> message, ProtocolType protocol, int timeout, CancellationToken cancellationToken)
        {
            SendMessageResult result = new SendMessageResult
            {
                Bytes = 0,
                Packets = 0,
            };

            ReadOnlyMemory<byte> modbusTcpMessage = buildTcpMessage(message, protocol);

            try
            {
                result.Bytes += await _client.SendAsync(modbusTcpMessage, timeout, cancellationToken);
                result.Packets += 1;
            }
            catch (ObjectDisposedException)
            {
                throw new UnitronicsException("Failed to Send " + protocol + " Message to Unitronics PLC '" + RemoteHost + ":" + Port + "' - The underlying Socket Connection has been Closed");
            }
            catch (TimeoutException)
            {
                throw new UnitronicsException("Failed to Send " + protocol + " Message within the Timeout Period to Unitronics PLC '" + RemoteHost + ":" + Port + "'");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new UnitronicsException("Failed to Send " + protocol + " Message to Unitronics PLC '" + RemoteHost + ":" + Port + "'", e);
            }

            return result;
        }

        private async Task<ReceiveMessageResult> receiveMessageAsync(ProtocolType protocol, int timeout, CancellationToken cancellationToken)
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

                while (DateTime.UtcNow.Subtract(startTimestamp).TotalMilliseconds < timeout && receivedData.Count < TCPHeaderLength)
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
                }

                if (receivedData.Count == 0)
                {
                    throw new UnitronicsException("Failed to Receive " + protocol + " Message from Unitronics PLC '" + RemoteHost + ":" + Port + "' - No Data was Received");
                }

                if (receivedData.Count < TCPHeaderLength)
                {
                    throw new UnitronicsException("Failed to Receive " + protocol + " Message within the Timeout Period from Unitronics PLC '" + RemoteHost + ":" + Port + "'");
                }

                if (receivedData[3] != 0)
                {
                    throw new UnitronicsException("Failed to Receive " + protocol + " Message from Unitronics PLC  '" + RemoteHost + ":" + Port + "' - The TCP Header was Invalid");
                }

                if (BitConverter.ToUInt16(receivedData.GetRange(0, 2).ToArray(), 0) != _requestId)
                {
                    throw new UnitronicsException("Failed to Receive " + protocol + " Message from Unitronics PLC '" + RemoteHost + ":" + Port + "' - The TCP Header Transaction ID did not Match");
                }

                if (receivedData[2] != (byte)protocol)
                {
                    throw new UnitronicsException("Failed to Receive " + protocol + " Message from Unitronics PLC  '" + RemoteHost + ":" + Port + "' - The TCP Header Protocol Type did not Match");
                }

                int tcpMessageDataLength = BitConverter.ToUInt16(new byte[] { receivedData[4], receivedData[5] });

                if (tcpMessageDataLength <= 0 || tcpMessageDataLength > 1009)
                {
                    throw new UnitronicsException("Failed to Receive " + protocol + " Message from Unitronics PLC  '" + RemoteHost + ":" + Port + "' - The TCP Message Length was Invalid");
                }

                receivedData.RemoveRange(0, TCPHeaderLength);

                if (receivedData.Count < tcpMessageDataLength - TCPHeaderLength)
                {
                    startTimestamp = DateTime.UtcNow;

                    while (DateTime.UtcNow.Subtract(startTimestamp).TotalMilliseconds < timeout && receivedData.Count < tcpMessageDataLength)
                    {
                        Memory<byte> buffer = new byte[1024];
                        TimeSpan receiveTimeout = TimeSpan.FromMilliseconds(timeout).Subtract(DateTime.UtcNow.Subtract(startTimestamp));

                        if (receiveTimeout.TotalMilliseconds >= 50)
                        {
                            int receivedBytes = await _client.ReceiveAsync(buffer, receiveTimeout, cancellationToken);

                            if (receivedBytes > 0)
                            {
                                receivedData.AddRange(buffer.Slice(0, receivedBytes).ToArray());
                            }

                            result.Bytes += receivedBytes;
                            result.Packets += 1;
                        }
                    }
                }

                if (receivedData.Count == 0)
                {
                    throw new UnitronicsException("Failed to Receive " + protocol + " Message from Unitronics PLC  '" + RemoteHost + ":" + Port + "' - No Data was Received after TCP Header");
                }

                if (receivedData.Count < tcpMessageDataLength - TCPHeaderLength)
                {
                    throw new UnitronicsException("Failed to Receive " + protocol + " Message within the Timeout Period from Unitronics PLC  '" + RemoteHost + ":" + Port + "'");
                }

                result.Message = receivedData.ToArray();
            }
            catch (ObjectDisposedException)
            {
                throw new UnitronicsException("Failed to Receive " + protocol + " Message from Unitronics PLC '" + RemoteHost + ":" + Port + "' - The underlying Socket Connection has been Closed");
            }
            catch (TimeoutException)
            {
                throw new UnitronicsException("Failed to Receive " + protocol + " Message within the Timeout Period from Unitronics PLC  '" + RemoteHost + ":" + Port + "'");
            }
            catch (System.Net.Sockets.SocketException e)
            {
                throw new UnitronicsException("Failed to Receive " + protocol + " Message from Unitronics PLC  '" + RemoteHost + ":" + Port + "'", e);
            }

            return result;
        }

        private ReadOnlyMemory<byte> buildTcpMessage(ReadOnlyMemory<byte> message, ProtocolType protocol)
        {
            List<byte> tcpMessage = new List<byte>();

            // Transaction Identifier
            tcpMessage.AddRange(BitConverter.GetBytes(getNextRequestId()));

            // Protocol Identifier
            tcpMessage.Add((byte)protocol);

            // Padding Byte
            tcpMessage.Add(0);

            // Command Length
            tcpMessage.AddRange(BitConverter.GetBytes(Convert.ToUInt16(message.Length)).Reverse());

            // Add Command Message
            tcpMessage.AddRange(message.ToArray());

            return tcpMessage.ToArray();
        }

        private ushort getNextRequestId()
        {
            if (_requestId == ushort.MaxValue)
            {
                _requestId = ushort.MinValue;
            }
            else
            {
                _requestId++;
            }

            return _requestId;
        }

        #endregion
    }
}
