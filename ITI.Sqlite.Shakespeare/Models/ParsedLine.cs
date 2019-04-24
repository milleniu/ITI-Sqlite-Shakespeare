using System;

namespace ITI.Sqlite.Shakespeare.Models
{
    public readonly struct ParsedLine
    {
        public readonly ReadOnlyMemory<char> Verse;
        public readonly ReadOnlyMemory<char> Piece;
        public readonly ReadOnlyMemory<char> Tirade;
        public readonly ReadOnlyMemory<char> TiradeInfo;
        public readonly ReadOnlyMemory<char> Character;
        public readonly ReadOnlyMemory<char> Text;

        public ParsedLine
        (
            in ReadOnlyMemory<char> verse,
            in ReadOnlyMemory<char> piece,
            in ReadOnlyMemory<char> tirade,
            in ReadOnlyMemory<char> tiradeInfo,
            in ReadOnlyMemory<char> character,
            in ReadOnlyMemory<char> text
        )
        {
            Verse = verse;
            Piece = piece;
            Tirade = tirade;
            TiradeInfo = tiradeInfo;
            Character = character;
            Text = text;
        }
    }
}
