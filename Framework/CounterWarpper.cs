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
        private int counter;

        public int Downloading { get { return counter; } }

        public void Increase()
        {
            Interlocked.Increment(ref counter);
            NotifyOfPropertyChange("Downloading");
        }

        public void Decrease()
        {
            Interlocked.Decrement(ref counter);
            NotifyOfPropertyChange("Downloading");
        }
    }
}
