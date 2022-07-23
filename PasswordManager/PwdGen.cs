/*
    Myna Password Manager
    Copyright (C) 2017-2022 Niels Stockfleth

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
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager
{
    public sealed class PwdGen
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

        public string Generate()
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
            List<int> numbers = new List<int>();
            for (int idx = 0; idx < Length; idx++)
            {
                numbers.Add(idx); // 0 => 0, 1 => 1, etc
            }
            List<int> positions = new List<int>();
            while (numbers.Count > 0)
            {
                var nidx = numbers.Count == 1 ? 0 : Next(numbers.Count);
                positions.Add(numbers[nidx]);
                numbers.RemoveAt(nidx);
            }
            char[] pwd = new char[Length];
            int drawidx = 0;
            Draw(pwd, ref drawidx, MinLowerCharacters, LowerCharacters, Length, positions);
            Draw(pwd, ref drawidx, MinUpperCharacters, UpperCharacters, Length, positions);
            Draw(pwd, ref drawidx, MinSymbols, Symbols, Length, positions);
            Draw(pwd, ref drawidx, MinDigits, Digits, Length, positions);
            Draw(pwd, ref drawidx, Length - drawidx, all, Length, positions);
            string ret = new string(pwd);
            Array.Clear(pwd, 0, pwd.Length);
            return ret;
        }

        private void Draw(  char[] pwd,
                            ref int drawidx,
                            int drawcnt,
                            string symbols,
                            int pwdlen,
                            List<int> positions)
        {
            if (symbols.Length > 0 && drawcnt > 0)
            {
                for (int cnt = 0; cnt < drawcnt && drawidx < pwdlen; cnt++)
                {
                    pwd[positions[drawidx++]] = symbols[Next(symbols.Length)];
                }
            }
        }

        private int Next(int upper_limit)
        {
            if (upper_limit <= 0)
            {
                throw new ArgumentException($"Invalid upper limit {upper_limit}.");
            }
            if (upper_limit == 1)
            {
                return 0;
            }
            return (int)(Next() % (uint)upper_limit);
        }

        private uint Next()
        {
            byte[] randomNumber = RandomNumberGenerator.GetBytes(4);
            return BitConverter.ToUInt32(randomNumber, 0);
        }
    }
}
