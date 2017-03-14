/*
    PasswordManager
    Copyright (C) 2017 Niels Stockfleth

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
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
        public int MinSymbols { get; set; } = 1;
        public int MinLowerCharacters { get; set; } = 1;
        public int MinUpperCharacters { get; set; } = 1;
        public int MinDigits { get; set; } = 1;

        public SecureString Generate()
        {
            string all = LowerCharacters + UpperCharacters + Symbols + Digits;
            using (var rng = new RNGCryptoServiceProvider())
            {
                int pwdlen = Next(rng, (MaxLength - MinLength) + 1) + MinLength;
                List<int> numbers = new List<int>();
                for (int idx = 0; idx < pwdlen; idx++)
                {
                    numbers.Add(idx); // 0 => 0, 1 => 1, etc
                }
                List<int> positions = new List<int>();
                while (numbers.Count > 0)
                {
                    var nidx = Next(rng, numbers.Count);
                    positions.Add(numbers[nidx]);
                    numbers.RemoveAt(nidx);
                }
                char[] pwd = new char[pwdlen];
                int drawidx = 0;
                for (int cnt = 0; cnt < MinLowerCharacters && drawidx < pwdlen; cnt++)
                {
                    pwd[positions[drawidx++]] = LowerCharacters[Next(rng, LowerCharacters.Length)];
                }
                for (int cnt = 0; cnt < MinUpperCharacters && drawidx < pwdlen; cnt++)
                {
                    pwd[positions[drawidx++]] = UpperCharacters[Next(rng, UpperCharacters.Length)];
                }
                for (int cnt = 0; cnt < MinSymbols && drawidx < pwdlen; cnt++)
                {
                    pwd[positions[drawidx++]] = Symbols[Next(rng, Symbols.Length)];
                }
                for (int cnt = 0; cnt < MinDigits && drawidx < pwdlen; cnt++)
                {
                    pwd[positions[drawidx++]] = Digits[Next(rng, Digits.Length)];
                }
                var count = pwdlen - drawidx;
                for (int idx = 0; idx < count; idx++)
                {
                    pwd[positions[drawidx++]] = all[Next(rng, all.Length)];
                }
                var ret = new SecureString();
                foreach (char c in pwd)
                {
                    ret.AppendChar(c);
                }
                Array.Clear(pwd, 0, pwdlen);
                return ret;
            }
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
