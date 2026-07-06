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
    public class FileEncryptViewModel : BaseViewModel
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly IFileEncryptService _fileEncryptService;
        public ICommand BrowseCommand { get; private set; }
        public ICommand DecryptCommand { get; private set; }
        public ICommand EncryptCommand { get; private set; }
        public ICommand ClearLogCommand { get; private set; }
        public ICommand FileDropCommand { get; private set; }

        private string _filePath;
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                if (SetProperty(ref _filePath, value, () => FilePath))
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
        public FileEncryptViewModel()
        {
            _fileSystemService = new FileSystemService();
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
            string path = _fileSystemService.BrowseFile();

            if (!string.IsNullOrEmpty(path))
            {
                FilePath = path;
                AddLog(LogLevel.Info, "File selected: " + path);
            }
            else
            {
                AddLog(LogLevel.Info, "File selection canceled.");
            }
        }

        private void OnEncrypt(object obj)
        {
            bool isSuccess = false;

            try
            {
                AddLog(LogLevel.Info, "Encrypt start");
                isSuccess = _fileEncryptService.EncryptFile(_filePath);

                if (isSuccess)
                {
                    AddLog(LogLevel.Info, "Encrypt completed");
                }
                else
                {
                    AddLog(LogLevel.Error, "Encrypt failed");
                }
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
                isSuccess = _fileEncryptService.DecryptFile(_filePath);
                if(isSuccess)
                {
                    AddLog(LogLevel.Info, "Decrypt completed");
                }
                else
                {
                    AddLog(LogLevel.Error, "Decrypt failed");
                }
            }
            catch (Exception ex)
            {
                AddLog(LogLevel.Error, "Decrypt failed: " + ex.Message);
            }
        }
        private bool CanExecuteCrypto(object obj)
        {
            if (string.IsNullOrEmpty(FilePath))
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
                MessageBox.Show("유효한 경로가 아닙니다. 파일을 다시 드롭해주세요.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(filePath))
            {
                MessageBox.Show("유효한 경로가 아닙니다. 파일을 다시 드롭해주세요.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            FilePath = filePath;
        }
    }
}
