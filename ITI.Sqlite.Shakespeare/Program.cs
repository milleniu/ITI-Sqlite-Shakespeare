using System;
using System.Diagnostics;
using System.IO;

namespace ITI.Sqlite.Shakespeare
{
    internal static class Program
    {
        private static void Main( string[] args )
        {
            var dbPath = args[0];
            var filePath = args[1];

            var sw = new Stopwatch();
            const int runs = 100;
            long sum = 0;

            for( var i = 0; i < runs; ++i )
            {
                sw.Start();

                var reader = new ShakespeareDatReader( File.ReadAllText( filePath ) );
                while( reader.MoveNextRecord() )
                {
                    while( reader.MoveNextValue() )
                    {
                    }
                }

                sw.Stop();

                sum += sw.ElapsedMilliseconds;
                Console.WriteLine( $"Run {i}: {sw.ElapsedMilliseconds}ms / avg: {sum / (i + 1)}" );

                sw.Reset();
            }

            Console.WriteLine( sum / runs );
        }
    }
}
