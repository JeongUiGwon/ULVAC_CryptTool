using CryptTool.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptTool.Services
{
    public class FileEncryptService : IFileEncryptService
    {
        private const string DefaultPassword = "SUYAMA1220";

        public void EncryptFile(string encryptedPath)
        {
            Legacy3DesCryptoApi.EncryptFile(encryptedPath, encryptedPath, DefaultPassword);
            return;
        }
        public void EncryptFile(string encryptedPath, string outputPath)
        {
            Legacy3DesCryptoApi.EncryptFile(encryptedPath, outputPath, DefaultPassword);
            return;
        }
        public void DecryptFile(string encryptedPath)
        {
            Legacy3DesCryptoApi.DecryptFile(encryptedPath, encryptedPath, DefaultPassword);
            return;
        }
        public void DecryptFile(string encryptedPath, string outputPath)
        {
            Legacy3DesCryptoApi.DecryptFile(encryptedPath, outputPath, DefaultPassword);
            return;
        }
    }
}
