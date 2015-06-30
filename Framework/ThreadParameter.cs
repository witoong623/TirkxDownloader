using System;
using Caliburn.Micro;

namespace TirkxDownloader.Framework
{
    public class ThreadParameter
    {
        public DownloadInfo DownloadInformation { get; set; }
        public IEventAggregator EventAggregate { get; set; }
        public CounterWarpper Counter { get; set; }
    }
}
