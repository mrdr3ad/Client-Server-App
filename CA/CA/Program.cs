using CA.NW_Connection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CA
{
    class Program
    {
        public static String LastInput = String.Empty;
        public static String Input = String.Empty;
        public static String[] Arguments;
        public static List<String> FilesList = new List<string>( );
        public static IPAddress sIPAddress = null;
        public static int sPort;
        public static bool connected = false;
        public static bool isFileServer = false;
        public static TcpClient tcpClient = null;
        public static ProtocalHandler pHandler;
        private static bool displayFilesList;

        static void PrintMenue( )
        {
            Console.Clear( );
            Console.WriteLine( "CA App" );
            Console.WriteLine( "To connect please write: <serveraddress> <portnumber>" );
            Console.WriteLine( "To close please write: exit" );
        }

        static void PrintServerMenue( )
        {
            Console.Clear( );
            Console.WriteLine( "To List Remote Files Enter: L" );
            Console.WriteLine( "To List Local Files Enter: l" );
            Console.WriteLine( "To Download File Enter: D <filename>" );
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
            while ( true )
            {
                if ( !connected )
                {
                    PrintMenue( );
                    GetInput( );

                    try
                    {
                        if ( Arguments[ 0 ] == "exit" )
                            break;

                        if ( IPAddress.TryParse( Arguments[ 0 ], out sIPAddress ) )
                        {
                            if ( int.TryParse( Arguments[ 1 ], out sPort ) )
                            {
                                try
                                {
                                    Console.WriteLine( "Connecting..." );
                                    tcpClient = new TcpClient( );
                                    IPEndPoint ipEndPoint = new IPEndPoint( sIPAddress, sPort );
                                    tcpClient.Connect( ipEndPoint );

                                    Console.WriteLine( "To connect please write: <username> <password>" );
                                    GetInput( );
                                    pHandler = new ProtocalHandler( tcpClient );
                                    if ( pHandler.Authenticate( Arguments[ 0 ], Arguments[ 1 ] ) )
                                    {
                                        Console.WriteLine( "Connected" );
                                        connected = true;
                                    }
                                    else
                                    {
                                        Console.WriteLine( "Failed to verify user, closing connection." );
                                        Console.WriteLine( "Attempt To Register with server? Yes/No" );
                                        GetInput( );

                                        if ( Arguments[ 0 ] == "Yes" )
                                        {
                                            Console.WriteLine( "To register please write: <username> <password>" );
                                            GetInput( );
                                            if ( pHandler.Register( Arguments[ 0 ], Arguments[ 1 ] ) )
                                            {
                                                Console.WriteLine( "Connected" );
                                                connected = true;
                                            }
                                        }
                                    }
                                }
                                catch ( Exception e )
                                {
                                    Console.WriteLine( e );
                                }
                                Thread.Sleep( 1000 );
                            }
                        }
                    }
                    catch ( Exception )
                    {

                    }
                }
                else
                {
                    while ( true )
                    {
                        Console.WriteLine( "Connected To Server" );

                        PrintServerMenue( );

                        if ( FilesList.Count > 0 )
                            PrintFilesList( );

                        GetInput( );

                        if ( Arguments[ 0 ] == "exit" )
                            break;

                        if ( Arguments[ 0 ] == "L" )
                        {
                            pHandler.DisplayFileList( FilesList );
                        }

                        if ( Arguments[ 0 ] == "l" )
                        {
                            pHandler.DisplayLocalFilesList( FilesList );
                        }

                        if ( Arguments[ 0 ] == "D" )
                        {
                            pHandler.DownloadFile( Arguments[ 1 ] );
                        }

                        if ( Arguments[ 0 ] == "H" )
                        {
                            displayFilesList = false;
                        }

                        LastInput = Input;
                    }

                    break;
                }
            }
        }

        private static void PrintFilesList( )
        {
            foreach ( String Filename in FilesList )
            {
                Console.WriteLine( Filename );
            }
        }
    }
}
