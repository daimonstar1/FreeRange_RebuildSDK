using System;
using System.IO;
using UnityEngine;


namespace FRG.Core
{
    /// <summary>
    /// A wrapping stream that can be recycled, switch out its sink, and can optionally not cascade close calls.
    /// A using block will cause it to despawn.
    /// </summary>
    public class RedirectionStream : Stream, IRecyclable, IDisposable
    {
        public static readonly Action<object> CloseStream = stream => ((Stream)stream).Close();

        Pooled<Stream> _baseStream;

        public Stream BaseStream { get { return _baseStream.Value; } }

        #region Readers and Writers

        private StreamReader _streamReader;
        private StreamWriter _streamWriter;
        private BinaryReader _binaryReader;
        private BinaryWriter _binaryWriter;

        public StreamReader StreamReader
        {
            get
            {
                // Shouldn't matter if double-initialized due to memory barrier issues
                if (_streamReader == null)
                {
                    _streamReader = new ReusableStreamReader(this);
                    GC.SuppressFinalize(_streamReader);
                }
                return _streamReader;
            }
        }

        public StreamWriter StreamWriter
        {
            get
            {
                // Shouldn't matter if double-initialized due to memory barrier issues
                if (_streamWriter == null)
                {
                    // AutoFlush means we don't have to worry about half-written strings,
                    // though it is slightly less efficient
                    _streamWriter = new ReusableStreamWriter(this);
                }
                return _streamWriter;
            }
        }

        public BinaryReader BinaryReader
        {
            get
            {
                // Shouldn't matter if double-initialized due to memory barrier issues
                if (_binaryReader == null)
                {
                    _binaryReader = new ReusableBinaryReader(this);
                }
                return _binaryReader;
            }
        }

        public BinaryWriter BinaryWriter
        {
            get
            {
                // Shouldn't matter if double-initialized due to memory barrier issues
                if (_binaryWriter == null)
                {
                    _binaryWriter = new ReusableBinaryWriter(this);
                }
                return _binaryWriter;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Create a wrapper stream that has no base stream. It is invalid until you call <see cref="ResetStream"/>.
        /// </summary>
        public RedirectionStream()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Reset the object for reuse, if possible.
        /// </summary>
        bool IRecyclable.Recycle()
        {
            ResetStream();
            return true;
        }

        public override void Close()
        {
            // Do nothing.
        }

        protected override void Dispose(bool disposing)
        {
            // Do nothing
        }
        
        public void ResetStream()
        {
            ResetStream(null, null);
        }

        /// <summary>
        /// Resets the current stream, closing the previous stream if there was one and it was set to cascade close.
        /// Will always close the previous stream, even if the new stream is the same stream.
        /// </summary>
        public void ResetStream(Stream baseStream, Action<object> destructor)
        {
            TransferStream(new Pooled<Stream>(baseStream, destructor));
        }

        /// <summary>
        /// Resets the current stream and transfers ownership.
        /// </summary>
        public void TransferStream(Pooled<Stream> disownedBaseStream)
        {
            _baseStream.Dispose();

            _baseStream = disownedBaseStream;
        }

        #endregion

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            RequireStream();
            return BaseStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            RequireStream();
            return BaseStream.BeginWrite(buffer, offset, count, callback, state);
        }

        #region BaseStream Properties

        public override bool CanRead
        {
            get
            {
                RequireStream();
                return BaseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                RequireStream();
                return BaseStream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                RequireStream();
                return BaseStream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                RequireStream();
                return BaseStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                RequireStream();
                return BaseStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                RequireStream();
                return BaseStream.Position;
            }

            set
            {
                RequireStream();
                BaseStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                RequireStream();
                return BaseStream.ReadTimeout;
            }

            set
            {
                RequireStream();
                BaseStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                RequireStream();
                return BaseStream.WriteTimeout;
            }

            set
            {
                RequireStream();
                BaseStream.WriteTimeout = value;
            }
        }

        #endregion

        #region BaseStream Methods

        public override int EndRead(IAsyncResult asyncResult)
        {
            RequireStream();
            return BaseStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            RequireStream();
            BaseStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            RequireStream();
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            RequireStream();
            return BaseStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            RequireStream();
            return BaseStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            RequireStream();
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            RequireStream();
            BaseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            RequireStream();
            BaseStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            RequireStream();
            BaseStream.WriteByte(value);
        }



        #endregion

        public override string ToString()
        {
            return GetType().CSharpName() + "{" + (BaseStream != null ? BaseStream.ToString() : "<null>") + "}";
        }

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        private void RequireStream()
        {
            Debug.Assert(BaseStream != null);
        }

        internal sealed class ReusableStreamReader : StreamReader
        {
            public ReusableStreamReader(Stream baseStream)
                : base(baseStream)
            {
                // Finalizer is useless
                GC.SuppressFinalize(this);
            }

            public override void Close()
            {
            }

            protected override void Dispose(bool disposing)
            {
            }
        }

        internal sealed class ReusableStreamWriter : StreamWriter
        {
            public ReusableStreamWriter(Stream baseStream)
                : base(baseStream)
            {
                AutoFlush = true;

                // Finalizer is useless
                GC.SuppressFinalize(this);
            }

            public override void Close()
            {
            }

            protected override void Dispose(bool disposing)
            {
            }
        }

        internal sealed class ReusableBinaryReader : BinaryReader
        {
            public ReusableBinaryReader(Stream baseStream)
                : base(baseStream)
            {
                // Finalizer is useless
                GC.SuppressFinalize(this);
            }

            public override void Close()
            {
            }

            protected override void Dispose(bool disposing)
            {
            }
        }

        internal sealed class ReusableBinaryWriter : BinaryWriter
        {
            public ReusableBinaryWriter(Stream baseStream)
                : base(baseStream)
            {
                // Finalizer is useless
                GC.SuppressFinalize(this);
            }

            public override void Close()
            {
            }

            protected override void Dispose(bool disposing)
            {
            }
        }
    }
}