using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptTool.Services
{
    public interface IFileEncryptService
    {
        void EncryptFile(string encryptedPath);
        void EncryptFile(string encryptedPath, string outputPath);
        void DecryptFile(string encryptedPath);
        void DecryptFile(string encryptedPath, string outputPath);
    }
}
