using System;
using System.Diagnostics;
using System.IO;
using MetroTrilithon.Serialization;

namespace TirkxDownloader.Models.Settings
{
    public static class SettingsProviders
    {
        public static string LocalFilePath { get; } 
            = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings", "Settings.xaml");

        public static ISerializationProvider Local { get; } = new FileSettingsProvider(LocalFilePath);
    }
}
