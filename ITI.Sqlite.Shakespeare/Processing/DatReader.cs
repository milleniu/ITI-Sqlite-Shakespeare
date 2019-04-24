using System;
using System.Runtime.CompilerServices;

namespace ITI.Sqlite.Shakespeare.Processing
{
    internal sealed class DatReader
    {
        private const string ValueDelimiter = "|";
        private const string RecordDelimiter = "\n";

        private ReadOnlyMemory<char> _memory;
        private int _index;
        private bool _isInRecord;

        public ReadOnlyMemory<char> Current { get; private set; }
        public int CurrentIndex { get; private set; }

        public DatReader( string input )
            : this( input.AsMemory() )
        {
        }

        public DatReader( ReadOnlyMemory<char> memory )
        {
            _memory = memory;
        }

        public bool MoveNextRecord()
        {
            if( _memory.IsEmpty )
            {
                Current = ReadOnlyMemory<char>.Empty;
                CurrentIndex = -1;
                return false;
            }

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
                CurrentIndex = -1;
                return false;
            }

            var span = _memory.Span;
            var valueDelimiterSpan = ValueDelimiter.AsSpan();
            var recordDelimiterSpan = RecordDelimiter.AsSpan();

            var length = span.Length;
            var i = 0;

            while( i < length )
            {
                if( IsAtDelimiter( span, valueDelimiterSpan, i ) )
                {
                    Move( i );
                    _isInRecord = true;
                    return true;
                }

                if( IsAtDelimiter( span, recordDelimiterSpan, i ) )
                {
                    Move( i );
                    _isInRecord = false;
                    return true;
                }

                ++i;
            }

            Current = _memory;
            CurrentIndex = _index;
            _memory = ReadOnlyMemory<char>.Empty;
            _index = -1;
            _isInRecord = false;
            return true;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static bool IsAtDelimiter( ReadOnlySpan<char> span, ReadOnlySpan<char> delimiter, int i )
            => span[i] == delimiter[0];

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void Move( int delimiterPosition )
        {
            Current = _memory.Slice( 0, delimiterPosition );
            CurrentIndex = _index;

            var moveAmount = delimiterPosition + 1;
            _memory = _memory.Slice( moveAmount );
            _index += moveAmount;
        }
    }
}
