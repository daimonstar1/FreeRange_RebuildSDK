using System;
using System.IO;
using System.Text;


namespace FRG.Core
{
    internal sealed class ReusableStringWriter : StringWriter, IRecyclable, ICapacity//, IPoolInitializable, IPoolInitializable<int>, IPoolInitializable<StringBuilder>
    {
        private static readonly char[] NewLineChars = new char[] { '\n' };

        private bool _unusable = false;
        private StringBuilder _builder;

        public int Capacity { get { return _builder.Capacity; } }

        public ReusableStringWriter()
            : base()
        {
            _builder = base.GetStringBuilder();

            // Finalizer is useless
            GC.SuppressFinalize(this);
        }

        //void IPoolInitializable.PoolInitialize()
        //{
        //    EnsureCapacity(RecyclingPool.MaxBufferCapacity);
        //}

        //void IPoolInitializable<int>.PoolInitialize(int capacity)
        //{
        //    EnsureCapacity(Math.Max(capacity, RecyclingPool.MaxBufferCapacity));
        //}

        //void IPoolInitializable<StringBuilder>.PoolInitialize(StringBuilder builder)
        //{
        //    ResetBuilder(builder);
        //}

        bool IRecyclable.Recycle()
        {
            ResetBuilder();
            CoreNewLine = NewLineChars;
            return !_unusable;
        }

        public override void Close()
        {
        }

        protected override void Dispose(bool disposing)
        {
            _unusable = true;
        }

        public override StringBuilder GetStringBuilder()
        {
            return _builder;
        }

        public void ResetBuilder()
        {
            ResetBuilder(null);
        }

        public void ResetBuilder(StringBuilder builder)
        {
            _builder = builder ?? base.GetStringBuilder();
            _builder.Length = 0;
        }

        public void EnsureCapacity(int capacity)
        {
            _builder.EnsureCapacity(capacity);
        }

        public void TrimExcess()
        {
            _builder.Capacity = _builder.Length;
        }

        public override void Write(char value)
        {
            _builder.Append(value);
        }

        public override void Write(char[] buffer, int index, int length)
        {
            _builder.Append(buffer, index, length);
        }

        public override void Write(string value)
        {
            _builder.Append(value);
        }

        public override void WriteLine()
        {
            _builder.Append(CoreNewLine);
        }

        public override void Write(string format, object arg0)
        {
            _builder.AppendFormat(format, arg0);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            _builder.AppendFormat(format, arg0, arg1);
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            _builder.AppendFormat(format, arg0, arg1, arg2);
        }

        public override void Write(string format, params object[] args)
        {
            _builder.AppendFormat(format, args);
        }

#if GAME_SERVER
        public override Task FlushAsync()
        {
            return Task.CompletedTask;
        }

        public override Task WriteAsync(char value)
        {
            Write(value);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(string value)
        {
            Write(value);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync()
        {
            WriteLine();
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(char value)
        {
            WriteLine(value);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            WriteLine(buffer, index, count);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(string value)
        {
            WriteLine(value);
            return Task.CompletedTask;
        }
#endif
    }
}
