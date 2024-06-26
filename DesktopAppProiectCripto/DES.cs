using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace DesktopAppProiectCripto
{
    public class DES
    {
        public static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText), "Plaintext cannot be null or empty.");
            if (string.IsNullOrEmpty(key) || key.Length != 8)
                throw new ArgumentException("Key must be exactly 8 bytes long.");

            try
            {
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    byte[] keyBytes = Encoding.ASCII.GetBytes(key);
                    des.Key = keyBytes;
                    des.IV = keyBytes;

                    using (MemoryStream memoryStream = new MemoryStream())
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, des.CreateEncryptor(), CryptoStreamMode.Write))
                    using (StreamWriter writer = new StreamWriter(cryptoStream))
                    {
                        writer.Write(plainText);
                        writer.Flush();
                        cryptoStream.FlushFinalBlock();
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Encryption failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public static string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText), "Ciphertext cannot be null or empty.");
            if (string.IsNullOrEmpty(key) || key.Length != 8)
                throw new ArgumentException("Key must be exactly 8 bytes long.");

            try
            {
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    byte[] keyBytes = Encoding.ASCII.GetBytes(key);
                    des.Key = keyBytes;
                    des.IV = keyBytes;

                    using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cipherText)))
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, des.CreateDecryptor(), CryptoStreamMode.Read))
                    using (StreamReader reader = new StreamReader(cryptoStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (CryptographicException ex)
            {
                MessageBox.Show($"Decryption failed. Invalid key or corrupted ciphertext: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Decryption failed. Invalid base64 string: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
