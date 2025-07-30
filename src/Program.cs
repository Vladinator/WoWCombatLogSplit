namespace WoWCombatLogSplit.src
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var settings = Settings.CreateFromArgs(args);
            ProgramUtils.StdOut("Using these settings:");
            ProgramUtils.StdOut($"- File: \"{settings.FilePath}\"");
            ProgramUtils.StdOut($"- Dir: \"{settings.OutDir}\"");
            ProgramUtils.StdOut($"- Gap: {settings.Gap}");
            if (!settings.IsValid())
            {
                ProgramUtils.StdOut("");
                ProgramUtils.StdErr("Invalid arguments.");
                ProgramUtils.StdErr("--file -file -f        The file path to an existing file. (Required)");
                ProgramUtils.StdErr("--dir -dir -d        The output directory for the splits. (Defaults to the input file directory)");
                ProgramUtils.StdErr("--gap -gap -g        The hour gap between timestamps. (Defaults to 1.0)");
                ProgramUtils.Exit(1);
            }
            try
            {
                Run(settings.FilePath, settings.OutDir, settings.Gap);
            }
            catch (Exception ex)
            {
                ProgramUtils.Exception(ex);
                return;
            }
            ProgramUtils.Exit(0);
        }
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="FormatException" />
        /// <exception cref="IOException" />
        /// <exception cref="NotSupportedException" />
        /// <exception cref="ObjectDisposedException" />
        /// <exception cref="UnauthorizedAccessException" />
        private static void Run(string filePath, string outDir, double gap)
        {
            var logReader = new LogReader(filePath);
            logReader.Process();
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
            LogWriter logWriter = new(logReader.FilePath, outDir);
            logWriter.Split(groups);
        }
    }
}
