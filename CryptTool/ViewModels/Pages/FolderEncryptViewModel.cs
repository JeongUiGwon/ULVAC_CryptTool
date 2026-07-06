using CryptTool.Infrastructure;
using CryptTool.Models;
using CryptTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
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
        public ICommand FileDropCommand { get; private set; }

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
            FileDropCommand = new RelayCommand(ExecuteDropFile);

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
            bool isSuccess = false;

            try
            {
                AddLog(LogLevel.Info, "Encrypt start");

                string[] files = _fileSystemService.GetFiles(FolderPath, true, "*.*").ToArray();
                foreach (string file in files)
                {
                    isSuccess = _fileEncryptService.EncryptFile(file);
                    if (isSuccess)
                    {
                        AddLog(LogLevel.Info, file + "Encrypt completed");
                    }
                    else
                    {
                        AddLog(LogLevel.Error, file + "Encrypt failed");
                    }
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
            bool isSuccess = false;

            try
            {
                AddLog(LogLevel.Info, "Decrypt start");

                string[] files = _fileSystemService.GetFiles(FolderPath, true, "*.*").ToArray();
                foreach (string file in files)
                {
                    isSuccess = _fileEncryptService.DecryptFile(file);
                    if (isSuccess)
                    {
                        AddLog(LogLevel.Info, file + "Decrypt completed");
                    }
                    else
                    {
                        AddLog(LogLevel.Error, file + "Decrypt failed");
                    }
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
        private void ExecuteDropFile(object parameter)
        {
            string filePath = parameter as string;

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("유효한 경로가 아닙니다. 폴더를 다시 드롭해주세요.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(filePath))
            {
                MessageBox.Show("유효한 경로가 아닙니다. 폴더를 다시 드롭해주세요.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            FolderPath = filePath;
        }
    }
}
