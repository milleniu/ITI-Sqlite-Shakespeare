using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ITI.Sqlite.Shakespeare.Database;
using ITI.Sqlite.Shakespeare.Processing;

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

            var fileProcessor = new FileProcessor();
            await fileProcessor.LoadFile( filePath );

            using( var connection = new SQLiteConnection( $"Data source={dbPath};Version=3;" ) )
            {
                try
                {
                    await connection.OpenAsync();
                    var transaction = connection.BeginTransaction();

                    fileProcessor.ProcessFile( connection, transaction );

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
            Console.WriteLine( $"Run: {sw.ElapsedMilliseconds}ms" );
        }
    }
}
