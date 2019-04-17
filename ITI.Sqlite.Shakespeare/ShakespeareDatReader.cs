using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace ITI.Sqlite.Shakespeare
{
    public sealed class ShakespeareDatReader
    {
        private readonly Stream _stream;
        private readonly string _valueDelimiter;
        private readonly string _recordDelimiter;
        private readonly Decoder _decoder;
        private readonly Span<char> _buffer;

        private bool _isInRecord;

        public ReadOnlyMemory<char> Current { get; private set; }

        public ShakespeareDatReader
        (
            Stream stream,
            string valueDelimiter = "|",
            string recordDelimiter = "\n",
            Decoder decoder = null
        )
        {
            _stream = stream;
            _valueDelimiter = valueDelimiter;
            _recordDelimiter = recordDelimiter;
            _decoder = decoder ?? Encoding.UTF8.GetDecoder();
            _buffer = new Span<char>();
        }

        public bool MoveNextRecord()
        {
            if( _isInRecord )
                while( MoveNextValue() ) { }

            if( !_isInRecord )
                _isInRecord = true;

            return true;
        }

        public bool MoveNextValue()
        {
            if( !_isInRecord )
            {
                Current = ReadOnlyMemory<char>.Empty;
                return false;
            }

            var valueDelimiter = _valueDelimiter.AsSpan();
            var recordDelimiter = _recordDelimiter.AsSpan();

            while( true )
            {
                var b = _stream.ReadByte();
                if( b == -1 ) break;

                if( _decoder.GetChars(new ReadOnlySpan<byte>(new []{ (byte)b }), _buffer, true ) == 0 )
                    continue;

                if( IsAtDelimiter(_buffer, valueDelimiter) )
                {
                    _isInRecord = true;
                    return true;
                }

                if( IsAtDelimiter( _buffer, recordDelimiter ) )
                {
                    _isInRecord = false;
                    return true;
                }
            }

            Current = _memory;
            _isInRecord = false;
            return true;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static bool IsAtDelimiter( ReadOnlySpan<char> span, ReadOnlySpan<char> delimiter )
            => span[0] == delimiter[0];
    }
}
