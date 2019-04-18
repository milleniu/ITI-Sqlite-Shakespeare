using System;
using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Threading.Tasks;
using Dapper;

namespace ITI.Sqlite.Shakespeare.Database
{
    internal sealed class CharacterHandler
    {
        private readonly SQLiteConnection _connection;
        private readonly SQLiteTransaction _transaction;
        private readonly ConcurrentDictionary<string, int> _dictionary;

        public CharacterHandler( SQLiteConnection connection, SQLiteTransaction transaction )
        {
            _connection = connection ?? throw new ArgumentNullException( nameof( connection ) );
            _transaction = transaction ?? throw new ArgumentNullException( nameof( transaction ) );
            _dictionary = new ConcurrentDictionary<string, int>();
        }

        public async ValueTask<int> GetCharacterId( string character )
        {
            if( _dictionary.TryGetValue( character, out var id ) ) return id;

            var characterId = await _connection.ExecuteScalarAsync<int>
            (
                @"
                    insert into personnages ( nom_personnage ) values( @Name );
                    select last_insert_rowid();",
                new {Name = character},
                _transaction
            );

            _dictionary.TryAdd( character, characterId );
            return characterId;
        }
    }
}
