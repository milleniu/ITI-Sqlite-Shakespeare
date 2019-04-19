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

        public async Task ProcessFile( SQLiteConnection connection, SQLiteTransaction transaction )
        {
            if( connection == null ) throw new ArgumentNullException( nameof( connection ) );
            if( _reader == null ) throw new InvalidOperationException( "You must call LoadFile beforehand" );

            var pieceHandler = new PieceHandler( connection, transaction );
            var characterHandler = new CharacterHandler( connection, transaction );
            var tiradeHandler = new TiradeHandler( connection, transaction );
            var verseHandler = new VerseHandler( connection, transaction );
            verseHandler.Start();

            var currentTiradeId = int.MinValue;
            var tiradeId = int.MinValue;

            while( _reader.MoveNextRecord() )
            {
                _reader.MoveNextValue();
                var verseId = int.Parse( _reader.Current.Span );

                _reader.MoveNextValue();
                var pieceId = await pieceHandler.GetPieceId( _reader.Current.ToString() );

                _reader.MoveNextValue();
                var parsedTiradeId = _reader.Current.IsEmpty ? -1 : int.Parse( _reader.Current.Span );

                _reader.MoveNextValue();
                int? act;
                int? scene;
                int? verse;
                if( _reader.Current.IsEmpty )
                {
                    act = null;
                    scene = null;
                    verse = null;
                }
                else
                {
                    var composite = _reader.Current.ToString().Split( '.' ).ToArray();
                    act = int.Parse( composite[0] );
                    scene = int.Parse( composite[1] );
                    verse = int.Parse( composite[2] );
                }

                _reader.MoveNextValue();
                var characterId = await characterHandler.GetCharacterId( _reader.Current.ToString() );

                _reader.MoveNextValue();
                var text = _reader.Current.ToString();

                if( parsedTiradeId != currentTiradeId )
                {
                    currentTiradeId = parsedTiradeId;
                    tiradeId = await tiradeHandler.GenerateTirade( pieceId, characterId, act, scene );
                }

                Debug.Assert( tiradeId != int.MinValue );
                verseHandler.Handle( new ParsedVerse( verseId, pieceId, tiradeId, verse, text ) );
            }

            verseHandler.Stop();
            verseHandler.Finalize( 3_600_000 );
        }
    }
}
