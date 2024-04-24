using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test;

/// <summary>
/// A <see cref="MemoryStream"/> implementation that will never read more than one byte at a time.
/// </summary>
internal sealed class SingleByteReadingMemoryStream : MemoryStream
{
    public override int Read(byte[] buffer, int offset, int count)
    {
        return base.Read(buffer, offset, Math.Min(1, count));
    }

    public override int Read(Span<byte> buffer)
    {
        return base.Read(buffer[.. Math.Min(1, buffer.Length)]);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return base.ReadAsync(buffer, offset, Math.Min(1, count), cancellationToken);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return base.ReadAsync(buffer[.. Math.Min(1, buffer.Length)], cancellationToken);
    }
}
