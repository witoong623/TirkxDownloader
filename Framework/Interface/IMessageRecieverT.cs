using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TirkxDownloader.Framework.Interface
{
    /// <summary>
    /// This interface should be implemented for recieve JSON then parse it and return to caller
    /// </summary>
    /// <typeparam name="T">Type of message, must be JSON compatibility</typeparam>
    public interface IMessageReciever<T>
    {
        ICollection<string> Prefixes { get; }

        bool IsRecieving { get; }

        Task<T> GetMessageAsync();

        Task<T> GetMessageAsync(CancellationToken ct);

        void StartSendEvent(CancellationToken ct);

        /// <summary>
        /// Call this method when terminate application
        /// </summary>
        void Close();
    }
}
