using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Valyreon.Elib.Wpf.Models
{
    public static class Signer
    {
        public static string ComputeHash(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            var hashes = new StringBuilder();
            var sha = SHA256.Create();

            foreach (var chunk in ReadChunks(filePath))
            {
                hashes.Append(sha.ComputeHash(chunk).ToHex());
            }

            var aggregateHash = Encoding.ASCII.GetBytes(hashes.ToString());
            return sha.ComputeHash(aggregateHash).ToHex();
        }

        private static string ToHex(this byte[] bytes)
        {
            return string.Join(string.Empty, bytes.Select(b => b.ToString("X2")));
        }

        private static IEnumerable<byte[]> ReadChunks(string fileName)
        {
            const int MAX_BUFFER = 20971520;// 20

            var filechunk = new byte[MAX_BUFFER];
            int numBytes;
            using var fs = File.OpenRead(fileName);
            var remainBytes = fs.Length;
            var bufferBytes = MAX_BUFFER;

            while (true)
            {
                if (remainBytes <= MAX_BUFFER)
                {
                    filechunk = new byte[remainBytes];
                    bufferBytes = (int)remainBytes;
                }

                if ((numBytes = fs.Read(filechunk, 0, bufferBytes)) > 0)
                {
                    remainBytes -= bufferBytes;
                    yield return filechunk;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
