namespace WoWCombatLogSplit.src
{
    internal class LogReaderProcessException(string message, Exception inner) : Exception(message, inner) { }
    internal class LogWriterSplitException(string message, Exception inner) : Exception(message, inner) { }
    internal class Worker(Settings settings)
    {
        /// <exception cref="LogReaderProcessException" />
        /// <exception cref="LogWriterSplitException" />
        public void Process()
        {
            var filePath = settings.FilePathFull;
            var dirPath = settings.DirPathFull;
            var gap = settings.Gap;
            var logReader = new LogReader(filePath);
            try
            {
                logReader.Process();
            }
            catch (Exception ex)
            {
                throw new LogReaderProcessException("The log reader couldn't process the file.", ex);
            }
            var groups = LogUtils.GroupLogReader(logReader, (previous, current) =>
            {
                var delta = current.Timestamp - previous.Timestamp;
                return delta.TotalHours < gap;
            });
            if (groups.Length <= 1)
            {
                ProgramUtils.StdOut("There is nothing to split.");
                return;
            }
            for (var i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                var ts = group.Start.Timestamp;
                var duration = FormatUtils.GetDuration(group.End.Timestamp - ts);
                var size = FormatUtils.GetFileSize(group.EndPosition - group.StartPosition);
                ProgramUtils.StdOut("{0:D4} | {1} | {2} | {3}", i + 1, FormatUtils.GetDateTime(ts, Constants.DateTimeStringFormat), duration, size);
            }
            LogWriter logWriter = new(filePath, dirPath);
            try
            {
                logWriter.Split(groups);
            }
            catch (Exception ex)
            {
                throw new LogWriterSplitException("The log writer couldn't save the splits to their own files.", ex);
            }
        }
    }
}
