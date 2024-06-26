using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DesktopAppProiectCripto
{
    public class RC5
    {
        private const int W = 32; 
        private const int R = 12; 
        private const int B = 16; 
        private uint[] L; 
        private uint[] S; 
        private readonly byte[] originalKeyHash; 
        private const uint Pw = 0xb7e15163, Qw = 0x9e3779b9; 

        public RC5(string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(B, '\0'));
            originalKeyHash = ComputeHash(keyBytes); 
            KeyExpansion(keyBytes);
        }

        private byte[] ComputeHash(byte[] key)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(key);
            }
        }

        private void KeyExpansion(byte[] key)
        {
            L = new uint[(key.Length + 3) / 4];
            Buffer.BlockCopy(key, 0, L, 0, key.Length);
            S = new uint[2 * (R + 1)];
            S[0] = Pw;
            for (int i = 1; i < S.Length; i++)
            {
                S[i] = S[i - 1] + Qw;
            }

            uint A = 0, B = 0;
            int x = 0, y = 0;

            for (int k = 0; k < 3 * S.Length; k++)
            {
                A = RotateLeft(S[x] + A + B, 3);
                B = RotateLeft(L[y] + A + B, (int)(A + B) % W);
                S[x] = A;
                L[y] = B;
                x = (x + 1) % S.Length;
                y = (y + 1) % L.Length;
            }
        }

        public string Encrypt(string plaintext)
        {
            byte[] data = Encoding.UTF8.GetBytes(plaintext);
            uint[] dataUint = new uint[(data.Length + 3) / 4 * 2]; 
            Buffer.BlockCopy(data, 0, dataUint, 0, data.Length);

            for (int i = 0; i < dataUint.Length; i += 2)
            {
                uint A = dataUint[i] + S[0];
                uint B = dataUint[i + 1] + S[1];
                for (int j = 1; j <= R; j++)
                {
                    A = RotateLeft(A ^ B, (int)B) + S[2 * j];
                    B = RotateLeft(B ^ A, (int)A) + S[2 * j + 1];
                }
                dataUint[i] = A;
                dataUint[i + 1] = B;
            }

            byte[] encrypted = new byte[dataUint.Length * 4];
            Buffer.BlockCopy(dataUint, 0, encrypted, 0, encrypted.Length);
            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string ciphertext, string key)
        {
            if (!VerifyKey(key))
            {
                return null; 
            }

            byte[] encrypted = Convert.FromBase64String(ciphertext);
            uint[] dataUint = new uint[encrypted.Length / 4];
            Buffer.BlockCopy(encrypted, 0, dataUint, 0, encrypted.Length);

            for (int i = 0; i < dataUint.Length; i += 2)
            {
                uint A = dataUint[i], B = dataUint[i + 1];
                for (int j = R; j > 0; j--)
                {
                    B = RotateRight(B - S[2 * j + 1], (int)A) ^ A;
                    A = RotateRight(A - S[2 * j], (int)B) ^ B;
                }
                A -= S[0];
                B -= S[1];
                dataUint[i] = A;
                dataUint[i + 1] = B;
            }

            byte[] decrypted = new byte[dataUint.Length * 4];
            Buffer.BlockCopy(dataUint, 0, decrypted, 0, decrypted.Length);
            return Encoding.UTF8.GetString(decrypted).TrimEnd('\0');
        }

        private bool VerifyKey(string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(B, '\0'));
            byte[] keyHash = ComputeHash(keyBytes);
            return keyHash.SequenceEqual(originalKeyHash);
        }

        private uint RotateLeft(uint value, int shift) => (value << shift) | (value >> (32 - shift));
        private uint RotateRight(uint value, int shift) => (value >> shift) | (value << (32 - shift));
    }
}
