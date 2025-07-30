namespace WoWCombatLogSplit
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var settings = Settings.CreateFromArgs(args);
            Console.WriteLine("Using these settings:");
            Console.WriteLine($"- File: \"{settings.FilePath}\"");
            Console.WriteLine($"- Dir: \"{settings.OutDir}\"");
            Console.WriteLine($"- Gap: {settings.Gap}");
            Console.WriteLine("");
            if (!settings.IsValid())
            {
                Console.Error.WriteLine("Invalid arguments.");
                Console.Error.WriteLine("--file -file -f        The file path to an existing file. (Required)");
                Console.Error.WriteLine("--dir -dir -d        The output directory for the splits. (Defaults to the input file directory)");
                Console.Error.WriteLine("--gap -gap -g        The hour gap between timestamps. (Defaults to 1.0)");
                Environment.Exit(1);
            }
            try
            {
                Run(settings.FilePath, settings.OutDir, settings.Gap);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(1);
                return;
            }
            Environment.Exit(0);
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
            var groups = Utils.GroupLogReader(logReader, (previous, current) =>
            {
                var delta = current.Timestamp - previous.Timestamp;
                return delta.TotalHours < gap;
            });
            if (groups.Length <= 1)
            {
                Console.WriteLine("There is nothing to split.");
                return;
            }
            for (var i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                var ts = group.Start.Timestamp;
                var duration = Utils.GetDuration(group.End.Timestamp - ts);
                var size = Utils.GetFileSize(group.EndPosition - group.StartPosition);
                Console.WriteLine("{0:D4} | {1} | {2} | {3}", i + 1, Utils.FormatDateTime(ts, Constants.DateTimeStringFormat), duration, size);
            }
            LogWriter logWriter = new(logReader.FilePath, outDir);
            logWriter.Split(groups);
        }
    }
}
