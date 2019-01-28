using System;
using System.IO;
using System.Net.Sockets;

namespace FS.NW_Connection
{
    internal class ProtocalHandler
    {
        private TcpClient tcpClient;

        public NetworkStream NWStream { get; private set; }
        public string StorageLocation
        {
            get
            {
                return ".\\";
            }
        }

        public Writer SocketWriter { get; private set; }
        public Reader SocketReader { get; private set; }

        public ProtocalHandler( TcpClient tcpClient, NetworkStream NWStream )
        {
            SocketWriter = new Writer( );
            SocketReader = new Reader( );
            this.tcpClient = tcpClient;
            this.NWStream = NWStream;
        }

        internal bool RegisterWithDA( )
        {
            SocketWriter.WriteToSocket( NWStream, "FileServer" );
            SocketWriter.WriteToSocket( NWStream, "REG" + " " + Program.ipAddress + " " + Program.portNumber );
            String Response = SocketReader.ReadFromSocket( NWStream, tcpClient.ReceiveBufferSize );

            if ( Response == "OK" )
                return true;

            return false;
        }

        internal byte[] AuthenticateWithDA( string v )
        {
            throw new NotImplementedException( );
        }

        internal void SendFile( byte[] encryptionKey )
        {
            throw new NotImplementedException( );
        }

        internal void HandleDaRequest( string Request )
        {
            switch (Request)
            {
                case "FILES":
                    SendFilesList( );
                    break;
            }
        }

        private void SendFilesList( )
        {
            string[] filePaths = Directory.GetFiles( StorageLocation );

            SocketWriter.WriteToSocket( NWStream, "Files "+ String.Join( " ", filePaths ) );
        }
    }
}