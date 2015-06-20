using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TirkxDownloader.Framework
{
    public class CounterWarpper
    {
        private int counter;

        public int Counter { get { return counter; } }

        public void Increase()
        {
            Interlocked.Increment(ref counter);
        }

        public void Decrease()
        {
            Interlocked.Decrement(ref counter);
        }
    }
}
