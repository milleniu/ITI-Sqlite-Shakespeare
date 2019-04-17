using System;
using System.Data.SQLite;
using System.IO;
using System.Text;
using ITI.Sqlite.Shakespeare.Database;

namespace ITI.Sqlite.Shakespeare.Processing
{
    internal class FileProcessor
    {
        private DatReader _reader;

        public FileProcessor()
        {
            _reader = null;
        }

        public void LoadFile( string filePath )
        {
            if( filePath == null ) throw new ArgumentNullException( nameof( filePath ) );
            if( !File.Exists( filePath ) ) throw new ArgumentException( nameof( filePath ) );

            var content = File.ReadAllText( filePath, Encoding.UTF8 );
            _reader = new DatReader( content );
        }

        public StringBuilder ProcessFile( SQLiteConnection connection )
        {
            if( connection == null ) throw new ArgumentNullException( nameof( connection ) );
            if( _reader == null ) throw new InvalidOperationException( "You must call LoadFile beforehand" );

            var tiradeHandler = new TiradeHandler( connection );
            tiradeHandler.Start();

            var sb = new StringBuilder();
            while( _reader.MoveNextRecord() )
            {
                while( _reader.MoveNextValue() )
                {
                    sb.Append( _reader.Current );
                    sb.Append( "|" );
                }

                sb.Append( "\n" );
            }

            tiradeHandler.Stop();
            tiradeHandler.Finalize( 1_000 );

            return sb;
        }
    }
}
