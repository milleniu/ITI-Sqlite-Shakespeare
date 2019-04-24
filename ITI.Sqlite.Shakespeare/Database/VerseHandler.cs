using System;
using System.Collections.Concurrent;
using System.Data.SQLite;
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
        private readonly ConcurrentDictionary<string, AsyncLazy<int>> _characters;


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
            _characters = new ConcurrentDictionary<string, AsyncLazy<int>>();
        }

        public void Start()
        {
            _processThread.Start();
        }

        public void Handle( in ParsedLine item )
        {
            Console.WriteLine( $"{_stopFlag == 0} {_queue.Count}" );
            if( _stopFlag == 0 ) _queue.Add( item, StopToken );
        }

        private void Process()
        {
            while( !_queue.IsCompleted && !_forceClose )
                if( _queue.TryTake( out var item, 10 ) )
                    Task.Run( () => ProcessItem( item ), StopToken );
        }

        private async Task ProcessItem( ParsedLine item )
        {
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
