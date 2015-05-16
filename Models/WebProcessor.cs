using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace TirkxDownloader.Models
{
    public class WebProcessor : ApplicationContext
    {
        private int NavigatingCouter;
        private int DocCompletedCounter;
        private string Url;

        private HtmlDocument generatedSource;
        private Thread thread;
        private WebBrowser web;
        

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
            
            web = new WebBrowser();
            web.Navigate(Url);
            web.DocumentCompleted += web_DocumentCompleted;
        }

        private void web_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var web = (WebBrowser)sender;

            if (web.ReadyState == WebBrowserReadyState.Interactive)
            {
                generatedSource = web.Document;
                web.Dispose();
                Application.Exit();
                thread.Abort();
            }
            else
            {
                return;
            }
        }
    }
}
