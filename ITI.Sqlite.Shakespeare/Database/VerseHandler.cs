using System;
using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Threading;
using Dapper;
using ITI.Sqlite.Shakespeare.Models;

namespace ITI.Sqlite.Shakespeare.Database
{
    internal sealed class VerseHandler
    {
        private readonly SQLiteConnection _connection;
        private readonly SQLiteTransaction _transaction;
        private readonly BlockingCollection<ParsedVerse> _queue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Thread _processThread;

        private volatile int _stopFlag;
        private volatile bool _forceClose;

        public bool IsRunning => _stopFlag == 0;
        public CancellationToken StopToken => _cancellationTokenSource.Token;

        public VerseHandler( SQLiteConnection connection, SQLiteTransaction transaction )
        {
            _connection = connection ?? throw new ArgumentNullException( nameof( connection ) );
            _transaction = transaction ?? throw new ArgumentNullException( nameof( transaction ) );
            _queue = new BlockingCollection<ParsedVerse>();
            _cancellationTokenSource = new CancellationTokenSource();
            _processThread = new Thread( Process ) { IsBackground = true };
        }

        public void Start()
        {
            _processThread.Start();
        }

        public void Handle( in ParsedVerse item )
        {
            if( _stopFlag == 0 ) _queue.Add( item, StopToken );
        }

        private void Process()
        {
            while( !_queue.IsCompleted && !_forceClose)
            {
                if( _queue.TryTake( out var item, 10 ) )
                {
                    ProcessItem( item );
                }
            }
        }

        private void ProcessItem( in ParsedVerse item )
        {
            _connection.Execute
            (
                @"
                    insert into texte( id, id_piece, id_tirade, numero_vers, texte )
                        values( @VerseId, @PieceId, @TiradeId, @VerseId, @Text );
                ",
                new { item.VerseId, item.PieceId, item.TiradeId, item.Verse, item.Text },
                _transaction
            );
        }

        public bool Stop()
        {
            if( Interlocked.Exchange( ref _stopFlag, 1 ) == 0 )
            {
                _cancellationTokenSource.Cancel();
                _queue.CompleteAdding();
                return true;
            }

            return false;
        }

        public void Finalize( int ms )
        {
            if( !_processThread.Join( ms ) ) _forceClose = true;

            _processThread.Join();
            _queue.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
