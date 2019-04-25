using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ITI.Sqlite.Shakespeare
{
    internal static class Program
    {
        private static async Task Main( string[] args )
        {
            var dbPath = args[0];
            var filePath = args[1];

            var sw = new Stopwatch();
            sw.Start();

            using( var connection = new SQLiteConnection( $"Data source={dbPath};Version=3;" ) )
            {
                try
                {
                    await connection.OpenAsync();
                    var transaction = connection.BeginTransaction();

                    await (await FileProcessor.GetFileProcessor( connection, transaction, filePath )).ProcessFile();

                    transaction.Commit();
                }
                catch( Exception exception )
                {
                    var foregroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine( $"Exception occured: {exception.Message}" );
                    Console.WriteLine( exception.StackTrace );
                    Console.ForegroundColor = foregroundColor;
                }
                finally
                {
                    connection.Close();
                }
            }

            sw.Stop();
            Console.WriteLine( $"Ran for {sw.ElapsedMilliseconds}ms" );
        }
    }
}
