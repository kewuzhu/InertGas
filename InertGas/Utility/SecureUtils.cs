using System;
using System.Net;
using System.Security;
using System.Security.Cryptography;

namespace InertGas.Application.Utility
{
    internal class SecureUtils
    {
        public static bool ValidateUserPassword(string expected, SecureString actual, byte[] salt) =>
            expected.Equals(HashPassword(actual, salt));

        public static string HashPassword(SecureString securePassword, byte[] salt)
        {
            var password = new NetworkCredential("", securePassword).Password;
            var key = new Rfc2898DeriveBytes(password, salt);
            return Convert.ToHexString(key.GetBytes(16));
        }
    }
}
