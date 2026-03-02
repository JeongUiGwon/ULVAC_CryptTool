using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CryptTool.Services
{
    public class FolderSystemService : IFolderSystemService
    {
        public string BrowseFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = true;

                DialogResult result = dialog.ShowDialog();

                if (result != DialogResult.OK)
                {
                    return null;
                }

                return dialog.SelectedPath;
            }
        }
    }
}
