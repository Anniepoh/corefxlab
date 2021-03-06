﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace System.Text.Utf8
{
    partial struct Utf8String
    {
        public struct Enumerator : IEnumerator<Utf8CodeUnit>, IEnumerator
        {
            // TODO: This object is heavier than string itself... Once we got ByteSpan runtime support, change it.
            // TODO: Reduce number of members when we get Span<byte> runtime support
            private ByteSpan _startBuffer;
            private ByteSpan _buffer;

            private byte[] _bytes;
            private int _startIndex;
            private int _index;
            private int _lastIndex;

            public Enumerator(byte[] bytes, int index, int length)
            {
                if (index + length > bytes.Length)
                    throw new ArgumentOutOfRangeException("index");

                _startBuffer = default(ByteSpan);
                _buffer = default(ByteSpan);

                _bytes = bytes;
                // first MoveNext moves to a first byte
                _startIndex = index - 1;
                _index = _startIndex;
                _lastIndex = index + length;
            }

            public unsafe Enumerator(ByteSpan buffer)
            {
                if (buffer.UnsafeBuffer == null)
                    throw new ArgumentNullException("buffer");

                // first MoveNext moves to a first byte
                _startBuffer = new ByteSpan(buffer.UnsafeBuffer - 1, buffer.Length + 1);
                _buffer = _startBuffer;

                _bytes = default(byte[]);
                _startIndex = default(int);
                _index = default(int);
                _lastIndex = default(int);
            }


            object IEnumerator.Current { get { return Current; } }

            public unsafe Utf8CodeUnit Current
            {
                get
                {
                    if (_bytes != null)
                    {
                        if (_index == _startIndex)
                        {
                            throw new InvalidOperationException("MoveNext() needs to be called at least once");
                        }

                        return (Utf8CodeUnit)_bytes[_index];
                    }
                    else
                    {
                        if (_buffer.UnsafeBuffer == _startBuffer.UnsafeBuffer)
                        {
                            throw new InvalidOperationException("MoveNext() needs to be called at least once");
                        }
                        return (Utf8CodeUnit)_buffer[0];
                    }
                }
            }

            void IDisposable.Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_bytes != null)
                {
                    _index++;
                    return _index < _lastIndex;
                }
                else
                {
                    _buffer = _buffer.Slice(1);
                    return _buffer.Length > 0;
                }
            }

            public void Reset()
            {
                _index = _startIndex;
                _buffer = _startBuffer;
            }
        }
    }
}
