using System;
using System.Collections.Generic;

namespace TirkxDownloader.Framework
{
    public sealed class Pair<TFirst, TSecond> : IEquatable<Pair<TFirst, TSecond>>
    {
        private readonly TFirst _first;
        private readonly TSecond _second;

        public Pair(TFirst first, TSecond second)
        {
            this._first = first;
            this._second = second;
        }

        public TFirst First
        {
            get { return _first; }
        }

        public TSecond Second
        {
            get { return _second; }
        }

        public bool Equals(Pair<TFirst, TSecond> other)
        {
            if (other == null)
            {
                return false;
            }

            return EqualityComparer<TFirst>.Default.Equals(_first, other._first) &&
                EqualityComparer<TSecond>.Default.Equals(_second, other._second);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TFirst>.Default.GetHashCode(_first) * 37 +
               EqualityComparer<TSecond>.Default.GetHashCode(_second);
        }
    }
}
