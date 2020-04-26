using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
                result.Append(bytes[i].ToString("X2"));

            return result.ToString();
        }
    }
}
