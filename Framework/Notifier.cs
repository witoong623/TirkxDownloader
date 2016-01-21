using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Caliburn.Micro;

namespace TirkxDownloader.Framework
{
    public class Notifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyOfPropertyChange([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
