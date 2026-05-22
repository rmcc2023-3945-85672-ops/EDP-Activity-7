using System;
using System.Security.Cryptography;
using System.Text;

namespace LibrarySystem.Utils
{
    public static class PasswordHelper
    {
        public static string Hash(string plainText)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            return Convert.ToBase64String(bytes);
        }

        public static bool Verify(string plainText, string hashed) =>
            Hash(plainText) == hashed;

        public static string GeneratePin()
        {
            var rng = new Random();
            return rng.Next(100000, 999999).ToString();
        }
    }
}
