using FS.NW_Connection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FS
{
    class Program
    {
        private static TcpClient tcpClient;
        internal static int       portNumber = 12000;
        internal static IPAddress ipAddress;

        public static string Input { get; private set; }
        public static string[] Arguments { get; private set; }
        public static bool Excecuting { get; private set; }
        public static List<Thread> ClientThreads { get; private set; }

        static void PrintMenue( )
        {
            Console.Clear( );
            Console.WriteLine( "FS App" );
            Console.WriteLine( "To connect please write: <serveraddress> <portnumber>" );
            Console.WriteLine( "To close please write: exit" );
        }

        static void GetInput( )
        {
            Console.WriteLine( );
            Console.Write( ":" );
            Input = Console.ReadLine( );
            Arguments = Input.Split( " " );
        }

        static void Main( string[] args )
        {
            Console.WriteLine( "File Server Started" );
            IPHostEntry hostentries = Dns.GetHostEntry( Dns.GetHostName( ) );
            ipAddress = hostentries.AddressList[ hostentries.AddressList.Length - 1 ];

            while ( true )
            {
                PrintMenue( );
                GetInput( );

                try
                {
                    if ( IPAddress.TryParse( Arguments[ 0 ], out IPAddress DAIPAddress ) )
                    {
                        if ( int.TryParse( Arguments[ 1 ], out int DAPort ) )
                        {
                            ConnectToDa( DAIPAddress, DAPort );

                            ProtocalHandler pHandler = new ProtocalHandler( tcpClient, tcpClient.GetStream() );

                            Thread.Sleep( 250 );
                            if ( pHandler.RegisterWithDA( ) )
                            {
                                Reader SocketReader = new Reader( );

                                Console.WriteLine( "Connected To: " + DAIPAddress + " " + DAPort );
                                
                                new Thread( new ThreadStart( () => { StartClientHandler( ); } ) );

                                NetworkStream NWStream = tcpClient.GetStream( );

                                while ( true )
                                {
                                    var message = SocketReader.ReadFromSocket( NWStream, tcpClient.ReceiveBufferSize );

                                    pHandler.HandleDaRequest( message );
                                }
                            }
                        }
                    }
                }
                catch ( Exception Ex )
                {
                    Console.WriteLine( Ex );
                }
                Thread.Sleep( 1000 );
            }
        }

        private static void StartClientHandler( )
        {
            Excecuting = true;
            
            TcpListener tcpListener = new TcpListener( ipAddress, portNumber );
            tcpListener.Start( );
            String Input;
            String[] Arguments;
            Reader SocketReader = new Reader( );

            Console.WriteLine( "TCPListner Started" );
            Console.WriteLine( "Address: " + ipAddress.ToString( ) );
            Console.WriteLine( "PortNumber: " + portNumber );

            while ( Excecuting )
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient( );

                //---get the incoming data through a network stream---
                NetworkStream NWStream = tcpClient.GetStream( );

                //---read incoming stream---
                Input = SocketReader.ReadFromSocket( NWStream, tcpClient.ReceiveBufferSize );
                Arguments = Input.Split( " " );

                switch ( Arguments[ 0 ] )
                {
                    case "AUTH":
                    case "FILES":
                    case "DLOAD":
                    case "REG":
                        ClientThreads.Add( new Thread( new ThreadStart( ( ) =>
                        {
                            ClientLoop( Input, tcpClient );
                        } ) ) );

                        ClientThreads[ ClientThreads.Count - 1 ].Start( );
                        break;
                }
            }
        }

        private static void ConnectToDa( IPAddress dAIPAddress, int dAPort )
        {
            Console.WriteLine( "Connecting..." );
            tcpClient = new TcpClient( );
            IPEndPoint ipEndPoint = new IPEndPoint( dAIPAddress, dAPort );
            tcpClient.Connect( ipEndPoint );
        }

        static void ServerActionOnClientInput( String input, ProtocalHandler pHandler )
        {
            String[] Arguments = input.Split( " " );

            Console.WriteLine( "Client Message: " + input );

            switch ( Arguments[ 0 ] )
            {
                case "FILE":
                    Byte[] EncryptionKey = pHandler.AuthenticateWithDA( Arguments[ 1 ] );
                    pHandler.SendFile( EncryptionKey );
                    break;
            }
        }

        static void ClientLoop( String InitialMessage, TcpClient tcpClient )
        {
            try
            {
                NetworkStream NWStream = tcpClient.GetStream( );
                Reader SocketReader = new Reader( );
                ProtocalHandler pHandler = new ProtocalHandler( tcpClient, NWStream );

                ServerActionOnClientInput( InitialMessage, pHandler );

                while ( Excecuting )
                {
                    ServerActionOnClientInput( SocketReader.ReadFromSocket( NWStream, tcpClient.ReceiveBufferSize ), pHandler );
                }
            }
            catch ( Exception Ex )
            {
                Console.WriteLine( Ex );
                tcpClient.Close( );
            }
        }
    }
}
