using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace DA.NW_Connection
{
    class ProtocalHandler
    {
        private Reader SocketReader;
        private Writer SocketWriter;
        private TcpClient DAServer;
        NetworkStream NWStream;
        public ProtocalHandler( TcpClient tcpClient, String StorageLocation = ".\\" )
        {
            SocketReader = new Reader( );
            SocketWriter = new Writer( );
            DAServer = tcpClient;
            NWStream = tcpClient.GetStream( );

            this.StorageLocation = StorageLocation;
            InitialiseUAuthStorage( );
        }

        private void InitialiseUAuthStorage( )
        {
            if ( File.Exists( StorageLocation + "users.osl" ) )
            {
                Stream stream = File.Open( StorageLocation + "users.osl", FileMode.Open );
                BinaryFormatter bformatter = new BinaryFormatter( );
                AuthenticatedUsers = ( Dictionary<String, String> )bformatter.Deserialize( stream );

                stream.Close( );
            }
            else
            {
                AuthenticatedUsers = new Dictionary<string, string>( );
            }
        }

        public String StorageLocation
        {
            set;
            get;
        }

        public Dictionary<String, String> AuthenticatedUsers
        {
            private set;
            get;
        }

        public Tuple<String, String> AuthTuple
        {
            private set;
            get;
        }


        public void Authenticate( String username, String password )
        {
            if ( AuthenticatedUsers.ContainsKey( username ) )
            {
                if ( AuthenticatedUsers[ username ] == password )
                {
                    AuthTuple = new Tuple<string, string>( username, password );
                    SocketWriter.WriteToSocket( NWStream, "OK" );
                }
                else
                {
                    SocketWriter.WriteToSocket( NWStream, "FAILED" );
                }
            }
            else
            {
                SocketWriter.WriteToSocket( NWStream, "FAILED" );
            }
        }

        internal void Register( String username, String password )
        {
            if ( AuthenticatedUsers.ContainsKey( username ) )
            {
                SocketWriter.WriteToSocket( NWStream, "FAILED" );
            }
            else
            {
                AuthenticatedUsers.Add( username, password );

                Stream stream = File.Open( StorageLocation + "users.osl", FileMode.OpenOrCreate );
                BinaryFormatter bformatter = new BinaryFormatter( );
                bformatter.Serialize( stream, AuthenticatedUsers );

                stream.Close( );

                AuthTuple = new Tuple<string, string>( username, password );
                SocketWriter.WriteToSocket( NWStream, "OK" );
            }
        }

        internal void Download( string Filename )
        {
            if ( File.Exists( StorageLocation + Filename ) )
            {
                byte[] FBytes = File.ReadAllBytes( StorageLocation + Filename );
                SocketWriter.WriteToSocket( NWStream, Convert.ToBase64String( FBytes ) );
            }
        }

        internal void SendFilesList( )
        {
            string[] filePaths = Directory.GetFiles( StorageLocation );

            SocketWriter.WriteToSocket( NWStream, String.Join( " ", filePaths ) );
        }

        internal NetworkStream RegisterFileServer( string v1, string v2 )
        {
            Console.WriteLine( "Registered File Server: " + " " + v1 + " " + v2 );

            SocketWriter.WriteToSocket( NWStream, "OK" );


            return NWStream;
        }
    }
}
