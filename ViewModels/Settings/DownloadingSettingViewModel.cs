using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using TirkxDownloader.Framework.Interface;

namespace TirkxDownloader.ViewModels.Settings
{
    public class DownloadingSettingViewModel : Screen, ISetting
    {
        public bool IsSet { get; set; }
    }
}
