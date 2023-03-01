using System.Net;

namespace NetLibrary
{
    public class NetUtil
    {
        public NetUtil() { }
        public static string localIp()
        {
            IPHostEntry host = Dns.GetHostByName(Dns.GetHostName());
            string myip = host.AddressList[0].ToString();
            return myip;
        }
    }
}
