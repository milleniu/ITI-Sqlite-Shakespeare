using System;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ITI.Sqlite.Shakespeare.Database;

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
            var tiradeHandler = new VerseHandler( connection, transaction );
            tiradeHandler.Start();

            var sb = new StringBuilder();
            while( _reader.MoveNextRecord() )
            {
                _reader.MoveNextValue();
                var id = _reader.Current;

                _reader.MoveNextValue();
                var pieceId = await pieceHandler.GetPieceId( _reader.Current.ToString() );

                _reader.MoveNextValue();
                var tiradeId = _reader.Current;

                _reader.MoveNextValue();
                var textIdentifier = _reader.Current;

                _reader.MoveNextValue();
                var characterId = await characterHandler.GetCharacterId( _reader.Current.ToString() );

                _reader.MoveNextValue();
                var text = _reader.Current;
            }

            tiradeHandler.Stop();
            tiradeHandler.Finalize( 1_000 );
        }
    }
}
