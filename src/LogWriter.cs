namespace WoWCombatLogSplit.src
{
    public class LogWriter(string filePath, string outputDirPath)
    {
        public readonly string FilePath = filePath;
        public readonly string FileName = PathUtils.GetFileName(filePath) ?? string.Empty;
        public readonly string OutputDirPath = outputDirPath;
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="FormatException" />
        /// <exception cref="IOException" />
        /// <exception cref="NotSupportedException" />
        /// <exception cref="ObjectDisposedException" />
        /// <exception cref="UnauthorizedAccessException" />
        public void Split(LogReaderGroup[] groups, Action<LogReaderGroup, string> callback)
        {
            var bufferSize = Constants.FileIOBufferSize;
            using FileStream fs = new(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: bufferSize, useAsync: false);
            byte[] buffer = new byte[bufferSize];
            foreach (var group in groups)
            {
                var remaining = group.EndPosition - group.StartPosition;
                if (remaining <= 0)
                {
                    continue;
                }
                var outputFileName = PathUtils.GetSplitFileName(FileName, group.Start.Timestamp);
                var outputFilePath = PathUtils.Combine(OutputDirPath, outputFileName);
                if (outputFilePath == null)
                {
                    continue;
                }
                if (PathUtils.FileExists(outputFilePath))
                {
                    ProgramUtils.StdOut("Skipping {0} it already exists.", outputFileName);
                    continue;
                }
                using FileStream ofs = new(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: bufferSize, useAsync: false);
                fs.Seek(group.StartPosition, SeekOrigin.Begin);
                while (remaining > 0)
                {
                    var toRead = remaining > bufferSize ? bufferSize : (int)remaining;
                    var read = fs.Read(buffer, 0, toRead);
                    if (read == 0) break;
                    ofs.Write(buffer, 0, read);
                    remaining -= read;
                }
                FileUtils.SetAttributes(ofs.SafeFileHandle, group.Start.Timestamp, group.End.Timestamp);
                ofs.Close();
                callback(group, outputFileName);
            }
            fs.Close();
        }
    }
}
