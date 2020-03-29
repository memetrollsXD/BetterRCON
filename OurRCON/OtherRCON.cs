using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace BetterRCON
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
            Auth,
            AuthResponse,
            AuthFail,
            Command,
            ResponseValue
        }

        public OtherRCON(string host, int port, string password)
        {
            m_uid = 1;
            m_sender = null;
            setupStream(host, port, password);
        }

        public OtherRCON()
        {
            m_uid = 1;
            m_host = null;
            m_password = null;
            m_sender = null;
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
            blist.AddRange(INT2LE(RCONMessageTypeToInt(mtype))); // Type
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

        protected int RCONMessageTypeToInt(RCONMessageType mtype)
        {
            switch (mtype)
            {
                case RCONMessageType.Auth: return 3; // Minecraft RCON login packet type
                case RCONMessageType.AuthResponse: return 2; //Server auth response
                case RCONMessageType.AuthFail: return -1; // Auth failure (password invalid)
                case RCONMessageType.Command: return 2; //  Command packet type
                case RCONMessageType.ResponseValue: return 0; //  Server response
                default:
                    return -1;
            }
        }

        public void Dispose()
        {
            CloseSocket();
        }

        protected int m_uid;
        protected Socket m_sender;

        public void setupStream(string host, int port, string password)
        {
            m_host = host;
            m_port = port;
            m_password = password;
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
            RCONMessageType receiveType = RCONMessageType.AuthFail;
            unpack_msg(response.ToArray(), ref receiveType, ref remotemsg);
            if (true)
            {
                remotemsg = ConvertColorCodes(remotemsg);
            }
            return remotemsg;
        }

        public string ConvertColorCodes(string remotemsg)
        {
            remotemsg = remotemsg.Replace("\xc2\u00A74", "\u001b[0;31m"); // dark_red
            remotemsg = remotemsg.Replace("\xc2\u00A7c", "\u001b[0;31m"); // red
            remotemsg = remotemsg.Replace("\xc2\u00A76", "\u001b[0;33m"); // gold
            remotemsg = remotemsg.Replace("\xc2\u00A7e", "\u001b[0;33m"); // yellow
            remotemsg = remotemsg.Replace("\xc2\u00A72", "\u001b[0;32m"); // dark_green
            remotemsg = remotemsg.Replace("\xc2\u00A7a", "\u001b[0;32m"); // green
            remotemsg = remotemsg.Replace("\xc2\u00A7b", "\u001b[0;36m"); // aqua
            remotemsg = remotemsg.Replace("\xc2\u00A73", "\u001b[0;36m"); // dark_aqua
            remotemsg = remotemsg.Replace("\xc2\u00A71", "\u001b[0;34m"); // blue
            remotemsg = remotemsg.Replace("\xc2\u00A79", "\u001b[0;34m"); // light_purple
            remotemsg = remotemsg.Replace("\xc2\u00A7d", "\u001b[0;35m"); // dark_purple
            remotemsg = remotemsg.Replace("\xc2\u00A75", "\u001b[0;35m"); // white
            remotemsg = remotemsg.Replace("\xc2\u00A7f", "\u001b[1;37m"); // gray
            remotemsg = remotemsg.Replace("\xc2\u00A77", "\u001b[0;37m"); // dark_gray
            remotemsg = remotemsg.Replace("\xc2\u00A78", "\u001b[1;30m"); // black
            remotemsg = remotemsg.Replace("\xc2\u00A70", "\u001b[0m"); // reset
            remotemsg = remotemsg.Replace("\xc2\u00A7l", ""); // bold
            remotemsg = remotemsg.Replace("\xc2\u00A7o", ""); // italic
            remotemsg = remotemsg.Replace("\xc2\u00A7m", ""); // stike
            remotemsg = remotemsg.Replace("\xc2\u00A7n", ""); // underline
            remotemsg = remotemsg.Replace("\xc2\u00A7k", ""); // we dont know
            return remotemsg;
        }

        private void unpack_msg(byte[] packet, ref RCONMessageType receiveType, ref string remotemsg)
        {
            int len = LE2INT(packet, 0);
            int id = LE2INT(packet, 4);
            int command = LE2INT(packet, 8);
            receiveType = IntToRCONMessageType(command);
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

        private RCONMessageType IntToRCONMessageType(int command)
        {
            switch (command)
            {
                case 0: return RCONMessageType.ResponseValue;
                case 2: return RCONMessageType.ResponseValue;
                case 3: return RCONMessageType.Auth;
            }
            return RCONMessageType.AuthFail;
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
