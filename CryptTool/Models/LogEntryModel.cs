using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptTool.Models
{
    public enum LogLevel
    {
        Info,
        Warn,
        Error
    }

    public class LogEntryModel
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
    }
}
