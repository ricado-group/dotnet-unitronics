using System;
using System.Threading;
using System.Threading.Tasks;

namespace RICADO.Unitronics.Channels
{
    internal interface IChannel : IDisposable
    {
        public Task InitializeAsync(int timeout, CancellationToken cancellationToken);

        public Task<ProcessMessageResult> ProcessMessageAsync(ReadOnlyMemory<byte> requestMessage, ProtocolType protocol, byte unitId, int timeout, int retries, CancellationToken cancellationToken);
    }
}
