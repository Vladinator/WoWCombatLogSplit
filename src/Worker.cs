namespace WoWCombatLogSplit.src
{
    public class LogReaderProcessException(string message, Exception inner) : Exception(message, inner) { }
    public class LogWriterSplitException(string message, Exception inner) : Exception(message, inner) { }
    public class Worker(Settings settings)
    {
        private void OnGroup(LogReaderGroup group, string filePath)
        {
            var ts = group.Start.Timestamp;
            var duration = FormatUtils.GetDuration(group.End.Timestamp - ts);
            var size = FormatUtils.GetFileSize(group.EndPosition - group.StartPosition);
            ProgramUtils.StdOut("{0} | {1} | {2}", filePath, duration, size);
        }
        /// <exception cref="LogReaderProcessException" />
        /// <exception cref="LogWriterSplitException" />
        public void Process()
        {
            var filePath = settings.FilePathFull;
            var dirPath = settings.DirPathFull;
            var gap = settings.Gap;
            var logReader = new LogReader(filePath, gap);
            try
            {
                logReader.Process();
            }
            catch (Exception ex)
            {
                throw new LogReaderProcessException("The log reader couldn't process the file.", ex);
            }
            var groups = LogUtils.GroupLogReader(logReader, logReader.IsClose);
            if (groups.Length <= 1)
            {
                ProgramUtils.StdOut("There is nothing to split.");
                return;
            }
            LogWriter logWriter = new(filePath, dirPath);
            try
            {
                logWriter.Split(groups, OnGroup);
            }
            catch (Exception ex)
            {
                throw new LogWriterSplitException("The log writer couldn't save the splits to their own files.", ex);
            }
        }
    }
}
