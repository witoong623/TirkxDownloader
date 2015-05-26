using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TirkxDownloader.Models
{
    /// <summary>
    /// Use to get DOM string from website that use javascript generate page
    /// </summary>
    public class WebProcessor : ApplicationContext
    {
        private string Url;
        private HtmlDocument generatedSource;
        private Thread thread;
        
        public async Task<HtmlDocument> GetGeneratedHTML(string url)
        {
            Url = url;
            await Task.Run(new Action(GetGeneratedHTMLHelper));

            return generatedSource;
        }

        private void GetGeneratedHTMLHelper()
        {
            thread = new Thread(new ThreadStart(RunMessageLoop));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private void RunMessageLoop()
        {
            webBrowserThread();
            Application.Run(this);
        }

        private void webBrowserThread()
        {
            var web = new WebBrowser();
            web.Navigate(Url);
            web.DocumentCompleted += web_DocumentCompleted;
        }

        private void web_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var web = (WebBrowser)sender;

            if (web.ReadyState == WebBrowserReadyState.Interactive)
            {
                generatedSource = web.Document;
                Application.Exit();
                web.Dispose();
                thread.Abort();
            }
            else
            {
                return;
            }
        }
    }
}
