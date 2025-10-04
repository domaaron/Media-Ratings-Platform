using Isopoh.Cryptography.Argon2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Infrastructure.security
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            return Argon2.Hash(password);
        }

        public static bool Verify(string password, string encodedHash)
        {
            return Argon2.Verify(encodedHash, password);
        }
    }
}
