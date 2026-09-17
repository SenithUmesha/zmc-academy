using System;
using System.Security.Cryptography;

namespace WF_CW2_TEST.Security
{
    internal static class PasswordHasher
    {
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const string Prefix = "PBKDF2";

        public static string Hash(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;
            using (var derive = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                hash = derive.GetBytes(HashSize);
            }

            return string.Join("$",
                Prefix,
                Iterations.ToString(),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static bool Verify(string password, string storedValue)
        {
            if (password == null || string.IsNullOrEmpty(storedValue))
            {
                return false;
            }

            if (!storedValue.StartsWith(Prefix + "$", StringComparison.Ordinal))
            {
                // Compatibility with the original development database.
                // New credentials are always stored as PBKDF2 hashes.
                return string.Equals(password, storedValue, StringComparison.Ordinal);
            }

            string[] parts = storedValue.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out int iterations))
            {
                return false;
            }

            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);
                byte[] actual;

                using (var derive = new Rfc2898DeriveBytes(password, salt, iterations))
                {
                    actual = derive.GetBytes(expected.Length);
                }

                return FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            int difference = 0;
            for (int i = 0; i < left.Length; i++)
            {
                difference |= left[i] ^ right[i];
            }

            return difference == 0;
        }
    }
}
