using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptTool.Services
{
    public interface IFileEncryptService
    {
        bool EncryptFile(string encryptedPath);
        bool EncryptFile(string encryptedPath, string outputPath);
        bool DecryptFile(string encryptedPath);
        bool DecryptFile(string encryptedPath, string outputPath);
    }
}
