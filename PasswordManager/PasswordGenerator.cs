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
        public int Length { get; set; } = 16;
        public int MinSymbols { get; set; } = 1;
        public int MinLowerCharacters { get; set; } = 1;
        public int MinUpperCharacters { get; set; } = 1;
        public int MinDigits { get; set; } = 1;

        public SecureString Generate()
        {
            StringBuilder sb = new StringBuilder();
            if (MinLowerCharacters > 0)
            {
                sb.Append(LowerCharacters);
            }
            if (MinUpperCharacters > 0)
            {
                sb.Append(UpperCharacters);
            }
            if (MinSymbols > 0)
            {
                sb.Append(Symbols);
            }
            if (MinDigits > 0)
            {
                sb.Append(Digits);
            }
            string all = sb.ToString();
            using (var rng = new RNGCryptoServiceProvider())
            {
                List<int> numbers = new List<int>();
                for (int idx = 0; idx < Length; idx++)
                {
                    numbers.Add(idx); // 0 => 0, 1 => 1, etc
                }
                List<int> positions = new List<int>();
                while (numbers.Count > 0)
                {
                    var nidx = numbers.Count == 1 ? 0 : Next(rng, numbers.Count);
                    positions.Add(numbers[nidx]);
                    numbers.RemoveAt(nidx);
                }
                char[] pwd = new char[Length];
                int drawidx = 0;
                Draw(rng, pwd, ref drawidx, MinLowerCharacters, LowerCharacters, Length, positions);
                Draw(rng, pwd, ref drawidx, MinUpperCharacters, UpperCharacters, Length, positions);
                Draw(rng, pwd, ref drawidx, MinSymbols, Symbols, Length, positions);
                Draw(rng, pwd, ref drawidx, MinDigits, Digits, Length, positions);
                Draw(rng, pwd, ref drawidx, Length - drawidx, all, Length, positions);
                var ret = new SecureString();
                foreach (char c in pwd)
                {
                    ret.AppendChar(c);
                }
                Array.Clear(pwd, 0, Length);
                return ret;
            }
        }

        private void Draw(  RNGCryptoServiceProvider    rng,
                            char []                     pwd,
                            ref int                     drawidx,
                            int                         drawcnt,
                            string                      symbols,
                            int                         pwdlen,
                            List<int>                   positions)
        {
            if (symbols.Length > 0 && drawcnt > 0)
            {
                for (int cnt = 0; cnt < drawcnt && drawidx < pwdlen; cnt++)
                {
                    pwd[positions[drawidx++]] = symbols[Next(rng, symbols.Length)];
                }
            }
        }

        private int Next(RNGCryptoServiceProvider rng, int upper_limit)
        {
            if (upper_limit <= 0)
            {
                throw new ArgumentException($"Invalid upper limit {upper_limit}.");
            }
            if (upper_limit == 1)
            {
                return 0;
            }
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
