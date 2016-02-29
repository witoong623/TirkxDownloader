using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using MahApps.Metro.Controls;

namespace TirkxDownloader.ViewModels
{
    public enum DialogType { Confirmation, Notificatioin, Input }
    public enum DialogResult { Yes, No, Cancel }
    public class DialogViewModel : Screen
    {
        protected DialogViewModel() { }
        #region Properties
        public DialogType DialogType { get; protected set; }
        /// <summary>
        /// This property shold be used with Comfirmation type
        /// </summary>
        public DialogResult DialogResult { get; set; }

        public string Text { get; set; }
        #endregion

        #region Methods
        public void YES()
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        public void NO()
        {
            DialogResult = DialogResult.No;
            Close();
        }

        public void CANCEL()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public void Close()
        {
            var window = (MetroWindow)GetView();
            window.Close();
        }

        public void Show()
        {

        }

        public static DialogViewModel CreateDialog(DialogType type, string title = "Title", string text = "Text")
        {
            return new DialogViewModel { DialogType = type, DisplayName = title, Text = text };
        }
        #endregion
    }

    public class DialogViewModel<T> : DialogViewModel where T : struct
    {
        private DialogViewModel() { }
        public T InputResult { get; set; }

        public static DialogViewModel<T> CreateDialog<T>(string title = "Title", string text = "Text",
            T input = default(T)) where T : struct
        {
            return new DialogViewModel<T> { DialogType = DialogType.Input, DisplayName = title,
                Text = text, InputResult = input };
        }
    }
}
