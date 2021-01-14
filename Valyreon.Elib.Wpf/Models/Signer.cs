using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Valyreon.Elib.Wpf.Models
{
	public static class Signer
	{
		public static string ComputeHash(byte[] file)
		{
			using var stream = new MemoryStream(file);
			var sha = new SHA256Managed();
			return sha.ComputeHash(stream).ToHex();
		}

		private static string ToHex(this byte[] bytes)
		{
			var result = new StringBuilder();

			for(var i = 0 ; i < bytes.Length ; i++)
			{
				result.Append(bytes[i].ToString("X2"));
			}

			return result.ToString();
		}
	}
}
