using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace ITI.Sqlite.Shakespeare
{
    internal sealed class FileProcessor : IDisposable
    {
        private readonly SQLiteConnection _connection;
        private readonly SQLiteTransaction _transaction;
        private readonly DatReader _reader;
        private readonly Dictionary<string, int> _characters;
        private readonly Dictionary<string, int> _pieces;

        public FileProcessor
        (
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            Stream fileStream
        )
        {
            _connection = connection ?? throw new ArgumentNullException( nameof( connection ) );
            _transaction = transaction ?? throw new ArgumentNullException( nameof( transaction ) );
            _reader = new DatReader( fileStream ?? throw new ArgumentNullException( nameof( fileStream ) ) );
            _characters = new Dictionary<string, int>();
            _pieces = new Dictionary<string, int>();
        }

        public async Task ProcessFile()
        {
            var verseHandler = new VerseHandler( _connection, _transaction );
            verseHandler.Start();

            var currentTiradeId = int.MinValue;
            var tiradeId = int.MinValue;

            while( _reader.MoveNextRecord() )
            {
                _reader.MoveNextValue();
                var verseId = int.Parse( _reader.Current.Span );

                _reader.MoveNextValue();
                var pieceId = await GetPieceId( new string( _reader.Current.Span ) );

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
                    var composite = new string( _reader.Current.Span ).Split( '.' ).ToArray();
                    act = int.Parse( composite[0] );
                    scene = int.Parse( composite[1] );
                    verse = int.Parse( composite[2] );
                }

                _reader.MoveNextValue();
                var characterId = await GetCharacterId( new string( _reader.Current.Span ) );

                _reader.MoveNextValue();
                var text = new string( _reader.Current.Span );

                if( parsedTiradeId != currentTiradeId )
                {
                    currentTiradeId = parsedTiradeId;
                    tiradeId = await InsertTirade( pieceId, characterId, act, scene );
                }

                Debug.Assert( tiradeId != int.MinValue );
                verseHandler.Handle( new ParsedVerse( verseId, pieceId, tiradeId, verse, text ) );
            }

            verseHandler.Stop();
            verseHandler.Finalize( 3_600_000 );
        }

        public async ValueTask<int> GetPieceId( string title )
        {
            if( _pieces.TryGetValue( title, out var id ) ) return id;

            var pieceId = await _connection.ExecuteScalarAsync<int>
            (
                @"
                    insert into pieces ( titre ) values( @Title );
                    select last_insert_rowid();",
                new {Title = title},
                _transaction
            );

            _pieces.Add( title, pieceId );
            return pieceId;
        }

        private async ValueTask<int> GetCharacterId( string character )
        {
            if( _characters.TryGetValue( character, out var id ) ) return id;

            var characterId = await _connection.ExecuteScalarAsync<int>
            (
                @"
                    insert into personnages ( nom_personnage ) values( @Name );
                    select last_insert_rowid();",
                new { Name = character },
                _transaction
            );

            _characters.Add( character, characterId );
            return characterId;
        }

        private async ValueTask<int> InsertTirade( int pieceId, int characterId, int? act, int? scene )
            => await _connection.ExecuteScalarAsync<int>
                (
                    @"
                        insert into tirades ( id_piece, id_personnage, acte, scene ) values( @PieceId, @CharacterId, @Act, @Scene );
                        select last_insert_rowid();",
                    new { PieceId = pieceId, CharacterId = characterId, Act = act, Scene = scene },
                    _transaction
                );

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
