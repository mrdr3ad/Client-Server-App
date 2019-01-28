using DA.NW_Connection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DA
{
    class Program
    {
        public static Thread ServerLoopThread;
        public static List<Thread> ClientThreads;
        public static List<Thread> FileServerThreads;
        public static bool Excecuting;
        public static IPAddress ipAddress;
        public static int portNumber = 11000;
        public static IPEndPoint localEndPoint;
        public static List<NetworkStream> FSStreams;

        static void ServerLoop( )
        {
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

                    case "FileServer":
                        FileServerThreads.Add( new Thread( new ThreadStart( ( ) =>
                        {
                            FileServerLoop( Input, tcpClient );
                        } ) ) );

                        FileServerThreads[ FileServerThreads.Count - 1 ].Start( );
                        break;
                }
            }
        }

        static void ServerActionOnClientInput( String input, ProtocalHandler pHandler )
        {
            String[] Arguments = input.Split( " " );

            Console.WriteLine( "Client Message: " + input );

            switch ( Arguments[ 0 ] )
            {
                case "AUTH":
                    pHandler.Authenticate( Arguments[ 1 ], Arguments[ 2 ] );
                    break;

                case "REG":
                    pHandler.Register( Arguments[ 1 ], Arguments[ 2 ] );
                    break;

                case "FILES":
                    pHandler.SendFilesList( );
                    break;

                case "DLOAD":
                    pHandler.Download( Arguments[ 1 ] );
                    break;
            }
        }

        static void ClientLoop( String InitialMessage, TcpClient tcpClient )
        {
            try
            {
                NetworkStream NWStream = tcpClient.GetStream( );
                Reader SocketReader = new Reader( );
                ProtocalHandler pHandler = new ProtocalHandler( tcpClient );

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

        static void ServerActionOnFSInput( String input, ProtocalHandler pHandler )
        {
            String[] Arguments = input.Split( " " );

            Console.WriteLine( "File Server Message: " + input );

            switch ( Arguments[ 0 ] )
            {
                case "REG":
                    FSStreams.Add( pHandler.RegisterFileServer( Arguments[ 1 ], Arguments[ 2 ] ) );
                    break;
            }
        }

        static void FileServerLoop( String InitialMessage, TcpClient tcpClient )
        {
            NetworkStream NWStream = tcpClient.GetStream( );
            Reader SocketReader = new Reader( );
            ProtocalHandler pHandler = new ProtocalHandler( tcpClient );

            while ( Excecuting )
            {
                ServerActionOnFSInput( SocketReader.ReadFromSocket( NWStream, tcpClient.ReceiveBufferSize ), pHandler );
            }
        }

        static void Main( string[] args )
        {
            ClientThreads = new List<Thread>( );
            FileServerThreads = new List<Thread>( );
            FSStreams = new List<NetworkStream>( );
            var hostentries = Dns.GetHostEntry( Dns.GetHostName( ) );

            ipAddress = hostentries.AddressList[ hostentries.AddressList.Length - 1 ];
            localEndPoint = new IPEndPoint( ipAddress, portNumber );
            Excecuting = true;
            ServerLoopThread = new Thread( new ThreadStart( ServerLoop ) );
            ServerLoopThread.Start( );
            ServerLoopThread.Join( );
        }
    }
}
