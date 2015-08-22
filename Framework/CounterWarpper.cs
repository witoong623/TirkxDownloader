using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Caliburn.Micro;

namespace TirkxDownloader.Framework
{
    public class CounterWarpper : PropertyChangedBase
    {
        private int _counter;

        public int Downloading { get { return _counter; } }

        public void Increase()
        {
            Interlocked.Increment(ref _counter);
            NotifyOfPropertyChange("Downloading");
        }

        public void Decrease()
        {
            Interlocked.Decrement(ref _counter);
            NotifyOfPropertyChange("Downloading");
        }
    }
}
