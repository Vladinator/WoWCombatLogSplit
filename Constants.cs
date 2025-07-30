namespace WoWCombatLogSplit
{
    internal class Constants
    {
        public static int FileIOBufferSize = 1_048_576;
        public static int MinBufferLength = "1/1/1000 00:00:00.0000  ".Length;
        public static int MaxBufferLength = "12/31/9999 23:59:59.9999  ".Length;
        public static string DateTimeFormat = "M/d/yyyy HH:mm:ss.ffff";
        public static string DateTimeSplitFormat = "yyyyMMdd_HHmmss";
        public static string DateTimeStringFormat = "yyyy-MM-dd HH:mm:ss";
        public static string EnvFile = ".env";
    }
}
