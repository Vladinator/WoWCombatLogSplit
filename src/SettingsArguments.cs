namespace WoWCombatLogSplit.src
{
    public class SettingsArguments(string[] args) : ISettings
    {
        private void LoadArgs()
        {
            if (args.Length == 0)
            {
                return;
            }
            for (var i = 0; i < args.Length - 1; i++)
            {
                var k = args[i].Trim().ToLowerInvariant();
                if (k.Length == 0)
                {
                    continue;
                }
                if (k.StartsWith("--"))
                {
                    k = k[2..];
                }
                else if (k.StartsWith('-'))
                {
                    k = k[1..];
                }
                if (k.Length == 0)
                {
                    continue;
                }
                var v = args[i + 1];
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
            if (FilePath.Length > 0)
            {
                return;
            }
            foreach (var arg in args)
            {
                if (!PathUtils.FileExists(arg))
                {
                    continue;
                }
                if (TrySetFile(arg))
                {
                    break;
                }
            }
        }
        public override void Process()
        {
            LoadArgs();
        }
    }
}
