using System.Text;
using System.Security.Cryptography;


    public static class Cryptography
    {
        public static string ToMd5(string value)
        {
            return Hash(value, MD5.Create());
        }

        public static string ToSha512(string value)
        {
            using (var sha512 = SHA512.Create())
            {
                return Hash(value, sha512);
            }
        }

        private static string Hash(string value, HashAlgorithm hash)
        {
            var bytes = hash.ComputeHash(Encoding.ASCII.GetBytes(value));
            var sb = new StringBuilder();

            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }

