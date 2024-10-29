using MarkdownViewer.Properties;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows.Forms;

namespace MarkdownViewer
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            this.Icon = Resources.MarkdownViewerIcon;

            this.Resize += (object sender, EventArgs e) => webView2.Size = this.ClientSize - new System.Drawing.Size(webView2.Location);
            InitializeAsync();
        }

        async void InitializeAsync()
        {
            await webView2.EnsureCoreWebView2Async(null);

            OpenFile(Program.FilePath);
        }

        //TODO: Use a css file for windows theming.

        private void OpenFile(string filePath)
        {
            string markdownText = File.ReadAllText(filePath);
            string markdownHTML = Markdig.Markdown.ToHtml(markdownText);

            try
            {
                webView2.NavigateToString(markdownHTML);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Markdown Viewer");
            }
        }
    }
}
