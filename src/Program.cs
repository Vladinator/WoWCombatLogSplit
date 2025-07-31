namespace WoWCombatLogSplit.src
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, ex) =>
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var message = $"[UnhandledException] {timestamp}: {ex.ExceptionObject}";
                ProgramUtils.StdErr(message);
                if (!ex.IsTerminating)
                {
                    return;
                }
                if (ProgramUtils.WriteLogException(message))
                {
                    return;
                }
                ProgramUtils.StdOut("Press any key to exit . . .");
                ProgramUtils.ReadKey();
            };
            var info = ProgramUtils.GetExecutableNameAndVersion("WoWCombatLogSplit (unknown version)");
            ProgramUtils.StdOut(info);
            ProgramUtils.StdOut("");
            var settings = new Settings(args);
            ProgramUtils.StdOut("Using these settings:");
            ProgramUtils.StdOut($"- File: \"{settings.FilePathFull}\"");
            ProgramUtils.StdOut($"- Dir: \"{settings.DirPathFull}\"");
            ProgramUtils.StdOut($"- Gap: {settings.Gap}");
            if (!settings.IsValid())
            {
                ProgramUtils.StdOut("");
                ProgramUtils.StdErr("Invalid arguments.");
                ProgramUtils.StdErr($"--{Constants.SettingsFileKeys[0]} -{Constants.SettingsFileKeys[0]} -{Constants.SettingsFileKeys[1]}        The file path to an existing file. (Required)");
                ProgramUtils.StdErr($"--{Constants.SettingsDirKeys[0]} -{Constants.SettingsDirKeys[0]} -{Constants.SettingsDirKeys[1]}        The output directory for the splits. (Defaults to the input file directory)");
                ProgramUtils.StdErr($"--{Constants.SettingsGapKeys[0]} -{Constants.SettingsGapKeys[0]} -{Constants.SettingsGapKeys[1]}        The hour gap between timestamps. (Defaults to 1.0)");
                ProgramUtils.Exit(1);
                return;
            }
            ProgramUtils.StdOut("");
            var worker = new Worker(settings);
            try
            {
                worker.Process();
            }
            catch (Exception ex)
            {
                ProgramUtils.Exception(ex);
                return;
            }
            ProgramUtils.Exit(0);
        }
    }
}
