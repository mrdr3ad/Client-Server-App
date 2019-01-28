using System;
using System.Net.Sockets;
using System.Text;

namespace CA.NW_Connection
{
    class Reader
    {
        int offset;
        bool firstFill;
        bool overspill;
        

        public String ReadFromSocket(NetworkStream networkStream, int bufferSize)
        {
            byte[] bytes = new byte[ bufferSize ];
            string message = "";
            int bytesRead = 0;
            offset = 0;
            firstFill = true;
            overspill = false;

            int length = 0;
            bool continuation = false;

            while (true)
            {
                try
                {
                    // We have never read into the buffer before, read to fill it
                    if (firstFill)
                    {
                        bytesRead = networkStream.Read(bytes);
                        offset = 0;
                        length = 0;
                        continuation = false;
                        firstFill = false;
                    }

                    do
                    {
                        if (overspill)
                        {
                            // message has been partially read and has spilled over into another buffer
                            // fill new buffer then read remainder of message
                            Array.Clear(bytes, 0, bufferSize);
                            bytesRead = networkStream.Read(bytes);
                            message = message + Encoding.ASCII.GetString(bytes, 2, length);
                            offset = length;
                            overspill = false;
                            length = 0;
                        } 
                        else
                        {
                            while (offset < bufferSize && bytes[offset] == 0)
                            {
                                offset = offset + 1;
                            }

                            if (offset == bufferSize)
                            {
                                // we are doing a normal read: check value at offset
                                Array.Clear(bytes, 0, bufferSize);
                                bytesRead = networkStream.Read(bytes);
                                offset = 0;
                                length = 0;
                            }

                            length = (int)bytes[offset]; // maximum of 254 bytes + length + continuation
                            continuation = bytes[offset + 1] > 0 ? true:false;

                            if (offset + length + 2 < bufferSize)
                            {
                                message = message + Encoding.ASCII.GetString(bytes, offset+2, length);
                                offset = offset + 2 + length;
                                length = 0;
                            } else
                            {
                                int chunk = bufferSize - offset - 2; // remove offset + 1 for the continuation byte, read the rest
                                message = message + Encoding.ASCII.GetString(bytes, offset+2, chunk);
                                length = length - chunk;
                                overspill = true;
                            }
                            //Console.WriteLine("Length:  " + length + " Continuation: " + continuation.ToString() + " MSGLEN: " + message.Length);   
                        }
                    } while (overspill || continuation);
                    return message;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    return message;
                }
            }
        }
    }
}
