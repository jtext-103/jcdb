using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace WebAPI.Models {
    public class PullStream : Stream {
        private byte[] internalBuffer;
        private bool ended;
        private static ManualResetEvent dataAvailable = new ManualResetEvent(false);
        private static ManualResetEvent dataEmpty = new ManualResetEvent(true);

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override void Flush() {
            throw new NotImplementedException();
        }

        public override long Length {
            get { throw new NotImplementedException(); }
        }

        public override long Position {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            dataAvailable.WaitOne();
            if (count >= internalBuffer.Length) {
                var retVal = internalBuffer.Length;
                Array.Copy(internalBuffer, buffer, retVal);
                internalBuffer = null;
                dataAvailable.Reset();
                dataEmpty.Set();
                return retVal;
            } else {
                Array.Copy(internalBuffer, buffer, count);
                internalBuffer = internalBuffer.Skip(count).ToArray(); // i know
                return count;
            }
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            dataEmpty.WaitOne();
            dataEmpty.Reset();

            internalBuffer = new byte[count];
            Array.Copy(buffer, internalBuffer, count);

            Debug.WriteLine("Writing some data");

            dataAvailable.Set();
        }

        public void End() {
            dataEmpty.WaitOne();
            dataEmpty.Reset();

            internalBuffer = new byte[0];

            Debug.WriteLine("Ending writes");

            dataAvailable.Set();
        }
    }
}