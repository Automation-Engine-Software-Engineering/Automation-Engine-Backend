using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tools.AuthoraizationTools
{
    public static class HashString
    {
        public static string HashPassword(string password, string? salt)
        {
            string passwordWithSalt = password + salt;

            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
                string hashedPassword = Convert.ToBase64String(hashBytes);

                return hashedPassword;
            }
        }
        public static string GetSalt()
        {
            string salt = Guid.NewGuid().ToString();
            return salt;
        }

    }
}
