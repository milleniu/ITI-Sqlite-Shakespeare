using System;
using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Threading.Tasks;
using Dapper;

namespace ITI.Sqlite.Shakespeare.Database
{
    internal sealed class PieceHandler
    {
        private readonly SQLiteConnection _connection;
        private readonly SQLiteTransaction _transaction;
        private readonly ConcurrentDictionary<string, int> _dictionary;

        public PieceHandler( SQLiteConnection connection, SQLiteTransaction transaction )
        {
            _connection = connection ?? throw new ArgumentNullException( nameof( connection ) );
            _transaction = transaction ?? throw new ArgumentNullException( nameof( transaction ) );
            _dictionary = new ConcurrentDictionary<string, int>();
        }

        public async ValueTask<int> GetPieceId( string title )
        {
            if( _dictionary.TryGetValue( title, out var id ) ) return id;

            var pieceId = await _connection.ExecuteScalarAsync<int>
            (
                @"
                    insert into pieces ( titre ) values( @Title );
                    select last_insert_rowid();",
                new {TItle = title},
                _transaction
            );

            _dictionary.TryAdd( title, pieceId );
            return pieceId;
        }
    }
}
