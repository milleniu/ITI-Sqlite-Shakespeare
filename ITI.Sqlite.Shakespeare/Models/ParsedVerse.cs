using System;

namespace ITI.Sqlite.Shakespeare.Models
{
    public readonly struct ParsedVerse
    {
        public readonly int PieceId;
        public readonly int TiradeId;
        public readonly int? Verse;
        public readonly int? VerseId;
        public readonly string Text;

        public ParsedVerse( int verseId, int pieceId, int tiradeId, int? verse, string text )
        {
            Text = text;
            VerseId = verseId;
            TiradeId = tiradeId;
            Verse = verse;
            PieceId = pieceId;
        }
    }
}
