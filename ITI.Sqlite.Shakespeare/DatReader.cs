using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ITI.Sqlite.Shakespeare
{
    internal sealed class DatReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly char[] _buffer;

        private bool _isInRecord;
        private int _bufferLength;

        private const byte ValueDelimiter = (byte)'|';
        private const byte RecordDelimiter = (byte)'\n';
        
        public ReadOnlyMemory<char> Current { private set; get; }

        public DatReader( Stream stream )
        {
            _stream = stream;
            _buffer = new char[2048];
            _bufferLength = 0;
        }

        public bool MoveNextRecord()
        {
            if( _stream.Position == _stream.Length )
            {
                Current = ReadOnlyMemory<char>.Empty;
                return false;
            }

            if( _isInRecord )
                while( MoveNextValue() );

            if( !_isInRecord )
                _isInRecord = true;

            return true;
        }

        public bool MoveNextValue()
        {
            while( true )
            {
                var b = _stream.ReadByte();
                if( b != -1 )
                {
                    switch (b)
                    {
                        case ValueDelimiter:
                            _isInRecord = true;
                            ProcessBuffer();
                            return false;

                        case RecordDelimiter:
                            _isInRecord = false;
                            ProcessBuffer();
                            return true;

                        default:
                            _buffer[_bufferLength++] = (char)b;
                            break;
                    }
                }
                else
                {
                    ProcessBuffer();
                    _isInRecord = false;
                    return true;
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void ProcessBuffer()
        {
            if( _bufferLength == 0 ) Current = ReadOnlyMemory<char>.Empty;
            else
            {
                Current = new ReadOnlyMemory<char>( _buffer, 0, _bufferLength );
                _bufferLength = 0;
            }
        }

        public void Dispose()
        {
            if( _stream == null ) return;
            _stream.Flush();
            _stream.Dispose();
        }
    }
}
