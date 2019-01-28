using System;
using System.Net.Sockets;
using System.Text;

namespace DA.NW_Connection
{
    class Writer
    {
        public void WriteToSocket(NetworkStream networkStream, String message)
        {
            message = message.Replace("\0", string.Empty);
            int bytesCount = 0;
            int sets = 0;
            byte[] sendBuffer = new byte[ 256 ];
            bytesCount = message.Length;
            byte[] temp = Encoding.ASCII.GetBytes( message );

            try
            {
                while ( bytesCount > 254 )
                {
                    // Set message buffer components
                    sendBuffer[ 0 ] = 254;
                    sendBuffer[ 1 ] = 1;

                    // Copy across the first 254 bits of the message from the message buffer to the send buffer
                    Buffer.BlockCopy( temp, sets * 254, sendBuffer, 2, 254 );

                    // increment the number of message sets and decrease the byte counter for this message
                    sets++;
                    bytesCount -= 254;

                    // add this to the send buffer in order
                    networkStream.Write( sendBuffer );

                    // clear the array - we don't know how many bytes we will have to deal with in future attempts
                    Array.Clear( sendBuffer, 0, 256 );
                }

                // Last chunk of the message - set the byte count to a real number and deal and contunuation to false; 
                sendBuffer[ 0 ] = ( byte )bytesCount;
                sendBuffer[ 1 ] = 0;

                // copy at offset sets*254, into sendBuffer offset at 2
                Buffer.BlockCopy( temp, sets * 254, sendBuffer, 2, bytesCount );
                networkStream.Write( sendBuffer );
            }
            catch ( Exception e )
            {
                Console.WriteLine(e);
            }
        }
    }
}
