using System;
using System.Collections.Generic;

namespace TirkxDownloader.Framework
{
    public sealed class Pair<TFirst, TSecond> : IEquatable<Pair<TFirst, TSecond>>
    {
        private readonly TFirst first;
        private readonly TSecond second;

        public Pair(TFirst first, TSecond second)
        {
            this.first = first;
            this.second = second;
        }

        public TFirst First
        {
            get { return first; }
        }

        public TSecond Second
        {
            get { return second; }
        }

        public override bool Equals(Pair<TFirst, TSecond> other)
        {
            if (other == null)
            {
                return false;
            }

            return EqualityComparer<TFirst>.Default.Equals(first, other.first) &&
                EqualityComparer<TSecond>.Default.Equals(second, other.second);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TFirst>.Default.GetHashCode(first) * 37 +
               EqualityComparer<TSecond>.Default.GetHashCode(second);
        }
    }
}
