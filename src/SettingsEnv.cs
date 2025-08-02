namespace WoWCombatLogSplit.src
{
    public class SettingsEnv(string? envPath) : ISettings
    {
        private void LoadEnvFile()
        {
            if (envPath == null || !PathUtils.FileExists(envPath))
            {
                return;
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
                return;
            }
            foreach (var line in lines)
            {
                var offset = line.IndexOf('=');
                if (offset < 0)
                {
                    continue;
                }
                var k = line[..offset].Trim().ToLowerInvariant();
                var v = line[(offset + 1)..].Trim();
                if (Constants.SettingsFileKeys.Contains(k))
                {
                    TrySetFile(v);
                }
                else if (Constants.SettingsDirKeys.Contains(k))
                {
                    TrySetDir(v);
                }
                else if (Constants.SettingsGapKeys.Contains(k))
                {
                    TrySetGap(v);
                }
            }
        }
        public override void Process()
        {
            LoadEnvFile();
        }
    }
}
