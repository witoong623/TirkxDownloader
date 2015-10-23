using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TirkxDownloader.Framework.Interface
{
    /// <summary>
    /// This interface should be implemented for recieve JSON then parse it and return to caller
    /// </summary>
    /// <typeparam name="T">Type of message, must be JSON compatibility</typeparam>
    interface IMessageReciever<T>
    {
        ICollection<string> Prefixes { get; }

        bool IsRecieving { get; }

        Task<T> GetMessageAsync();

        /// <summary>
        /// Call this method when terminate application
        /// </summary>
        void Close();

        /// <summary>
        /// Call this method when you want to stop reciever
        /// </summary>
        void StopReciever();
    }
}
