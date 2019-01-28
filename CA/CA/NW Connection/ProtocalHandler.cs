using CA.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace CA.NW_Connection
{
    class ProtocalHandler
    {
        private Reader SocketReader;
        private Writer SocketWriter;
        private TcpClient DAServer;
        NetworkStream NWStream;

        public ProtocalHandler( TcpClient tcpClient )
        {
            SocketReader = new Reader( );
            SocketWriter = new Writer( );
            DAServer = tcpClient;
            NWStream = tcpClient.GetStream( );
        }

        public String StorageLocation
        {
            get
            {
                return ".\\Files\\";
            }
        }

        internal bool Authenticate( string username, string password )
        {
            SocketWriter.WriteToSocket( NWStream, "AUTH" + " " + username + " " + password.GetSHA256Hash( ) );
            String Response = SocketReader.ReadFromSocket( NWStream, DAServer.ReceiveBufferSize );

            if ( Response == "OK" )
                return true;

            return false;
        }

        internal void DisplayFileList( List<string> filesList )
        {
            SocketWriter.WriteToSocket( NWStream, "FILES" );
            String Response = SocketReader.ReadFromSocket( NWStream, DAServer.ReceiveBufferSize );

            filesList.Clear( );
            filesList.AddRange( new List<string>( Response.Split( " " ) ) );
        }

        internal void DownloadFile( string filename )
        {
            SocketWriter.WriteToSocket( NWStream, "DLOAD " + filename );
            String Response = SocketReader.ReadFromSocket( NWStream, DAServer.ReceiveBufferSize );
            File.WriteAllBytes( StorageLocation + filename, Convert.FromBase64String( Response ) );
        }

        internal bool Register( string username, string password )
        {
            SocketWriter.WriteToSocket( NWStream, "REG" + " " + username + " " + password.GetSHA256Hash( ) );
            String Response = SocketReader.ReadFromSocket( NWStream, DAServer.ReceiveBufferSize );

            if ( Response == "OK" )
                return true;

            return false;
        }

        internal void DisplayLocalFilesList( List<string> filesList )
        {
            if ( !Directory.Exists( StorageLocation ) )
                Directory.CreateDirectory( StorageLocation );

            filesList.Clear( );

            filesList.AddRange( new List<string>( Directory.GetFiles( StorageLocation ) ) );
        }
    }
}
