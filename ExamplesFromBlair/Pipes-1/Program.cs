using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// See Reader.cs and Writer.cs to see what this example is doing.

namespace Pipes
{
    // This file is not germane to the example being offered. Instead, it starts up two separate tasks and
    // runs each of our simulated processes in each one. The Writer supplies information through its Out,
    // and the Reader receives this information through it's In, to process it.
    //
    // The rest of the code simulates the In and Out properties of the Console class and manage starting
    // and monitoring the tasks representing processes, taking on the role of the Command Prompt command
    // processor.

    class Program
    {
        static void Main(string[] args)
        {
            // Simulated pipe wiring
            CreatePipeEnds(out var @out, out var @in);
            var reader = new Task(() => new Reader(@in).DoSomething());
            var writer = new Task(() => new Writer(@out).DoSomething());
            reader.ContinueWith(t => @in.Dispose());
            writer.ContinueWith(t => @out.Dispose());

            // pipe endpoint task management
            var tasks = new[] { reader, writer };
            Array.ForEach(tasks, t => t.Start());
            Task.WaitAll(tasks);
        }

        private static void CreatePipeEnds(out TextWriter @out, out TextReader @in)
        {
            var str = new SimulatedPipe();
            @out = TextWriter.Synchronized(new StreamWriter(new SimPipeStream(str, true), Encoding.UTF8, 256) { AutoFlush = true });
            @in = TextReader.Synchronized(new StreamReader(new SimPipeStream(str, false), Encoding.UTF8, false, 256));
        }

        private class SimulatedPipe
        {
            private readonly object _lock = new object();
            private readonly ConcurrentQueue<byte> _q = new ConcurrentQueue<byte>();
            private readonly AutoResetEvent _dataAvailable = new AutoResetEvent(false);
            private volatile bool _writerIsDisposed = false;
            private volatile bool _readerIsDisposed = false;

            internal void SetWriterDisposed()
            {
                lock (_lock)
                {
                    _writerIsDisposed = true;
                    _dataAvailable.Set();
                }
            }

            internal void SetReaderDisposed()
            {
                lock (_lock) { _readerIsDisposed = true; }
            }

            internal int Read(byte[] buffer, int offset, int count)
            {
                if (buffer == null) { throw new ArgumentNullException(nameof(buffer)); }
                if (offset < 0) { throw new ArgumentOutOfRangeException(nameof(offset)); }
                if (count < 0) { throw new ArgumentOutOfRangeException(nameof(count)); }
                if (offset + count > buffer.Length) { throw new ArgumentException(nameof(buffer)); }

                lock (_lock)
                {
                    if (_readerIsDisposed) { throw new ObjectDisposedException(nameof(SimulatedPipe)); }
                    var i = 0;
                    if (WaitForInput())
                    {
                        while (count > i)
                        {
                            if (_q.TryDequeue(out var c)) { buffer[offset + i++] = c; }
                            else { break; }
                        }
                    }
                    return i;
                }

                bool WaitForInput() // true if data available, false if (empty and EOF)
                {
                    while (!_writerIsDisposed || !_q.IsEmpty)
                    {
                        var timeout = _q.IsEmpty ? Timeout.Infinite : 0;
                        Monitor.Exit(_lock);

                        bool result = false;
                        try { result = _dataAvailable.WaitOne(timeout); }
                        catch (Exception) { Monitor.Enter(_lock); throw; }

                        Monitor.Enter(_lock);
                        if (result) { return true; }
                    }

                    return !_q.IsEmpty;
                }
            }

            internal void Write(byte[] buffer, int offset, int count)
            {
                if (buffer == null) { throw new ArgumentNullException(nameof(buffer)); }
                if (offset < 0) { throw new ArgumentOutOfRangeException(nameof(offset)); }
                if (count < 0) { throw new ArgumentOutOfRangeException(nameof(count)); }
                if (offset + count > buffer.Length) { throw new ArgumentException(nameof(buffer)); }

                lock (_lock)
                {
                    if (_writerIsDisposed) { throw new ObjectDisposedException(nameof(SimulatedPipe)); }
                    if (_readerIsDisposed) { throw new IOException(); } // TODO: something more accurate
                    var i = 0;
                    while (count > i)
                    {
                        _q.Enqueue(buffer[offset + i++]);
                    }
                    if (count > 0) { _dataAvailable.Set(); }
                }
            }
        }

        private class SimPipeStream : Stream
        {
            private readonly bool _isWriter;
            private readonly SimulatedPipe _pipe;

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing)
                {
                    if (_isWriter) { _pipe.SetWriterDisposed(); }
                    else { _pipe.SetReaderDisposed(); }
                }
            }

            internal SimPipeStream(SimulatedPipe pipe, bool isWriter)
            {
                _pipe = pipe;
                _isWriter = isWriter;
            }

            public override bool CanRead => !_isWriter;
            public override bool CanSeek => false;
            public override bool CanWrite => _isWriter;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush() { }

            public override int Read(byte[] buffer, int offset, int count) =>
                _pipe.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) =>
                throw new NotSupportedException();

            public override void SetLength(long value) =>
                throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) =>
                _pipe.Write(buffer, offset, count);
        }
    }
}
