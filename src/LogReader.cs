namespace WoWCombatLogSplit.src
{
    public class LogReaderArgs
    {
        public long Position { get; internal set; }
        public DateTime StartTimestamp { get; internal set; }
        public DateTime EndTimestamp { get; internal set; }
    }
    public class LogReader(string filePath, double gap)
    {
        private readonly SlidingBuffer<char> Buffer = new(Constants.MaxBufferLength);
        public readonly List<LogReaderArgs> Lines = [];
        private LogReaderArgs? PrevLine;
        public bool IsProcessed { get; private set; } = false;
        public long FileLength { get; internal set; }
        public bool IsClose(LogReaderArgs previous, DateTime ts)
        {
            var delta = ts - previous.EndTimestamp;
            return delta.TotalHours < gap;
        }
        public bool IsClose(LogReaderArgs previous, LogReaderArgs current)
        {
            return IsClose(previous, current.EndTimestamp);
        }
        private void AddLine(long position, DateTime timestamp)
        {
            if (PrevLine != null)
            {
                var isCloseResult = IsClose(PrevLine, timestamp);
                if (isCloseResult == true)
                {
                    PrevLine.EndTimestamp = timestamp;
                    return;
                }
            }
            LogReaderArgs logArgs = new()
            {
                Position = position,
                StartTimestamp = timestamp,
                EndTimestamp = timestamp,
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
    public class LogReaderGroup(LogReaderArgs start, long startPosition)
    {
        public readonly LogReaderArgs StartLine = start;
        public readonly long StartPosition = startPosition;
        public required LogReaderArgs EndLine { get; set; }
        public required long EndPosition { get; set; }
        public static LogReaderGroup CreateFrom(LogReaderArgs args)
        {
            return new(args, args.Position)
            {
                EndLine = args,
                EndPosition = args.Position,
            };
        }
    }
}
