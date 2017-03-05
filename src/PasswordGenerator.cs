using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager
{
    /// <summary>
    /// Simple password generator that guarantees that a password contains at least
    /// one upper character, one lower character, one symbol and one digit.
    /// </summary>
    public sealed class PasswordGenerator
    {
        public string LowerCharacters { get; set; } = "abcdefghijklmnopqrstuvwxyz";
        public string UpperCharacters { get; set; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public string Symbols { get; set; } = "!@$()=+-,:.";
        public string Digits { get; set; } = "0123456789";        
        public int MinLength { get; set; } = 8;
        public int MaxLength { get; set; } = 16;

        public SecureString Generate()
        {
            string all = LowerCharacters + UpperCharacters + Symbols + Digits;
            var pwd = new SecureString();
            using (var rng = new RNGCryptoServiceProvider())
            {
                int pwdlen = Next(rng, (MaxLength - MinLength) + 1) + MinLength;
                pwd.AppendChar(UpperCharacters[Next(rng, UpperCharacters.Length)]);
                pwd.AppendChar(Symbols[Next(rng, Symbols.Length)]);
                pwd.AppendChar(Digits[Next(rng, Digits.Length)]);
                for (int idx = 0; idx < pwdlen - 4; idx++)
                {
                    pwd.AppendChar(all[Next(rng, all.Length)]);
                }
                pwd.AppendChar(LowerCharacters[Next(rng, LowerCharacters.Length)]);
            }
            return pwd;
        }

        private int Next(RNGCryptoServiceProvider rng, int upper_limit)
        {
            return (int)(Next(rng) % (uint)upper_limit);
        }

        private uint Next(RNGCryptoServiceProvider rng)
        {
            byte[] randomNumber = new byte[4];
            rng.GetBytes(randomNumber);
            return BitConverter.ToUInt32(randomNumber, 0);
        }
    }
}
