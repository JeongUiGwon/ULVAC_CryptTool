using CryptTool.Infrastructure;
using CryptTool.Models;
using CryptTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CryptTool.ViewModels.Pages
{
    public class LogEncryptViewModel : BaseViewModel
    {
        private const string DefaultLogPath = @"D:\ENTRON\HISTORY";

        private readonly IFileSystemService _fileSystemService;
        private readonly IFolderSystemService _folderSystemService;
        private readonly IFileEncryptService _fileEncryptService;
        public ICommand BrowseCommand { get; private set; }
        public ICommand DecryptCommand { get; private set; }
        public ICommand EncryptCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        private string _folderPath;
        public string FolderPath
        {
            get
            {
                return _folderPath;
            }
            set
            {
                if (SetProperty(ref _folderPath, value, () => FolderPath))
                {
                    UpdateCommandState();
                }
            }
        }
        private ObservableCollection<FileInfo> _files;
        public ObservableCollection<FileInfo> Files
        {
            get { return _files; }
            private set { SetProperty(ref _files, value, "Files"); }
        }

        private string _summaryText;
        public string SummaryText
        {
            get { return _summaryText; }
            private set { SetProperty(ref _summaryText, value, "SummaryText"); }
        }

        private string _currentWorkText;
        public string CurrentWorkText
        {
            get { return _currentWorkText; }
            private set { SetProperty(ref _currentWorkText, value, "CurrentWorkText"); }
        }

        private int _progressMax;
        public int ProgressMax
        {
            get { return _progressMax; }
            private set { SetProperty(ref _progressMax, value, "ProgressMax"); }
        }
        private int _progressValue;
        public int ProgressValue
        {
            get { return _progressValue; }
            private set { SetProperty(ref _progressValue, value, "ProgressValue"); }
        }

        private BackgroundWorker _worker;
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                if (SetProperty(ref _isBusy, value, "IsBusy"))
                {
                    UpdateCommandState();
                }
            }
        }
        public LogEncryptViewModel()
        {
            _fileSystemService = new FileSystemService();
            _folderSystemService = new FolderSystemService();
            _fileEncryptService = new FileEncryptService();

            Files = new ObservableCollection<FileInfo>();

            BrowseCommand = new RelayCommand(OnBrowse);
            RefreshCommand = new RelayCommand(OnRefresh);
            DecryptCommand = new RelayCommand(OnDecrypt, CanExecuteCrypto);
            EncryptCommand = new RelayCommand(OnEncrypt, CanExecuteCrypto);

            InitializeDefaultFolder();
        }
        private void OnBrowse(object obj)
        {
            string path = _folderSystemService.BrowseFolder();

            if (!string.IsNullOrEmpty(path))
            {
                FolderPath = path;
            }
            else
            {
                return;
            }

            LoadFiles();
        }
        private void OnRefresh(object obj)
        {
            LoadFiles();
        }

        private void OnEncrypt(object obj)
        {
            StartCryptoWork(true);
        }

        private void OnDecrypt(object obj)
        {
            StartCryptoWork(false);
        }

        private bool CanExecuteCrypto(object obj)
        {
            if (IsBusy)
            {
                return false;
            }

            if (string.IsNullOrEmpty(FolderPath))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void StartCryptoWork(bool encrypt)
        {
            if (IsBusy)
            {
                return;
            }

            if (Files == null || Files.Count == 0)
            {
                return;
            }

            IsBusy = true;

            ProgressMax = Files.Count;
            ProgressValue = 0;
            CurrentWorkText = "Starting...";

            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                DoCryptoWork((BackgroundWorker)sender, encrypt);
            };
            _worker.ProgressChanged += OnWorkerProgressChanged;
            _worker.RunWorkerCompleted += OnWorkerCompleted;

            _worker.RunWorkerAsync();
        }
        private void DoCryptoWork(BackgroundWorker bw, bool encrypt)
        {
            for (int i = 0; i < Files.Count; i++)
            {
                FileInfo item = Files[i];

                string path = item.FullName;

                WorkReport report = new WorkReport();
                report.Index = i;
                report.Total = Files.Count;
                report.Path = path;

                try
                {
                    ProcessInPlace(path, encrypt);     // 실제 Encrypt/Decrypt 수행
                    report.Success = true;
                    report.Message = encrypt ? "Encrypted" : "Decrypted";
                }
                catch (Exception ex)
                {
                    report.Success = false;
                    report.Message = "Failed: " + ex.Message;
                }

                bw.ReportProgress(i + 1, report);
            }
        }
        private void OnWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressValue = e.ProgressPercentage;

            WorkReport report = e.UserState as WorkReport;
            if (report == null)
            {
                return;
            }

            CurrentWorkText = string.Format("{0}/{1} : {2} ({3})",
                                            report.Index + 1,
                                            report.Total,
                                            report.Path,
                                            report.Message);
        }
        private void OnWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                CurrentWorkText = "Error: " + e.Error.Message;
            }
            else
            {
                CurrentWorkText = "Done.";
            }

            IsBusy = false;
        }
        private void ProcessInPlace(string path, bool encrypt)
        {
            string tempPath = path + ".tmp";

            try
            {
                if (System.IO.File.Exists(tempPath))
                {
                    System.IO.File.Delete(tempPath);
                }
            }
            catch
            {
            }

            if (encrypt)
            {
                _fileEncryptService.EncryptFile(path, tempPath);
            }
            else
            {
                _fileEncryptService.DecryptFile(path, tempPath);
            }

            try
            {
                System.IO.File.Replace(tempPath, path, null, true);
            }
            catch
            {
                try
                {
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
                catch
                {
                }

                System.IO.File.Move(tempPath, path);
            }
        }

        private void LoadFiles()
        {
            Files.Clear();

            if (string.IsNullOrEmpty(FolderPath))
            {
                return;
            }

            // PMC LOG
            var paths = _fileSystemService.GetFiles(FolderPath + "\\LOG", true, "LDT*.CSV");
            for (int i = 0; i < paths.Count; i++)
            {
                Files.Add(new FileInfo(paths[i]));
            }

            // SMC LOG
            paths = _fileSystemService.GetFiles(FolderPath + "\\SMC_LOG", true, "LDT*.CSV");
            for (int i = 0; i < paths.Count; i++)
            {
                Files.Add(new FileInfo(paths[i]));
            }

            // MAN LOG
            paths = _fileSystemService.GetFiles(FolderPath + "\\PRCS", true, "LDT*.CSV");
            for (int i = 0; i < paths.Count; i++)
            {
                Files.Add(new FileInfo(paths[i]));
            }

            SummaryText = "Found " + Files.Count + " files";
            ProgressMax = Files.Count;
            ProgressValue = 0;
        }

        private void UpdateCommandState()
        {
            RelayCommand encrypt = EncryptCommand as RelayCommand;
            RelayCommand decrypt = DecryptCommand as RelayCommand;

            if (encrypt != null)
            {
                encrypt.RaiseCanExecuteChanged();
            }

            if (decrypt != null)
            {
                decrypt.RaiseCanExecuteChanged();
            }
        }
        private void InitializeDefaultFolder()
        {
            try
            {
                if (Directory.Exists(DefaultLogPath))
                {
                    FolderPath = DefaultLogPath;
                    LoadFiles();
                }
            }
            catch
            {
                // 예외 발생해도 프로그램 죽지 않도록 무시
            }
        }
    }
}
