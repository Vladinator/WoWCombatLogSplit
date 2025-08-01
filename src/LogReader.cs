namespace WoWCombatLogSplit.src
{
    internal class LogReaderArgs
    {
        public long Position { get; internal set; }
        public DateTime Timestamp { get; internal set; }
    }
    internal class LogReader(string filePath, double gap)
    {
        private readonly SlidingBuffer<char> Buffer = new(Constants.MaxBufferLength);
        public readonly List<LogReaderArgs> Lines = [];
        private LogReaderArgs? PrevLine;
        public bool IsProcessed { get; private set; } = false;
        public long FileLength { get; internal set; }
        public bool IsClose(LogReaderArgs previous, DateTime ts)
        {
            var delta = ts - previous.Timestamp;
            return delta.TotalHours < gap;
        }
        public bool IsClose(LogReaderArgs previous, LogReaderArgs current)
        {
            return IsClose(previous, current.Timestamp);
        }
        private void AddLine(long position, DateTime timestamp)
        {
            if (PrevLine != null)
            {
                var isCloseResult = IsClose(PrevLine, timestamp);
                if (isCloseResult == true)
                {
                    PrevLine.Timestamp = timestamp;
                    return;
                }
            }
            LogReaderArgs logArgs = new()
            {
                Position = position,
                Timestamp = timestamp,
            };
            Lines.Add(logArgs);
            PrevLine = logArgs;
        }
        private void OnByte(FileReaderArgs args)
        {
            Buffer.Push(args.Char);
            var length = Buffer.Length;
            if (length < Constants.MinBufferLength)
            {
                return;
            }
            if (!LogUtils.EndsWithTwoSpaces(Buffer))
            {
                return;
            }
            var chars = Buffer.ToCharArray();
            var ts = LogUtils.ExtractTimestamp(length, chars, Constants.DateTimeFormat, out var tsLength);
            if (ts is { } _ts)
            {
                AddLine(args.Position - tsLength + 1, _ts);
            }
        }
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
            FileLength = FileReader.Read(filePath, OnByte);
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
