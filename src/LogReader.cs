namespace WoWCombatLogSplit.src
{
    internal class LogReaderArgs
    {
        public long Position { get; internal set; }
        public DateTime Timestamp { get; internal set; }
    }
    internal class LogReader(string filePath)
    {
        private readonly SlidingBuffer<char> buffer = new(Constants.MaxBufferLength);
        public readonly List<LogReaderArgs> Lines = [];
        public string FilePath { get { return filePath; } }
        public bool IsProcessed { get; private set; } = false;
        public long FileLength { get; internal set; }
        /// <exception cref="ArgumentException" />
        /// <exception cref="IOException" />
        /// <exception cref="NotSupportedException" />
        /// <exception cref="ObjectDisposedException" />
        public void Process()
        {
            if (IsProcessed)
            {
                return;
            }
            IsProcessed = true;
            DateTime? ts;
            FileLength = FileReader.Read(filePath, (args) =>
            {
                buffer.Push(args.Char);
                var length = buffer.Length;
                if (length < Constants.MinBufferLength)
                {
                    return;
                }
                if (!LogUtils.EndsWithTwoSpaces(buffer))
                {
                    return;
                }
                var chars = buffer.ToCharArray();
                ts = LogUtils.ExtractTimestamp(length, chars, Constants.DateTimeFormat);
                if (ts is { } _ts)
                {
                    LogReaderArgs logArgs = new()
                    {
                        Position = args.Position - chars.Length + 1,
                        Timestamp = _ts,
                    };
                    Lines.Add(logArgs);
                }
            });
        }
    }
    internal class LogReaderGroup
    {
        public required LogReaderArgs Start { get; internal set; }
        public required LogReaderArgs End { get; internal set; }
        public required long StartPosition { get; internal set; }
        public required long EndPosition { get; internal set; }
        public static LogReaderGroup CreateFrom(LogReaderArgs args)
        {
            return new()
            {
                Start = args,
                End = args,
                StartPosition = args.Position,
                EndPosition = args.Position,
            };
        }
    }
}
