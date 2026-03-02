using CryptTool.Infrastructure;
using CryptTool.Models;
using CryptTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CryptTool.ViewModels.Pages
{
    public class FolderEncryptViewModel : BaseViewModel
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly IFolderSystemService _folderSystemService;
        private readonly IFileEncryptService _fileEncryptService;
        public ICommand BrowseCommand { get; private set; }
        public ICommand DecryptCommand { get; private set; }
        public ICommand EncryptCommand { get; private set; }
        public ICommand ClearLogCommand { get; private set; }

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
        private LogEntryModel _selectedLog;
        public LogEntryModel SelectedLog
        {
            get
            {
                return _selectedLog;
            }
            set
            {
                SetProperty(ref _selectedLog, value, () => SelectedLog);
            }
        }
        public ObservableCollection<LogEntryModel> Logs { get; private set; }
        public FolderEncryptViewModel()
        {
            _fileSystemService = new FileSystemService();
            _folderSystemService = new FolderSystemService();
            _fileEncryptService = new FileEncryptService();

            BrowseCommand = new RelayCommand(OnBrowse);
            DecryptCommand = new RelayCommand(OnDecrypt, CanExecuteCrypto);
            EncryptCommand = new RelayCommand(OnEncrypt, CanExecuteCrypto);
            ClearLogCommand = new RelayCommand(OnClearLog);

            Logs = new ObservableCollection<LogEntryModel>();
        }
        private void OnBrowse(object obj)
        {
            string path = _folderSystemService.BrowseFolder();

            if (!string.IsNullOrEmpty(path))
            {
                FolderPath = path;
                AddLog(LogLevel.Info, "Folder selected: " + path);
            }
            else
            {
                AddLog(LogLevel.Info, "Folder selection canceled.");
            }
        }

        private void OnEncrypt(object obj)
        {
            try
            {
                AddLog(LogLevel.Info, "Encrypt start");

                string[] files = _fileSystemService.GetFiles(FolderPath, true, "*.*").ToArray();
                foreach (string file in files)
                {
                    _fileEncryptService.EncryptFile(file);
                    AddLog(LogLevel.Info, file + "Encrypt completed");
                }

                AddLog(LogLevel.Info, "All files have been encrypted.");
            }
            catch (Exception ex)
            {
                AddLog(LogLevel.Error, "Encrypt failed: " + ex.Message);
            }
        }

        private void OnDecrypt(object obj)
        {
            try
            {
                AddLog(LogLevel.Info, "Decrypt start");

                string[] files = _fileSystemService.GetFiles(FolderPath, true, "*.*").ToArray();
                foreach (string file in files)
                {
                    _fileEncryptService.DecryptFile(file);
                    AddLog(LogLevel.Info, file + "Decrypt completed");
                }
                
                AddLog(LogLevel.Info, "All files have been decrypted.");
            }
            catch (Exception ex)
            {
                AddLog(LogLevel.Error, "Decrypt failed: " + ex.Message);
            }
        }
        private bool CanExecuteCrypto(object obj)
        {
            if (string.IsNullOrEmpty(FolderPath))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void OnClearLog(object obj)
        {
            Logs.Clear();
        }

        private void AddLog(LogLevel level, string message)
        {
            Logs.Add(new LogEntryModel
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            });

            SelectedLog = Logs.Count > 0 ? Logs[Logs.Count - 1] : null;
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
    }
}
