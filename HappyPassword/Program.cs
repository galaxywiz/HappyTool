using System;
using UtilLibrary;

namespace HappyPassword
{
    class Program
    {
        static void Main(string[] args)
        {
            AESEncrypt aesEncrypt = new AESEncrypt();
            string 키 = "행복라떼";
            //local IP: 192.168.1.140
            //host IP: 110.47.208.26
            string local = "192.168.1.30";
            string temp = aesEncrypt.AESEncrypt128(local, 키);
            Console.WriteLine(local + ": " + temp);

            string host = "110.47.208.26";
            temp = aesEncrypt.AESEncrypt128(host, 키);
            Console.WriteLine(host + ": " + temp);

            temp = aesEncrypt.AESEncrypt128("galaxywi", 키);
            Console.WriteLine("galaxywi" + ": " + temp);

            temp = aesEncrypt.AESEncrypt128("행복라떼", "암호푸는키");
            Console.WriteLine("행복라떼" + ": " + temp);
        }
    }
}
