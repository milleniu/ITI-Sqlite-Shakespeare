using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using ITI.Sqlite.Shakespeare.Models;

namespace ITI.Sqlite.Shakespeare.Database
{
    internal sealed class ParsedLineHandler
    {
        private readonly SQLiteConnection _connection;
        private readonly SQLiteTransaction _transaction;
        private readonly BlockingCollection<ParsedLine> _queue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Thread _processThread;
        private readonly Dictionary<string, int> _characters;
        private readonly Dictionary<string, int> _pieces;

        private volatile int _stopFlag;
        private volatile bool _forceClose;

        public bool IsRunning => _stopFlag == 0;
        public CancellationToken StopToken => _cancellationTokenSource.Token;

        public ParsedLineHandler( SQLiteConnection connection, SQLiteTransaction transaction )
        {
            _connection = connection ?? throw new ArgumentNullException( nameof( connection ) );
            _transaction = transaction ?? throw new ArgumentNullException( nameof( transaction ) );
            _queue = new BlockingCollection<ParsedLine>();
            _cancellationTokenSource = new CancellationTokenSource();
            _processThread = new Thread( Process ) { IsBackground = true };
            _characters = new Dictionary<string, int>();
            _pieces = new Dictionary<string, int>();
        }

        public void Start()
        {
            _processThread.Start();
        }

        public void Handle( in ParsedLine item )
        {
            if( _stopFlag == 0 ) _queue.Add( item, StopToken );
        }

        private void Process()
        {
            var taskList = new List<Task>();
            var tiradeId = int.MinValue;
            var actualTiradeId = int.MinValue;

            while( !_queue.IsCompleted && !_forceClose )
                if( _queue.TryTake( out var item, 10 ) )
                {
                    int? act;
                    int? scene;
                    int? verse;

                    var characterId = GetCharacterId( new string( item.Character.Span ) );
                    var pieceId = GetPieceId( new string( item.Piece.Span ) );

                    if( item.TiradeInfo.IsEmpty )
                    {
                        act = null;
                        scene = null;
                        verse = null;
                    }
                    else
                    {
                        var info = new string( item.TiradeInfo.Span ).Split( '.' );
                        act = int.Parse( info[0] );
                        scene = int.Parse( info[1] );
                        verse = int.Parse( info[2] );
                    }

                    var verseTiradeId = item.Tirade.IsEmpty ? -1 : int.Parse( item.Tirade.Span );
                    if( verseTiradeId != tiradeId )
                    {
                        tiradeId = verseTiradeId;
                        actualTiradeId = GetTiradeId( pieceId, characterId, act, scene );
                    }
                    var id = actualTiradeId;

                    Debug.Assert( id != int.MinValue );

                    var verseId = int.Parse( item.Verse.Span );
                    var task = Task.Run( () => InsertVerse( verseId, pieceId, id, verse, item.Text ), StopToken );
                    taskList.Add( task );
                }

            Task.WaitAll( taskList.ToArray(), StopToken );
        }

        private int GetCharacterId( string character )
        {
            if( _characters.TryGetValue( character, out var characterId ) ) return characterId;

            var insertedCharacterId = _connection.ExecuteScalar<int>
            (
                @"
                    insert into personnages ( nom_personnage ) values( @Name );
                    select last_insert_rowid();",
                new {Name = character},
                _transaction
            );

            _characters.Add( character, insertedCharacterId );
            return insertedCharacterId;
        }

        private int GetPieceId( string title )
        {
            if( _pieces.TryGetValue( title, out var pieceId ) ) return pieceId;

            var insertedPieceId = _connection.ExecuteScalar<int>
            (
                @"
                    insert into pieces ( titre ) values( @Title );
                    select last_insert_rowid();",
                new { Title = title },
                _transaction
            );

            _pieces.Add( title, insertedPieceId );
            return insertedPieceId;
        }

        private int GetTiradeId( int pieceId, int characterId, int? act, int? scene )
            => _connection.ExecuteScalar<int>
            (
                @"
                    insert into tirades ( id_piece, id_personnage, acte, scene ) values( @PieceId, @CharacterId, @Act, @Scene );
                    select last_insert_rowid();",
                new { PieceId = pieceId, CharacterId = characterId, Act = act, Scene = scene },
                _transaction
            );

        private async Task InsertVerse( int verseId, int pieceId, int tiradeId, int? verse, ReadOnlyMemory<char> text )
            => await _connection.ExecuteAsync
            (
                @"
                    insert into texte( id, id_piece, id_tirade, numero_vers, texte )
                        values( @VerseId, @PieceId, @TiradeId, @VerseId, @Text );
                ",
                new { VerseId = verseId, PieceId = pieceId, TiradeId = tiradeId, Verse = verse, Text = new string( text.Span ) },
                _transaction
            );

        public bool Stop()
        {
            if( Interlocked.Exchange( ref _stopFlag, 1 ) == 0 )
            {
                _queue.CompleteAdding();
                return true;
            }

            return false;
        }

        public void Finalize( int ms )
        {
            if( !_processThread.Join( ms ) ) _forceClose = true;

            _cancellationTokenSource.Cancel();
            _processThread.Join();
            _queue.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
