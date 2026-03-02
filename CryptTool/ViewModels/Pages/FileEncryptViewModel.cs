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
    public class FileEncryptViewModel : BaseViewModel
    {
        private readonly IFileDialogService _fileDialogService;
        private readonly IFileEncryptService _fileEncryptService;
        public ICommand BrowseCommand { get; private set; }
        public ICommand DecryptCommand { get; private set; }
        public ICommand EncryptCommand { get; private set; }
        public ICommand ClearLogCommand { get; private set; }

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
            _fileDialogService = new FileDialogService();
            _fileEncryptService = new FileEncryptService();

            BrowseCommand = new RelayCommand(OnBrowse);
            DecryptCommand = new RelayCommand(OnDecrypt, CanExecuteCrypto);
            EncryptCommand = new RelayCommand(OnEncrypt, CanExecuteCrypto);
            ClearLogCommand = new RelayCommand(OnClearLog);

            Logs = new ObservableCollection<LogEntryModel>();
        }
        private void OnBrowse(object obj)
        {
            string path = _fileDialogService.OpenFile();

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
            try
            {
                AddLog(LogLevel.Info, "Encrypt start");
                _fileEncryptService.EncryptFile(_filePath);
                AddLog(LogLevel.Info, "Encrypt completed");
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
                _fileEncryptService.DecryptFile(_filePath);
                AddLog(LogLevel.Info, "Decrypt completed");
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
    }
}
