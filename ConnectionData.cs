using System;


namespace BetterRCON
{
    [Serializable] // nightmare nightmare nightmare nightmare nightmare nightmare nightmare nightmare
    public class ConnectionData
    {
        public string IP;
        public int Port;
        public string Password;

        public ConnectionData(string ip, int port, string password)
        {
            IP = ip;
            Port = port;
            Password = password;
        }
    }
}