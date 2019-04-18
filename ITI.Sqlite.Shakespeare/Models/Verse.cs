using System;

namespace ITI.Sqlite.Shakespeare.Models
{
    public readonly struct Verse
    {
        public readonly int PieceId;
        public readonly int TiradeId;
        public readonly int VerseId;
        public readonly ReadOnlyMemory<char> Text;

        public Verse( ReadOnlyMemory<char> text, int verseId, int tiradeId, int pieceId )
        {
            Text = text;
            VerseId = verseId;
            TiradeId = tiradeId;
            PieceId = pieceId;
        }
    }
}
