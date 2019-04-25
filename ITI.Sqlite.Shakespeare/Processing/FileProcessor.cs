using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITI.Sqlite.Shakespeare.Database;
using ITI.Sqlite.Shakespeare.Models;

namespace ITI.Sqlite.Shakespeare.Processing
{
    internal class FileProcessor
    {
        private DatReader _reader;

        public FileProcessor()
        {
            _reader = null;
        }

        public async Task LoadFile( string filePath )
        {
            if( filePath == null ) throw new ArgumentNullException( nameof( filePath ) );
            if( !File.Exists( filePath ) ) throw new ArgumentException( nameof( filePath ) );

            var content = await File.ReadAllTextAsync( filePath, Encoding.UTF8 );
            _reader = new DatReader( content );
        }

        public void ProcessFile( SQLiteConnection connection, SQLiteTransaction transaction )
        {
            if( connection == null ) throw new ArgumentNullException( nameof( connection ) );
            if( _reader == null ) throw new InvalidOperationException( "You must call LoadFile beforehand" );

            var verseHandler = new ParsedLineHandler( connection, transaction );
            verseHandler.Start();

            while( _reader.MoveNextRecord() )
            {
                _reader.MoveNextValue();
                var verse = _reader.Current;

                _reader.MoveNextValue();
                var piece = _reader.Current;

                _reader.MoveNextValue();
                var tirade = _reader.Current;

                _reader.MoveNextValue();
                var tiradeInfo = _reader.Current;

                _reader.MoveNextValue();
                var character = _reader.Current;

                _reader.MoveNextValue();
                var text = _reader.Current;

                verseHandler.Handle( new ParsedLine( verse, piece, tirade, tiradeInfo, character, text ) );
            }

            verseHandler.Stop();
            verseHandler.Finalize( 3_600_000 );
        }
    }
}
