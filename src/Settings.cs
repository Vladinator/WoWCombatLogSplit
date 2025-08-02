namespace WoWCombatLogSplit.src
{
    public abstract class ISettings
    {
        public string FilePath { get; internal set; } = string.Empty;
        public string DirPath { get; internal set; } = string.Empty;
        public double Gap { get; internal set; } = 1.0;
        public string FilePathFull { get { return PathUtils.GetFullPath(FilePath); } }
        public string DirPathFull { get { return PathUtils.GetFullPath(DirPath); } }
        public bool TrySetFile(string? value)
        {
            if (value == null || value.Length == 0)
            {
                return false;
            }
            FilePath = value;
            if (PathUtils.FileExists(value) && !PathUtils.DirectoryExists(DirPath))
            {
                TrySetDir(PathUtils.GetDirectoryPath(value));
            }
            return true;
        }
        public bool TrySetDir(string? value)
        {
            if (value == null || value.Length == 0)
            {
                return false;
            }
            DirPath = value;
            return true;
        }
        public bool TrySetGap(object? value)
        {
            if (value == null)
            {
                return false;
            }
            if (value is double dValue)
            {
                Gap = dValue;
                return true;
            }
            if (value is string sValue)
            {
                if (sValue.Length == 0)
                {
                    return false;
                }
                if (double.TryParse(sValue, out var temp))
                {
                    Gap = temp;
                    return true;
                }
            }
            return false;
        }
        public bool IsValid()
        {
            return Gap > 0 && PathUtils.FileExists(FilePath) && PathUtils.DirectoryExists(DirPath);
        }
        public abstract void Process();
    }
    public class Settings : ISettings
    {
        private readonly SettingsEnv EnvSettings;
        private readonly SettingsArguments ArgSettings;
        public Settings(string[] args)
        {
            EnvSettings = new();
            ArgSettings = new(args);
            Process();
        }
        private void LoadFrom(ISettings settings)
        {
            TrySetFile(settings.FilePath);
            TrySetDir(settings.DirPath);
            TrySetGap(settings.Gap);
        }
        public override void Process()
        {
            EnvSettings.Process();
            ArgSettings.Process();
            LoadFrom(EnvSettings);
            LoadFrom(ArgSettings);
        }
    }
}
