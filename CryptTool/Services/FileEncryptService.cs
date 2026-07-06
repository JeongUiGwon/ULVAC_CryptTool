using CryptTool.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptTool.Services
{
    public class FileEncryptService : IFileEncryptService
    {
        private string DefaultPassword = Properties.Settings.Default.CryptoPassword;

        public bool EncryptFile(string encryptedPath)
        {
            return Legacy3DesCryptoApi.EncryptFile(encryptedPath, encryptedPath, DefaultPassword);
        }
        public bool EncryptFile(string encryptedPath, string outputPath)
        {
            return Legacy3DesCryptoApi.EncryptFile(encryptedPath, outputPath, DefaultPassword);
        }
        public bool DecryptFile(string encryptedPath)
        {
            return Legacy3DesCryptoApi.DecryptFile(encryptedPath, encryptedPath, DefaultPassword);
        }
        public bool DecryptFile(string encryptedPath, string outputPath)
        {
            return Legacy3DesCryptoApi.DecryptFile(encryptedPath, outputPath, DefaultPassword);
        }
    }
}
