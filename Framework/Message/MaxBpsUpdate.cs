namespace TirkxDownloader.Framework.Message
{
    public class MaxBpsUpdate
    {
        public MaxBpsUpdate(long maxBps)
        {
            MaximumBytesPerSecond = maxBps;
        }

        public long MaximumBytesPerSecond { get; private set; }
    }
}