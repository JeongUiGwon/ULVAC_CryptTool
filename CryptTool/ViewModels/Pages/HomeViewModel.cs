using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Resources;

namespace CryptTool.ViewModels.Pages
{
    public class HomeViewModel : BaseViewModel
    {
        private string _version;
        private string _releaseNoteText;

        public string Version
        {
            get { return _version; }
            private set { SetProperty(ref _version, value, "Version"); }
        }

        public string ReleaseNoteText
        {
            get { return _releaseNoteText; }
            private set { SetProperty(ref _releaseNoteText, value, "ReleaseNoteText"); }
        }

        public HomeViewModel()
        {
            LoadVersion();
            LoadReleaseNoteFromResource("/Resources/ReleaseNote_v1.0.0.txt");
        }

        private void LoadVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            Version = string.Format("v{0}.{1}.{2}",
                                    version.Major,
                                    version.Minor,
                                    version.Build);
        }

        private void LoadReleaseNoteFromResource(string resourcePath)
        {
            try
            {
                Uri uri = new Uri(resourcePath, UriKind.Relative);
                StreamResourceInfo info = Application.GetResourceStream(uri);

                if (info == null || info.Stream == null)
                {
                    ReleaseNoteText = "Release Note 리소스를 찾을 수 없습니다.\r\n(" + resourcePath + ")";
                    return;
                }

                using (StreamReader reader = new StreamReader(info.Stream))
                {
                    ReleaseNoteText = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                ReleaseNoteText = "Release Note 로드 중 오류가 발생했습니다.\r\n" + ex.Message;
            }
        }
    }
}
