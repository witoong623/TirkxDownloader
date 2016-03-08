using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using MetroRadiance.UI.Controls;

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

        /// <summary>
        /// Create DialogViewModel of Notification or Confirmation
        /// </summary>
        /// <param name="type">Dialog type</param>
        /// <param name="title">Dialog Title</param>
        /// <param name="text">Dialog message</param>
        /// <returns>DialogViewModel instance</returns>
        public static DialogViewModel CreateDialog(DialogType type, string title = "Title", string text = "Text")
        {
            return new DialogViewModel { DialogType = type, DisplayName = title, Text = text };
        }
        #endregion
    }

    public class DialogViewModel<T> : DialogViewModel
    {
        protected DialogViewModel() { }

        private T _input;
        public T InputResult
        {
            get { return _input; }
            set
            {
                _input = value;
                NotifyOfPropertyChange(nameof(InputResult));
            }
        }

        /// <summary>
        /// Create DialogViewModel that use to receive input from user, user should provide their own validation rules
        /// </summary>
        /// <typeparam name="T">Type of input value, it must be number type or string otherwise exception will be thrown</typeparam>
        /// <param name="title">Dialog Title</param>
        /// <param name="text">Dialog message</param>
        /// <param name="defaultVal">Default value of input</param>
        /// <returns>DialogViewModel instance</returns>
        public static DialogViewModel<T> CreateDialog<T>(string title = "Title", string text = "Text",
            T defaultVal = default(T))
        {
            if (!(typeof(T) == typeof(int) || typeof(T) == typeof(long) || typeof(T) == typeof(float) ||
                typeof(T) == typeof(double) || typeof(T) == typeof(string)))
            {
                throw new ArgumentException("T must be int,long,float,double,string", nameof(T));
            }

            return new DialogViewModel<T> { DialogType = DialogType.Input, DisplayName = title,
                Text = text, InputResult = defaultVal };
        }
    }
}
