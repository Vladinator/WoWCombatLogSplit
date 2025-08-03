namespace WoWCombatLogSplit.src
{
    public class LogReaderProcessException(string message, Exception inner) : Exception(message, inner) { }
    public class LogWriterSplitException(string message, Exception inner) : Exception(message, inner) { }
    public class Worker(Settings settings)
    {
        /// <exception cref="LogReaderProcessException" />
        /// <exception cref="LogWriterSplitException" />
        public bool Process(Action<LogReaderGroup, string, bool>? callback = null)
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
                return false;
            }
            LogWriter logWriter = new(filePath, dirPath);
            try
            {
                logWriter.Split(groups, callback);
            }
            catch (Exception ex)
            {
                throw new LogWriterSplitException("The log writer couldn't save the splits to their own files.", ex);
            }
            return true;
        }
    }
}
