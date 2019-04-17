using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text;
using ITI.Sqlite.Shakespeare.Database;
using ITI.Sqlite.Shakespeare.Processing;

namespace ITI.Sqlite.Shakespeare
{
    internal static class Program
    {
        private static void Main( string[] args )
        {
            var dbPath = args[0];
            var filePath = args[1];

            var sw = new Stopwatch();
            sw.Start();

            var fileProcessor = new FileProcessor();
            fileProcessor.LoadFile( filePath );

            StringBuilder sb = null;

            using( var connection = new SQLiteConnection( $"Data source={dbPath};Version=3;" ) )
            {
                try
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();

                    sb = fileProcessor.ProcessFile( connection );

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
            if( sb != null ) Console.WriteLine( sb );
            Console.WriteLine( $"Run: {sw.ElapsedMilliseconds}ms" );
        }
    }
}
