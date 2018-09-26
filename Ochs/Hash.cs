using System;
using System.Security.Cryptography;
using System.Text;

namespace Ochs
{
    public class Hash
    {
        public static string getHashSha256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += $"{x:x2}";
            }
            return hashString;
        }
    }
}