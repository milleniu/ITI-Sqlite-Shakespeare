using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using Dapper;

namespace ITI.Sqlite.Shakespeare.Database
{
    public class TiradeHandler
    {
        private readonly SQLiteConnection _connection;
        private readonly SQLiteTransaction _transaction;

        public TiradeHandler( SQLiteConnection connection, SQLiteTransaction transaction )
        {
            _connection = connection ?? throw new ArgumentNullException( nameof(connection) );
            _transaction = transaction ?? throw new ArgumentNullException( nameof(transaction) );
        }

        public async ValueTask<int> GenerateTirade( int pieceId, int characterId, int? act, int? scene )
        {
            return await _connection.ExecuteScalarAsync<int>
            (
                @"
                    insert into tirades ( id_piece, id_personnage, acte, scene ) values( @PieceId, @CharacterId, @Act, @Scene );
                    select last_insert_rowid();",
                new { PieceId = pieceId, CharacterId = characterId, Act = act, Scene = scene },
                _transaction
            );
        }
    }
}
