using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptTool.Services
{
    public interface IFileSystemService
    {
        string BrowseFile();
        IList<string> GetFiles(string folderPath, bool includeSubfolders, string searchPattern);
    }
}
