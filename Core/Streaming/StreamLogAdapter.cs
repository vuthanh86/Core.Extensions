using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Streaming
{
    public class StreamAdapter : Stream
    {
        private readonly Stream source;
        private readonly Stream destination;

        public StreamAdapter(Stream source,Stream destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readed = source.Read(buffer, offset, count);

            if (readed > 0)
            {
                destination.Write(buffer, offset, readed);
            }

            return readed;
        }

        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var readed = await source.ReadAsync(buffer, offset, count, cancellationToken);

            if (readed > 0)
            {
                await destination.WriteAsync(buffer, offset, readed, cancellationToken);
            }

            return readed;
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => source.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => source.Length;

        public override long Position
        {
            get { return source.Length; }
            set { throw new NotSupportedException(); }
        }
    }
}
