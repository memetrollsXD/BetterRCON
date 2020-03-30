using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ChimitRCON
{

    /*
     * use constant {
    # Packet types
    AUTH            =>  3,  # Minecraft RCON login packet type
    AUTH_RESPONSE   =>  2,  # Server auth response
    COMMAND         =>  2,  # Command packet type
    RESPONSE_VALUE  =>  0,  # Server response
    };
     */
    public class OtherRCON : IDisposable
    {
        public enum RCONMessageType
        {
            None = -1,
            Auth = 3,
            Command = 2,
            AuthResponse = 2,
            ResponseValue = 0
        }

        public enum RCONColorMode
        {
            AS_IS,
            ANSI,
            STRIP
        }

        public OtherRCON(string host, int port, string password, RCONColorMode colormode)
        {
            m_uid = 1;
            m_sender = null;
            setupStream(host, port, password, colormode);
        }

        public OtherRCON()
        {
            m_uid = 1;
            m_host = null;
            m_password = null;
            m_sender = null;
            m_colormode = RCONColorMode.AS_IS;
        }

        public void ConnectSocket()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                IPHostEntry ipHostInfo = Dns.GetHostEntry(m_host);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, m_port);

                // Create a TCP/IP  socket.  
                m_sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    m_sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        m_sender.RemoteEndPoint.ToString());
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
                m_sender.ReceiveTimeout = 2000;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        protected void SendSocket(byte[] packet)
        {
            // Send the data through the socket.  
            if (null == m_sender)
            {
                return;
            }
            int bytesSent;
            try
            {
                bytesSent = m_sender.Send(packet);
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }

        protected int ReceiveSocket(byte[] bytes) {

            // Receive the response from the remote device.  
            int bytesRec = -1;
            if (null == m_sender)
            {
                return bytesRec;
            }
            try
            {
                bytesRec = m_sender.Receive(bytes);
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
            return bytesRec;
        }

        protected void CloseSocket() {
            if (null == m_sender)
            {
                return;
            }
            // Release the socket.  
            m_sender.Shutdown(SocketShutdown.Both);
            m_sender.Close();
            m_sender.Dispose();
            m_sender = null;
        }

        protected byte[] INT2LE(int data)
        {
            byte[] b = new byte[4];
            b[0] = (byte)data;
            b[1] = (byte)(((uint)data >> 8) & 0xFF);
            b[2] = (byte)(((uint)data >> 16) & 0xFF);
            b[3] = (byte)(((uint)data >> 24) & 0xFF);
            return b;
        }

        private byte[] pack_msg(RCONMessageType mtype, string msg)
        {
            int msgsize = CalcSize(msg);
            List<byte> blist = new List<byte>();
            blist.AddRange(INT2LE(msgsize)); // Size
            blist.AddRange(INT2LE(m_uid++)); // ID
            blist.AddRange(INT2LE((int)mtype)); // Type
            blist.AddRange(STR2B(msg)); // Body, null terminated ascii string
            blist.AddRange(STR2B("")); // Null terminated empty string
            return blist.ToArray();
        }

        protected int CalcSize(string msg)
        {
            return 10 + msg.Length;
        }

        protected byte[] STR2B(string str)
        {
            byte[] b = new byte[str.Length + 1];
            for (int i = 0; i < str.Length; ++i)
            {
                b[i] = (byte)str[i];
            }
            b[str.Length] = 0;
            return b;
        }

        public void Dispose()
        {
            CloseSocket();
        }

        protected int m_uid;
        protected Socket m_sender;

        public RCONColorMode m_colormode { get; private set; }

        public void setupStream(string host, int port, string password, RCONColorMode colormode)
        {
            m_host = host;
            m_port = port;
            m_password = password;
            m_colormode = colormode;
            ConnectSocket();
            sendMessage(RCONMessageType.Auth, m_password);
        }

        public string m_host { get; private set; }
        public int m_port { get; private set; }

        public string sendMessage(RCONMessageType commandType, string msg)
        {
            byte[] request = pack_msg(commandType, msg);
            SendSocket(request);
            string res = ReceiveMessage();
            return res;
        }

        public string ReceiveMessage()
        {
            byte[] buffer = new byte[bufsize];
            List<byte> response = new List<byte>();
            do
            {
                int len = ReceiveSocket(buffer);
                if (len <= 0)
                {
                    return "";
                }
                for (int i = 0; i < len; ++i)
                {
                    response.Add(buffer[i]);
                }
            } while (!PacketComplete(response.ToArray()));
            string remotemsg = "";
            RCONMessageType receiveType = RCONMessageType.None;
            unpack_msg(response.ToArray(), ref receiveType, ref remotemsg);
            switch (m_colormode)
            {
                case RCONColorMode.ANSI:
                    remotemsg = ConvertColorCodesToAnsi(remotemsg);
                    break;
                case RCONColorMode.AS_IS:
                    // do nothing
                    break;
                case RCONColorMode.STRIP:
                    remotemsg = StripColorCodes(remotemsg);
                    break;
            }
            return remotemsg;
        }

        private string StripColorCodes(string remotemsg)
        {
            // quick and dirty version
            remotemsg = remotemsg.Replace("\xc2\u00A74", ""); // dark_red
            remotemsg = remotemsg.Replace("\xc2\u00A7c", ""); // red
            remotemsg = remotemsg.Replace("\xc2\u00A76", ""); // gold
            remotemsg = remotemsg.Replace("\xc2\u00A7e", ""); // yellow
            remotemsg = remotemsg.Replace("\xc2\u00A72", ""); // dark_green
            remotemsg = remotemsg.Replace("\xc2\u00A7a", ""); // green
            remotemsg = remotemsg.Replace("\xc2\u00A7b", ""); // aqua
            remotemsg = remotemsg.Replace("\xc2\u00A73", ""); // dark_aqua
            remotemsg = remotemsg.Replace("\xc2\u00A71", ""); // dark_blue
            remotemsg = remotemsg.Replace("\xc2\u00A79", ""); // blue
            remotemsg = remotemsg.Replace("\xc2\u00A7d", ""); // light_purple
            remotemsg = remotemsg.Replace("\xc2\u00A75", ""); // dark_purple
            remotemsg = remotemsg.Replace("\xc2\u00A7f", ""); // white
            remotemsg = remotemsg.Replace("\xc2\u00A77", ""); // gray
            remotemsg = remotemsg.Replace("\xc2\u00A78", ""); // dark_gray
            remotemsg = remotemsg.Replace("\xc2\u00A70", ""); // black
            remotemsg = remotemsg.Replace("\xc2\u00A7r", ""); // reset
            remotemsg = remotemsg.Replace("\xc2\u00A7l", ""); // bold
            remotemsg = remotemsg.Replace("\xc2\u00A7o", ""); // italic
            remotemsg = remotemsg.Replace("\xc2\u00A7m", ""); // strike
            remotemsg = remotemsg.Replace("\xc2\u00A7n", ""); // underline
            remotemsg = remotemsg.Replace("\xc2\u00A7k", ""); // we dont know
            return remotemsg;
        }

        public string ConvertColorCodesToAnsi(string remotemsg)
        {
            // still to-do: The light and the dark version are the same now. ansi module needs to be updated as well
            remotemsg = remotemsg.Replace("\xc2\u00A74", "\u001b[0;31m"); // dark_red
            remotemsg = remotemsg.Replace("\xc2\u00A7c", "\u001b[1;31m"); // red
            remotemsg = remotemsg.Replace("\xc2\u00A76", "\u001b[1;33m"); // gold
            remotemsg = remotemsg.Replace("\xc2\u00A7e", "\u001b[0;33m"); // yellow
            remotemsg = remotemsg.Replace("\xc2\u00A72", "\u001b[0;32m"); // dark_green
            remotemsg = remotemsg.Replace("\xc2\u00A7a", "\u001b[1;32m"); // green

            remotemsg = remotemsg.Replace("\xc2\u00A7b", "\u001b[1;36m"); // aqua td
            remotemsg = remotemsg.Replace("\xc2\u00A73", "\u001b[0;36m"); // dark_aqua td
            remotemsg = remotemsg.Replace("\xc2\u00A71", "\u001b[0;34m"); // dark_blue
            remotemsg = remotemsg.Replace("\xc2\u00A79", "\u001b[1;34m"); // blue
            remotemsg = remotemsg.Replace("\xc2\u00A7d", "\u001b[0;35m"); // light_purple
            remotemsg = remotemsg.Replace("\xc2\u00A75", "\u001b[1;35m"); // dark_purple
            remotemsg = remotemsg.Replace("\xc2\u00A7f", "\u001b[0;35m"); // white
            remotemsg = remotemsg.Replace("\xc2\u00A77", "\u001b[1;30m"); // gray
            remotemsg = remotemsg.Replace("\xc2\u00A78", "\u001b[1;30m"); // dark_gray
            remotemsg = remotemsg.Replace("\xc2\u00A70", "\u001b[0;30m"); // black
            remotemsg = remotemsg.Replace("\xc2\u00A7r", "\u001b[0m"); // reset
            remotemsg = remotemsg.Replace("\xc2\u00A7l", "\u001b[1m"); // bold
            remotemsg = remotemsg.Replace("\xc2\u00A7o", "\u001b[3m"); // italic
            remotemsg = remotemsg.Replace("\xc2\u00A7m", "\u001b[21m"); // strike
            remotemsg = remotemsg.Replace("\xc2\u00A7n", "\u001b[4m"); // underline
            remotemsg = remotemsg.Replace("\xc2\u00A7k", "\u001b[7m"); // we dont know, reverse
            return remotemsg;
        }

        private void unpack_msg(byte[] packet, ref RCONMessageType receiveType, ref string remotemsg)
        {
            int len = LE2INT(packet, 0);
            int id = LE2INT(packet, 4);
            int command = LE2INT(packet, 8);
            receiveType = (RCONMessageType)command;
            remotemsg = "";
            for (int i = 12; 0 != packet[i]; ++i)
            {
                remotemsg += (char)packet[i];
            }
            if (packet[13 + remotemsg.Length] != 0)
            {
                // error
            }
        }

        protected bool PacketComplete(byte[] packet)
        {
            int len = LE2INT(packet, 0);
            return packet.Length >= len + 4;
        }

        protected int LE2INT(byte[] data, int startIndex)
        {
            int res;
            res = data[startIndex + 3] << 24;
            res |= data[startIndex + 2] << 16;
            res |= data[startIndex + 1] << 8;
            res |= data[startIndex];
            return res;
        }

        public string m_password { get; private set; }
        const int bufsize = 4096; // maximum possible packet size
    }
}
