namespace WoWCombatLogSplit.src
{
    public class Constants
    {
        public readonly static string[] SettingsFileKeys = ["file", "f"];
        public readonly static string[] SettingsDirKeys = ["dir", "d"];
        public readonly static string[] SettingsGapKeys = ["gap", "g"];
        public readonly static int FileIOBufferSize = 1_048_576;
        public readonly static int MinBufferLength = "1/1/1000 00:00:00.0000  ".Length;
        public readonly static int MaxBufferLength = "12/31/9999 23:59:59.9999  ".Length;
        public readonly static string DateTimeFormat = "M/d/yyyy HH:mm:ss.ffff";
        public readonly static string DateTimeSplitFormat = "yyyyMMdd_HHmmss";
        public readonly static string DateTimeStringFormat = "yyyy-MM-dd HH:mm:ss";
        public readonly static string EnvFile = ".env";
    }
}
