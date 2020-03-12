using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Web;

namespace WebAPI.Models {
    /// <summary>
    /// Sample stream that write how many characters were written to it.
    /// </summary>
    internal class SummaryStream : DelegatingStream {
        public SummaryStream()
            : base(Stream.Null) {
        }

        public override void Flush() {
            Console.WriteLine("Flushing stream...");
            _innerStream.Flush();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            Console.WriteLine("Writing {0} bytes SYNChronously...", count);
            _innerStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value) {
            Console.WriteLine("Writing a single byte SYNChronously...");
            _innerStream.WriteByte(value);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
            Console.WriteLine("Writing {0} bytes ASYNChronously...", count);
            return _innerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close() {
            Console.WriteLine("Closing stream...");
            base.Close();
        }
    }
    /// <summary>
    /// Utility <see cref="Stream"/> which delegates everything to an inner <see cref="Stream"/>
    /// </summary>
    internal abstract class DelegatingStream : Stream {
        protected Stream _innerStream;

        protected DelegatingStream(Stream innerStream) {
            Contract.Assert(innerStream != null);
            _innerStream = innerStream;
        }

        public override bool CanRead {
            get { return _innerStream.CanRead; }
        }

        public override bool CanSeek {
            get { return _innerStream.CanSeek; }
        }

        public override bool CanWrite {
            get { return _innerStream.CanWrite; }
        }

        public override long Length {
            get { return _innerStream.Length; }
        }

        public override long Position {
            get { return _innerStream.Position; }
            set { _innerStream.Position = value; }
        }

        public override int ReadTimeout {
            get { return _innerStream.ReadTimeout; }
            set { _innerStream.ReadTimeout = value; }
        }

        public override bool CanTimeout {
            get { return _innerStream.CanTimeout; }
        }

        public override int WriteTimeout {
            get { return _innerStream.WriteTimeout; }
            set { _innerStream.WriteTimeout = value; }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _innerStream.Dispose();
            }
            base.Dispose(disposing);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return _innerStream.Seek(offset, origin);
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return _innerStream.Read(buffer, offset, count);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
            return _innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult) {
            return _innerStream.EndRead(asyncResult);
        }

        public override int ReadByte() {
            return _innerStream.ReadByte();
        }

        public override void Flush() {
            _innerStream.Flush();
        }

        public override void SetLength(long value) {
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            _innerStream.Write(buffer, offset, count);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
            return _innerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult) {
            _innerStream.EndWrite(asyncResult);
        }

        public override void WriteByte(byte value) {
            _innerStream.WriteByte(value);
        }
    }
}