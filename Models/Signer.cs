using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Models
{
    public static class Signer
    {
        public static string ComputeHash(byte[] file)
        {
            using MemoryStream stream = new MemoryStream(file);
            SHA256Managed sha = new SHA256Managed();
            return sha.ComputeHash(stream).ToHex();
        }

        private static string ToHex(this byte[] bytes)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                result.Append(bytes[i].ToString("X2"));
            }

            return result.ToString();
        }
    }
}