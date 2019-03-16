using System;
using System.IO;
using UnityEngine;

namespace FRG.Core
{
    /// <summary>
    /// A memory stream that spawns via <see cref="RecyclingPool"/>.
    /// </summary>
    public class ReusableMemoryStream : MemoryStream, IRecyclable, ICapacity//, IPoolInitializable, IPoolInitializable<int>, IPoolInitializable<byte[]>, IPoolInitializable<byte[], int, int>, IPoolInitializable<Stream>
    {
        public ReusableMemoryStream()
            : base(0)
        {
            GC.SuppressFinalize(this);
        }

        public ReusableMemoryStream(int capacity)
            : this()
        {
            EnsureCapacity(capacity);
        }

        //void IPoolInitializable.PoolInitialize()
        //{
        //    EnsureCapacity(RecyclingPool.MaxBufferCapacity);
        //}

        //void IPoolInitializable<int>.PoolInitialize(int capacity)
        //{
        //    EnsureCapacity(Math.Max(RecyclingPool.MaxBufferCapacity, capacity));
        //}

        //void IPoolInitializable<byte[]>.PoolInitialize(byte[] buffer)
        //{
        //    SetBuffer(buffer);
        //}

        //void IPoolInitializable<byte[], int, int>.PoolInitialize(byte[] buffer, int startIndex, int length)
        //{
        //    SetBuffer(buffer, startIndex, length);
        //}

        //void IPoolInitializable<Stream>.PoolInitialize(Stream copyStream)
        //{
        //    CopyFrom(copyStream);
        //}
        
        /// <summary>
        /// Override normal behavior of making the stream useless.
        /// </summary>
        public override void Close()
        {
            // Do nothing
        }

        /// <summary>
        /// Override normal behavior of making the stream useless.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            // Do nothing
        }

        public override int Capacity
        {
            get
            {
                return base.Capacity;
            }

            set
            {
                // Alleviate mono bug
                if (value == 0)
                {
                    base.Capacity = 16;
                    return;
                }

                base.Capacity = value;
            }
        }

        public void EnsureCapacity(int capacity)
        {
            Debug.Assert(capacity >= 0);
            Debug.Assert(Length <= int.MaxValue);
            
            if (Capacity < capacity)
            {
                // Instead of tiny incremental increases, double by default
                Capacity = Math.Max(Capacity * 2, capacity);
            }
        }

        public void TrimExcess()
        {
            if (Length <= int.MaxValue)
            {
                Capacity = (int)Length;
            }
        }

        public void ResetStream()
        {
            Seek(0, SeekOrigin.Begin);
            SetLength(0);
        }

        public void SetBuffer(byte[] buffer)
        {
            Debug.Assert(buffer != null);

            SetBuffer(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Clear the current stream contents and set them to the buffer, ready to read from the beginning.
        /// </summary>
        public void SetBuffer(byte[] buffer, int startIndex, int count)
        {
            Debug.Assert(buffer != null);
            Debug.Assert(startIndex >= 0 && startIndex <= buffer.Length);
            Debug.Assert(count >= 0 && startIndex + count <= buffer.Length);

            ResetStream();
            EnsureCapacity(count);
            Write(buffer, startIndex, count);
            Seek(0, SeekOrigin.Begin);
        }

        public void CopyFrom(Stream stream)
        {
            // Fast path
            MemoryStream memoryStream = stream as MemoryStream;
            if (memoryStream != null) {
                SetBuffer(memoryStream.GetBuffer() ?? ArrayUtil.Empty<byte>(), (int)memoryStream.Position, (int)memoryStream.Length - (int)memoryStream.Position);
                return;
            }

            Seek(0, SeekOrigin.Begin);
            int index = 0;
            long length = Length;

            byte[] buffer = GetBuffer() ?? ArrayUtil.Empty<byte>();
            while (true)
            {
                if (length - index <= 0) {
                    if (length >= int.MaxValue) {
                        throw new IOException("Stream is too large to copy.");
                    }
                    length = Math.Max(Math.Min(length * 2, int.MaxValue), 1024);
                    SetLength(length);
                    // May reallocate
                    buffer = GetBuffer() ?? ArrayUtil.Empty<byte>();
                }

                int readAmount = stream.Read(buffer, index, (int)length - index);
                if (readAmount == 0)
                {
                    break;
                }

                index += readAmount;
            }

            SetLength(index);
        }

        /// <summary>
        /// Reset the object for reuse, if possible.
        /// </summary>
        bool IRecyclable.Recycle()
        {
            ResetStream();
            return true;
        }
    }
}