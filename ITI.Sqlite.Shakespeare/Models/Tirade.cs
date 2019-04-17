namespace ITI.Sqlite.Shakespeare.Models
{
    public readonly struct Tirade
    {
        public readonly int TiradeId;
        public readonly int PieceId;
        public readonly int CharacterId;
        public readonly int Act;
        public readonly int Scene;

        public Tirade( int tiradeId, int pieceId, int characterId, int act, int scene )
        {
            TiradeId = tiradeId;
            PieceId = pieceId;
            CharacterId = characterId;
            Act = act;
            Scene = scene;
        }
    }
}
