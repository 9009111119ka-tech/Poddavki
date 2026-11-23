
using System;
using System.Security.Cryptography;

namespace Poddavki.Data
{
    public static class PasswordHasher
    {
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static void CreateHash(
            string password,
            out string hash,
            out string salt)
        {
            byte[] saltBytes = new byte[SaltSize];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(saltBytes);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password, saltBytes, Iterations))
            {
                hash = Convert.ToBase64String(
                    pbkdf2.GetBytes(HashSize));
            }

            salt = Convert.ToBase64String(saltBytes);
        }

        public static bool Verify(
            string password,
            string hash,
            string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] expectedHash = Convert.FromBase64String(hash);

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password, saltBytes, Iterations))
            {
                byte[] actualHash = pbkdf2.GetBytes(HashSize);

                if (actualHash.Length != expectedHash.Length)
                    return false;

                int difference = 0;

                for (int i = 0; i < actualHash.Length; i++)
                    difference |= actualHash[i] ^ expectedHash[i];

                return difference == 0;
            }
        }
    }
}
