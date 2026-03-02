using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CryptTool.Services
{
    public class FileSystemService : IFileSystemService
    {
        public string BrowseFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files (*.*)|*.*";
            dlg.Multiselect = false;

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                return dlg.FileName;
            }
            else
            {
                return null;
            }
        }
        public IList<string> GetFiles(string folderPath, bool includeSubfolders, string searchPattern)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                throw new ArgumentException("folderPath");
            }

            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException(folderPath);
            }

            string pattern = string.IsNullOrEmpty(searchPattern)
                             ? "*.*"
                             : searchPattern;

            SearchOption option = includeSubfolders
                                  ? SearchOption.AllDirectories
                                  : SearchOption.TopDirectoryOnly;

            string[] files = Directory.GetFiles(folderPath, pattern, option);

            return new List<string>(files);
        }
    }
}
