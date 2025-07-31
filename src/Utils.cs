using System.Diagnostics;

namespace WoWCombatLogSplit.src
{
    internal class FileUtils
    {
        public static void SetAttributes(Microsoft.Win32.SafeHandles.SafeFileHandle fileHandle, DateTime created, DateTime modified)
        {
            try
            {
                File.SetCreationTime(fileHandle, created);
                File.SetLastWriteTime(fileHandle, modified);
                File.SetLastAccessTime(fileHandle, modified);
            }
            catch (Exception ex)
            {
                ProgramUtils.Exception(ex, null);
            }
        }
    }
    internal class FormatUtils
    {
        private static readonly string[] FileSizes = ["B", "KB", "MB", "GB", "TB", "PB"];
        public static string GetDateTime(DateTime dateTime, string format)
        {
            return dateTime.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
        }
        public static string GetDuration(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 1)
                return "~1s";
            var parts = new List<string>();
            if (timeSpan.Days > 0)
                parts.Add($"{timeSpan.Days}d");
            if (timeSpan.Hours > 0)
                parts.Add($"{timeSpan.Hours}h");
            if (timeSpan.Minutes > 0)
                parts.Add($"{timeSpan.Minutes}m");
            if (timeSpan.Seconds > 0)
                parts.Add($"{timeSpan.Seconds}s");
            if (timeSpan.TotalSeconds < 1 && timeSpan.Milliseconds > 0)
                parts.Add($"{timeSpan.Milliseconds}ms");
            return string.Join(" ", parts);
        }
        public static string GetFileSize(long bytes)
        {
            double length = bytes;
            int order = 0;
            while (length >= 1024 && order < FileSizes.Length - 1)
            {
                order++;
                length /= 1024;
            }
            return $"{length:0.##} {FileSizes[order]}";
        }
    }
    internal class LogUtils
    {
        private delegate bool CheckFunc(char chr);
        private static bool IsDigit(char chr)
        {
            return chr >= '0' && chr <= '9';
        }
        private static bool IsDateDelim(char chr)
        {
            return chr == '/';
        }
        private static bool IsTimeDelim(char chr)
        {
            return chr == ':';
        }
        private static bool IsSpace(char chr)
        {
            return chr == ' ';
        }
        private static bool IsNewLine(char chr)
        {
            return chr == '\r' || chr == '\n';
        }
        private static bool IsNullByte(char chr)
        {
            return chr == '\0';
        }
        private static bool IsDot(char chr)
        {
            return chr == '.';
        }
        private static int CheckUntil(char[] chars, int offset, int min, int max, CheckFunc check)
        {
            int count = 0;
            for (int i = offset; i < chars.Length; i++)
            {
                var chr = chars[i];
                if (!check(chr))
                {
                    if (count >= min)
                    {
                        break;
                    }
                    return -1;
                }
                if (++count == max)
                {
                    break;
                }
            }
            return count;
        }
        private static int IsNumDigits(char[] chars, int offset, int min, int max)
        {
            return CheckUntil(chars, offset, min, max, IsDigit);
        }
        private static int IsNumDateDelim(char[] chars, int offset, int min, int max)
        {
            return CheckUntil(chars, offset, min, max, IsDateDelim);
        }
        private static int IsNumTimeDelim(char[] chars, int offset, int min, int max)
        {
            return CheckUntil(chars, offset, min, max, IsTimeDelim);
        }
        private static int IsSpacing(char[] chars, int offset, int min, int max)
        {
            return CheckUntil(chars, offset, min, max, IsSpace);
        }
        private static int IsNewLines(char[] chars, int offset, int min, int max)
        {
            return CheckUntil(chars, offset, min, max, IsNewLine);
        }
        private static int IsNullBytes(char[] chars, int offset, int min, int max)
        {
            return CheckUntil(chars, offset, min, max, IsNullByte);
        }
        private static int IsNumDot(char[] chars, int offset, int min, int max)
        {
            return CheckUntil(chars, offset, min, max, IsDot);
        }
        private static int IsDate(char[] chars, int offset)
        {
            int startOffset = offset;
            int tempOffset = IsNumDigits(chars, offset, 1, 2);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsNumDateDelim(chars, offset, 1, 1);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsNumDigits(chars, offset, 1, 2);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsNumDateDelim(chars, offset, 1, 1);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsNumDigits(chars, offset, 4, 4);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            return offset - startOffset;
        }
        private static int IsTime(char[] chars, int offset)
        {
            int startOffset = offset;
            int tempOffset = IsNumDigits(chars, offset, 2, 2);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsNumTimeDelim(chars, offset, 1, 1);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsNumDigits(chars, offset, 2, 2);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsNumTimeDelim(chars, offset, 1, 1);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsNumDigits(chars, offset, 2, 2);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsNumDot(chars, offset, 1, 1);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsNumDigits(chars, offset, 4, 4);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            return offset - startOffset;
        }
        private static int IsTimestamp(char[] buffer)
        {
            int offset = 0;
            var tempOffset = IsDate(buffer, offset);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsSpacing(buffer, offset, 1, 1);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsTime(buffer, offset);
            if (tempOffset == -1)
            {
                return -1;
            }
            offset += tempOffset;
            tempOffset = IsSpacing(buffer, offset, 2, 2);
            if (tempOffset == -1)
            {
                return -1;
            }
            //offset += tempOffset;
            return offset;
        }
        public static bool EndsWithTwoSpaces(SlidingBuffer<char> buffer)
        {
            return buffer[^2] == ' ' && buffer[^1] == ' ';
        }
        public static DateTime? ExtractTimestamp(int bufferLength, char[] buffer, string dateTimeFormat)
        {
            int offset = 0;
            if (bufferLength != buffer.Length)
            {
                offset = IsNullBytes(buffer, offset, 0, 2);
                if (offset != -1)
                {
                    buffer = buffer[offset..];
                }
                offset = 0;
            }
            offset = IsNewLines(buffer, offset, 0, 2);
            if (offset != -1)
            {
                buffer = buffer[offset..];
            }
            offset = IsTimestamp(buffer);
            if (offset == -1)
            {
                return null;
            }
            buffer = buffer[0..offset];
            var str = new string(buffer);
            try
            {
                if (!DateTime.TryParseExact(str, dateTimeFormat, null, System.Globalization.DateTimeStyles.AssumeLocal, out var ts))
                {
                    return null;
                }
                return ts;
            }
            catch
            {
                return null;
            }
        }
        public delegate bool GroupFunc(LogReaderArgs a, LogReaderArgs b);
        public static LogReaderGroup[] GroupLogReader(LogReader logReader, GroupFunc predicate)
        {
            if (logReader.Lines.Count == 0)
            {
                return [];
            }
            List<LogReaderGroup> results = [];
            LogReaderGroup? previous = null;
            foreach (var arg in logReader.Lines)
            {
                if (previous == null)
                {
                    previous = LogReaderGroup.CreateFrom(arg);
                    results.Add(previous);
                    continue;
                }
                var group = predicate(previous.End, arg);
                if (group != false)
                {
                    previous.End = arg;
                    continue;
                }
                previous = LogReaderGroup.CreateFrom(arg);
                results.Add(previous);
            }
            for (var i = 0; i < results.Count - 1; i++)
            {
                var current = results[i];
                var next = results[i + 1];
                current.EndPosition = next.StartPosition - 1;
            }
            var last = results[^1];
            last.EndPosition = logReader.FileLength;
            return [.. results];
        }
    }
    internal class PathUtils
    {
        public static string? GetFileName(string filePath)
        {
            string? fileName = null;
            try
            {
                fileName = Path.GetFileNameWithoutExtension(filePath);
            }
            catch
            {
            }
            if (fileName == null || fileName.Length == 0)
            {
                return null;
            }
            return fileName;
        }
        public static string? GetDirectoryPath(string filePath)
        {
            string? dirPath = null;
            try
            {
                dirPath = Path.GetDirectoryName(filePath);
            }
            catch
            {
            }
            if (dirPath == null || dirPath.Length == 0)
            {
                return null;
            }
            return dirPath;
        }
        public static string? Combine(string path1, string path2)
        {
            string? path = null;
            try
            {
                path = Path.Combine(path1, path2);
            }
            catch
            {
            }
            if (path == null || path.Length == 0)
            {
                return null;
            }
            return path;
        }
        public static string GetSplitFileName(string fileName, DateTime dateTime)
        {
            string timestamp = FormatUtils.GetDateTime(dateTime, Constants.DateTimeSplitFormat);
            return $"{fileName}_{timestamp}.txt";
        }
        public static string GetFullPath(string path)
        {
            try
            {
                return Path.GetFullPath(path);
            }
            catch
            {
                return string.Empty;
            }
        }
        public static string? ChangeExtension(string? path, string? extension)
        {
            if (path == null || path.Length == 0 || extension == null)
            {
                return null;
            }
            string? newPath = null;
            try
            {
                newPath = Path.ChangeExtension(path, extension);
            }
            catch
            {
            }
            if (newPath == null || newPath.Length == 0)
            {
                return null;
            }
            return newPath;
        }
        public static bool FileExists(string? filePath)
        {
            return filePath != null && filePath.Length > 0 && File.Exists(filePath);
        }
        public static bool DirectoryExists(string? dirPath)
        {
            return dirPath != null && dirPath.Length > 0 && Directory.Exists(dirPath);
        }
    }
    internal class ProgramUtils
    {
        public static void Exit(int exitCode)
        {
            Environment.Exit(exitCode);
        }
        public static void StdOut(string text, params object[] args)
        {
            Console.WriteLine(text, args);
        }
        public static void StdErr(string text, params object[] args)
        {
            Console.Error.WriteLine(text, args);
        }
        public static void Exception(Exception ex, int? exitCode = 1, bool? asError = true)
        {
            var text = ex.ToString();
            if (asError != false)
            {
                StdOut(text);
            }
            else
            {
                StdErr(text);
            }
            if (exitCode is { } _exitCode)
            {
                Exit(_exitCode);
            }
        }
        private static ProcessModule? GetProcessModule()
        {
            var process = Process.GetCurrentProcess();
            if (process == null)
            {
                return null;
            }
            ProcessModule? module = null;
            try
            {
                module = process.MainModule;
            }
            catch
            {
            }
            return module;
        }
        private static string? GetExecutablePath()
        {
            var module = GetProcessModule();
            return module?.FileName;
        }
        private static FileVersionInfo? GetExecutableInfo()
        {
            var module = GetProcessModule();
            return module?.FileVersionInfo;
        }
        public static string GetExecutableNameAndVersion(string fallback)
        {
            var info = GetExecutableInfo();
            if (info == null)
            {
                return fallback;
            }
            var name = info.ProductName;
            var version = info.ProductVersion;
            if (name == null || version == null)
            {
                return fallback;
            }
            var hash = version.IndexOf('+');
            if (hash > -1)
            {
                version = version[..hash];
            }
            return $"{name} ({version})";
        }
        public static bool WriteLogException(string message)
        {
            var path = GetExecutablePath();
            if (path == null)
            {
                return false;
            }
            string? logPath = PathUtils.ChangeExtension(path, ".log");
            if (logPath == null)
            {
                return false;
            }
            try
            {
                File.AppendAllText(logPath, message + Environment.NewLine);
                return true;
            }
            catch
            {
            }
            return false;
        }
        public static void ReadKey()
        {
            try
            {
                Console.ReadKey(true);
            }
            catch
            {
            }
        }
    }
}
