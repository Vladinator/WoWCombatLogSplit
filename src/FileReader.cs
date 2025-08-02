namespace WoWCombatLogSplit.src
{
    public class FileReaderArgs
    {
        public long Position { get; internal set; }
        public char Char { get; internal set; }
        public bool Break { get; set; } = false;
        public bool Forward { get; set; } = true;
    }
    public class FileReader()
    {
        /// <exception cref="ArgumentException" />
        /// <exception cref="IOException" />
        /// <exception cref="NotSupportedException" />
        /// <exception cref="ObjectDisposedException" />
        public static long Read(string filePath, Action<FileReaderArgs> callback)
        {
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: Constants.FileIOBufferSize, useAsync: false);
            FileReaderArgs args = new();
            long position = 0;
            long length = fs.Length;
            bool forward = true;
            int b;
            while (true)
            {
                if (forward ? position >= length : position <= 0)
                {
                    break;
                }
                fs.Seek(position, SeekOrigin.Begin);
                b = fs.ReadByte();
                if (b == -1)
                {
                    break;
                }
                args.Position = position;
                args.Char = (char)b;
                args.Forward = forward;
                callback(args);
                if (args.Break)
                {
                    break;
                }
                forward = args.Forward;
                position += forward ? 1 : -1;
            }
            fs.Close();
            return length;
        }
    }
}
