using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MetroTrilithon.Serialization;

namespace TirkxDownloader.Models.Settings
{
    public class DownloadingSetting
    {
        public static SerializableProperty<long> MaximumBytesPerSec { get; } 
            = new SerializableProperty<long>(GetKey(), SettingsProviders.Local, 0);

        public static SerializableProperty<byte> MaxConcurrentDownload { get; }
            = new SerializableProperty<byte>(GetKey(), SettingsProviders.Local, 1);

        private static string GetKey([CallerMemberName] string caller = "")
        {
            return nameof(DownloadingSetting) + "." + caller;
        }
    }
}
