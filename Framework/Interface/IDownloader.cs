using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TirkxDownloader.Framework.Interface
{
    public interface IDownloader
    {
        bool IsDownloading { get; }
    }
}
