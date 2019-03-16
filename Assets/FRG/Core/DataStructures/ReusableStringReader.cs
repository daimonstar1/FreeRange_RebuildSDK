using System;
using System.IO;
using UnityEngine;


namespace FRG.Core
{
    internal sealed class ReusableStringReader : StringReader, IRecyclable
    {
        private bool _unusable = false;

        private string _target = "";
        private int _position = 0;
        private int _length = 0;

        public ReusableStringReader()
            : base("")
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

        bool IRecyclable.Recycle()
        {
            ResetString("");
            return !_unusable;
        }

        public void ResetString(string value)
        {
            value = value ?? "";

            _target = value;
            _position = 0;
            _length = value.Length;
        }

        public void ResetString(string value, int startPosition, int length)
        {
            value = value ?? "";
            if (startPosition < 0 || startPosition > value.Length) { throw new ArgumentOutOfRangeException("startPosition is out of range.", startPosition, "startPosition"); }
            if (length < 0 || startPosition + length > value.Length) { throw new ArgumentOutOfRangeException("length is out of range.", length, "length"); }

            _target = value;
            _position = startPosition;
            _length = length;
        }

        public override int Peek()
        {
            if (_position >= _length)
            {
                return -1;
            }
            return _target[_position];
        }

        public override int Read()
        {
            if (_position >= _length)
            {
                return -1;
            }
            char c = _target[_position];
            _position += 1;
            return c;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            Debug.Assert(index >= 0 && index <= buffer.Length);
            Debug.Assert(count >= 0 && index + count <= buffer.Length);

            int total = Math.Min(count, _length - _position);
            _target.CopyTo(_position, buffer, index, total);
            _position += total;
            return total;
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            return Read(buffer, index, count);
        }

        public override string ReadLine()
        {
            for (int i = _position; i < _length; ++i)
            {
                char c = _target[i];
                if (c == '\r' || c == '\n')
                {
                    string innerResult = _target.Substring(_position, i - _position);
                    if (c == '\r' && i < _length && _target[i] == '\n')
                    {
                        _position = i + 2;
                    }
                    else
                    {
                        _position = i + 1;
                    }
                    return innerResult;
                }
            }

            return ReadToEnd();
        }

        public override string ReadToEnd()
        {
            string result = _target.Substring(_position, _length - _position);
            _position = _length;
            return result;
        }

#if GAME_SERVER
        public override Task<int> ReadAsync(char[] buffer, int index, int count)
        {
            return Task.FromResult<int>(Read(buffer, index, count));
        }

        public override Task<int> ReadBlockAsync(char[] buffer, int index, int count)
        {
            return Task.FromResult<int>(ReadBlock(buffer, index, count));
        }

        public override Task<string> ReadLineAsync()
        {
            return Task.FromResult<string>(ReadLine());
        }

        public override Task<string> ReadToEndAsync()
        {
            return Task.FromResult<string>(ReadToEnd());
        }
#endif
    }
}
