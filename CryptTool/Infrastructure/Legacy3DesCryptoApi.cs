using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CryptTool.Infrastructure
{
    public static class Legacy3DesCryptoApi
    {
        private const string Provider = "Microsoft RSA SChannel Cryptographic Provider";
        private const uint PROV_RSA_SCHANNEL = 12;
        private const uint CRYPT_VERIFYCONTEXT = 0xF0000000;

        private const uint ALG_CLASS_HASH = 4U << 13;
        private const uint ALG_CLASS_DATA_ENCRYPT = 3U << 13;
        private const uint ALG_TYPE_ANY = 0;
        private const uint ALG_TYPE_BLOCK = 3U << 9;
        private const uint ALG_SID_MD5 = 3;
        private const uint ALG_SID_3DES = 3;

        private const uint CALG_MD5 = ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_MD5;
        private const uint CALG_3DES = ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_BLOCK | ALG_SID_3DES;
        private const uint KEYLENGTH_192 = 192U << 16;

        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool CryptAcquireContext(
            out IntPtr phProv,
            string pszContainer,
            string pszProvider,
            uint dwProvType,
            uint dwFlags);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CryptReleaseContext(IntPtr hProv, uint dwFlags);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CryptCreateHash(
            IntPtr hProv,
            uint Algid,
            IntPtr hKey,
            uint dwFlags,
            out IntPtr phHash);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CryptHashData(
            IntPtr hHash,
            byte[] pbData,
            uint dwDataLen,
            uint dwFlags);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CryptDeriveKey(
            IntPtr hProv,
            uint Algid,
            IntPtr hBaseData,
            uint dwFlags,
            out IntPtr phKey);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CryptEncrypt(
            IntPtr hKey,
            IntPtr hHash,
            bool final,
            uint dwFlags,
            byte[] pbData,
            ref uint pdwDataLen,
            uint dwBufLen);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CryptDecrypt(
            IntPtr hKey,
            IntPtr hHash,
            bool final,
            uint dwFlags,
            byte[] pbData,
            ref uint pdwDataLen);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CryptDestroyHash(IntPtr hHash);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CryptDestroyKey(IntPtr hKey);

        public static void EncryptFile(string inputPath, string outputPath, string key)
        {
            EncryptFile(inputPath, outputPath, key, Encoding.UTF8, true);
        }

        public static void EncryptFile(string inputPath, string outputPath, string key, Encoding keyEncoding, bool overwrite)
        {
            ValidateKey(key);
            ValidateInputFile(inputPath);
            ValidateOutputFile(outputPath, overwrite);

            byte[] plainData = File.ReadAllBytes(inputPath);
            byte[] encryptedData = Transform(plainData, key, keyEncoding ?? Encoding.UTF8, true);

            EnsureDirectory(outputPath);
            File.WriteAllBytes(outputPath, encryptedData);
        }

        public static void DecryptFile(string inputPath, string outputPath, string key)
        {
            DecryptFile(inputPath, outputPath, key, Encoding.UTF8, true);
        }

        public static void DecryptFile(string inputPath, string outputPath, string key, Encoding keyEncoding, bool overwrite)
        {
            ValidateKey(key);
            ValidateInputFile(inputPath);
            ValidateOutputFile(outputPath, overwrite);

            byte[] encryptedData = File.ReadAllBytes(inputPath);
            byte[] plainData = Transform(encryptedData, key, keyEncoding ?? Encoding.UTF8, false);

            EnsureDirectory(outputPath);
            File.WriteAllBytes(outputPath, plainData);
        }

        private static byte[] Transform(byte[] input, string key, Encoding keyEncoding, bool encrypt)
        {
            IntPtr hProv = IntPtr.Zero;
            IntPtr hHash = IntPtr.Zero;
            IntPtr hKey = IntPtr.Zero;

            try
            {
                if (!CryptAcquireContext(out hProv, null, Provider, PROV_RSA_SCHANNEL, CRYPT_VERIFYCONTEXT))
                {
                    throw CreateWin32Exception("CryptAcquireContext");
                }

                if (!CryptCreateHash(hProv, CALG_MD5, IntPtr.Zero, 0, out hHash))
                {
                    throw CreateWin32Exception("CryptCreateHash");
                }

                byte[] keyBytes = keyEncoding.GetBytes(key);
                if (!CryptHashData(hHash, keyBytes, (uint)keyBytes.Length, 0))
                {
                    throw CreateWin32Exception("CryptHashData");
                }

                if (!CryptDeriveKey(hProv, CALG_3DES, hHash, KEYLENGTH_192, out hKey))
                {
                    throw CreateWin32Exception("CryptDeriveKey");
                }

                if (encrypt)
                {
                    byte[] buffer = new byte[input.Length + 16];
                    Buffer.BlockCopy(input, 0, buffer, 0, input.Length);

                    uint dataLength = (uint)input.Length;
                    if (!CryptEncrypt(hKey, IntPtr.Zero, true, 0, buffer, ref dataLength, (uint)buffer.Length))
                    {
                        throw CreateWin32Exception("CryptEncrypt");
                    }

                    byte[] encrypted = new byte[dataLength];
                    Buffer.BlockCopy(buffer, 0, encrypted, 0, (int)dataLength);
                    return encrypted;
                }

                byte[] decryptBuffer = new byte[input.Length];
                Buffer.BlockCopy(input, 0, decryptBuffer, 0, input.Length);

                uint decryptLength = (uint)decryptBuffer.Length;
                if (!CryptDecrypt(hKey, IntPtr.Zero, true, 0, decryptBuffer, ref decryptLength))
                {
                    throw CreateWin32Exception("CryptDecrypt");
                }

                byte[] plain = new byte[decryptLength];
                Buffer.BlockCopy(decryptBuffer, 0, plain, 0, (int)decryptLength);
                return plain;
            }
            finally
            {
                if (hKey != IntPtr.Zero)
                {
                    CryptDestroyKey(hKey);
                }

                if (hHash != IntPtr.Zero)
                {
                    CryptDestroyHash(hHash);
                }

                if (hProv != IntPtr.Zero)
                {
                    CryptReleaseContext(hProv, 0);
                }
            }
        }

        private static Exception CreateWin32Exception(string apiName)
        {
            return new Win32Exception(Marshal.GetLastWin32Error(), apiName + " failed.");
        }

        private static void ValidateInputFile(string inputPath)
        {
            if (string.IsNullOrEmpty(inputPath))
            {
                throw new ArgumentNullException("inputPath");
            }

            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException("Input file was not found.", inputPath);
            }
        }

        private static void ValidateOutputFile(string outputPath, bool overwrite)
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                throw new ArgumentNullException("outputPath");
            }

            if (!overwrite && File.Exists(outputPath))
            {
                throw new IOException("Output file already exists. " + outputPath);
            }
        }

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
        }

        private static void EnsureDirectory(string outputPath)
        {
            string directoryPath = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
}
