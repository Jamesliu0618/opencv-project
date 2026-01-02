using System;
using System.IO;

namespace PCBInspection.Drivers
{
    public class MockIo : IAdvantechAdapter
    {
        private readonly string _logPath;

        public MockIo(string logPath)
        {
            _logPath = logPath;
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath) ?? ".");
        }

        public void Initialize()
        {
            File.AppendAllText(_logPath, $"[{DateTime.UtcNow:o}] MockIo Initialize\n");
        }

        public bool WriteDigitalOutput(string signalName, bool value, int pulseMs = 100)
        {
            var line = $"[{DateTime.UtcNow:o}] OUT {signalName}={(value?1:0)} pulseMs={pulseMs}\n";
            File.AppendAllText(_logPath, line);
            return true;
        }

        public bool ReadDigitalInput(string signalName)
        {
            File.AppendAllText(_logPath, $"[{DateTime.UtcNow:o}] READ {signalName}\n");
            return false;
        }

        public void Dispose()
        {
            File.AppendAllText(_logPath, $"[{DateTime.UtcNow:o}] MockIo Dispose\n");
        }
    }
}