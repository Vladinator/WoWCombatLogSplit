namespace WoWCombatLogSplit.src
{
    internal class Settings
    {
        private static readonly string[] FileArgKeys = ["--file", "-file", "-f"];
        private static readonly string[] OutDirArgKeys = ["--dir", "-dir", "-d"];
        private static readonly string[] GapArgKeys = ["--gap", "-gap", "-g"];
        private static readonly string[] FileEnvKeys = ["file"];
        private static readonly string[] OutDirEnvKeys = ["dir"];
        private static readonly string[] GapEnvKeys = ["gap"];
        private string _FilePath = string.Empty;
        private string _OutDir = string.Empty;
        private double _Gap = 1.0;
        public string FilePath
        {
            get => _FilePath;
            internal set
            {
                if (value.Length > 0 && File.Exists(value))
                {
                    _FilePath = value;
                }
            }
        }
        public string OutDir
        {
            get => _OutDir;
            internal set
            {
                if (value.Length > 0 && Directory.Exists(value))
                {
                    _OutDir = value;
                }
            }
        }
        public double Gap
        {
            get => _Gap;
            internal set
            {
                if (value > 0)
                {
                    _Gap = value;
                }
            }
        }
        public bool IsValid()
        {
            if (FilePath.Length <= 0 || Gap <= 0)
            {
                return false;
            }
            return File.Exists(FilePath);
        }
        private static string? GetArg(string[] args, string[] keys)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i].ToLowerInvariant();
                if (!keys.Contains(arg))
                {
                    continue;
                }
                var index = i + 1;
                if (index > args.Length - 1)
                {
                    continue;
                }
                var value = args[index];
                if (value != null && value.Length > 0)
                {
                    return value;
                }
            }
            return null;
        }
        private static void SetFilePath(Settings settings, string? value)
        {
            if (value == null)
            {
                return;
            }
            settings.FilePath = value;
        }
        private static void SetOutDir(Settings settings, string? value)
        {
            if (value == null)
            {
                return;
            }
            settings.OutDir = value;
        }
        private static void SetGap(Settings settings, string? value)
        {
            if (value == null || value.Length == 0)
            {
                return;
            }
            value = value.Replace(",", ".");
            if (value.Length == 0)
            {
                return;
            }
            if (double.TryParse(value, out var gap))
            {
                settings.Gap = gap;
            }
        }
        private static void SetAll(Settings settings, string? filePath, string? outDir, string? gap)
        {
            SetFilePath(settings, filePath);
            SetOutDir(settings, outDir);
            SetGap(settings, gap);
        }
        private static Settings CreateFromEnv()
        {
            var settings = new Settings();
            var envPath = PathUtils.Combine(AppContext.BaseDirectory, Constants.EnvFile);
            if (envPath == null || !File.Exists(envPath))
            {
                return settings;
            }
            IEnumerable<string>? lines = null;
            try
            {
                lines = File.ReadLines(envPath);
            }
            catch
            {
            }
            if (lines == null)
            {
                return settings;
            }
            var dict = new Dictionary<string, string>();
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line == null || line.Length == 0 || line.StartsWith('#'))
                {
                    continue;
                }
                var parts = line.Split("=", 2);
                if (parts.Length != 2)
                {
                    continue;
                }
                var key = parts[0].Trim().ToLowerInvariant();
                var value = parts[1].Trim();
                if (FileEnvKeys.Contains(key))
                {
                    key = FileEnvKeys[0];
                }
                else if (OutDirEnvKeys.Contains(key))
                {
                    key = OutDirEnvKeys[0];
                }
                else if (GapEnvKeys.Contains(key))
                {
                    key = GapEnvKeys[0];
                }
                else
                {
                    continue;
                }
                dict[key] = value;
            }
            var fileArg = dict[FileEnvKeys[0]];
            var outDirArg = dict[OutDirEnvKeys[0]];
            var gapArg = dict[GapEnvKeys[0]];
            SetAll(settings, fileArg, outDirArg, gapArg);
            return settings;
        }
        public static Settings CreateFromArgs(string[] args)
        {
            var settings = CreateFromEnv();
            var fileArg = GetArg(args, FileArgKeys);
            var outDirArg = GetArg(args, OutDirArgKeys);
            var gapArg = GetArg(args, GapArgKeys);
            SetAll(settings, fileArg, outDirArg, gapArg);
            if (settings.OutDir.Length == 0)
            {
                try
                {
                    settings.OutDir = PathUtils.GetDirectoryPath(settings.FilePath) ?? string.Empty;
                }
                catch
                {
                }
            }
            return settings;
        }
    }
}
