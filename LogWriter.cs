namespace WoWCombatLogSplit
{
    /// <exception cref="ArgumentException" />
    /// <exception cref="PathTooLongException" />
    internal class LogWriter(string filePath, string outputDirPath)
    {
        public readonly string FilePath = filePath;
        public readonly string FileName = Utils.GetFileName(filePath);
        public readonly string OutputDirPath = outputDirPath;
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="FormatException" />
        /// <exception cref="IOException" />
        /// <exception cref="NotSupportedException" />
        /// <exception cref="ObjectDisposedException" />
        /// <exception cref="UnauthorizedAccessException" />
        public void Split(LogReaderGroup[] groups)
        {
            var bufferSize = Constants.FileIOBufferSize;
            using FileStream fs = new(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: bufferSize, useAsync: false);
            byte[] buffer = new byte[bufferSize];
            foreach (var group in groups)
            {
                long remaining = group.EndPosition - group.StartPosition;
                if (remaining <= 0)
                {
                    continue;
                }
                string outputFileName = Utils.GetSplitFileName(FileName, group.Start.Timestamp);
                string outputFilePath = Path.Combine(OutputDirPath, outputFileName);
                if (File.Exists(outputFilePath))
                {
                    Console.WriteLine("Skipping {0} it already exists.", outputFileName);
                    continue;
                }
                using FileStream ofs = new(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: bufferSize, useAsync: false);
                fs.Seek(group.StartPosition, SeekOrigin.Begin);
                while (remaining > 0)
                {
                    int toRead = remaining > bufferSize ? bufferSize : (int)remaining;
                    int read = fs.Read(buffer, 0, toRead);
                    if (read == 0) break;
                    ofs.Write(buffer, 0, read);
                    remaining -= read;
                }
                File.SetCreationTime(ofs.SafeFileHandle, group.Start.Timestamp);
                File.SetLastWriteTime(ofs.SafeFileHandle, group.End.Timestamp);
                File.SetLastAccessTime(ofs.SafeFileHandle, group.End.Timestamp);
                ofs.Close();
            }
            fs.Close();
        }
    }
}
