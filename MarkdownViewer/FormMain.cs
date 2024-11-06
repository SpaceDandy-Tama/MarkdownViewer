using System;
using System.IO;
using System.Windows.Forms;

using Markdig;
using Microsoft.Web.WebView2.Core;
using MarkdownViewer.Properties;
using System.Diagnostics;
using OpenBookMarkdown;

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
            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disable-web-security --allow-file-access-from-files --allow-file-access");
            CoreWebView2Environment coreWebView2Environment = await CoreWebView2Environment.CreateAsync(null, Path.GetTempPath(), options);
            await webView2.EnsureCoreWebView2Async(coreWebView2Environment);

            //webView2.DefaultBackgroundColor = System.Drawing.Color.DarkGray;
            //webView2.BackColor = System.Drawing.Color.DarkGray;
            //webView2.CoreWebView2.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Dark;

            OpenFile(Program.FilePath);
        }

        private void OpenFile(string inputFilePath)
        {
            try
            {
                string path = null;
                if (inputFilePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                {
                    path = RebuildHtmlForLocalFiles(inputFilePath);
                }
                else if (inputFilePath.EndsWith(".obmd", StringComparison.OrdinalIgnoreCase))
                {
                    string obmdHtml = OpenBookMarkdown.OpenBookMarkdown.ToHtml(inputFilePath);
                    path = Path.Combine(Path.GetTempPath(), "MarkdownViewerRebuilt.html");
                    File.WriteAllText(path, obmdHtml);
                }
                else
                {
                    //pdf, txt, etc
                    path = inputFilePath;
                }

                if (path == null)
                    return;

                webView2.CoreWebView2.Navigate($"File:///{path}");

                webView2.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Application.ProductName);
            }
        }

        private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            webView2.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            webView2.CoreWebView2.NavigationCompleted -= CoreWebView2_NavigationCompleted;
        }

        private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            e.Cancel = true;

            try
            {
                Process.Start(new ProcessStartInfo(e.Uri) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        private string RebuildHtmlForLocalFiles(string inputFilePath)
        {
            string markdownText = File.ReadAllText(inputFilePath);
            string markdownHTML = Markdown.ToHtml(markdownText);

#if DEBUG
            File.WriteAllText("debug.html", markdownHTML);
#endif

            string[] htmlLines = markdownHTML.Split('\n');

            for (int i = 0; i < htmlLines.Length; i++)
            {
                if (string.IsNullOrEmpty(htmlLines[i]))
                    continue;

                htmlLines[i] = CheckAndReplaceFilePathsWithProperProtocol(htmlLines[i]);
            }

            markdownHTML = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n    <head>\r\n        <meta charset=\"UTF-8\">\r\n        <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n        <title>{Path.GetFileName(inputFilePath)}</title>\r\n    </head>\r\n    <body>\r\n";
            foreach (string line in htmlLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                markdownHTML += $"        {line}\r\n";
            }
            markdownHTML += "    </body>\r\n</html>";

            string path = Path.Combine(Path.GetTempPath(), "MarkdownViewerRebuilt.html");
            File.WriteAllText(path, markdownHTML);

            return path;
        }

        private string CheckAndReplaceFilePathsWithProperProtocol(string line)
        {
            int startIndex = line.IndexOf("src=\"");
            int endIndex = -1;

            if (startIndex < 0)
            {
                startIndex = line.IndexOf("href=\"");
            }

            if (startIndex > -1)
            {
                //first double quote, we know this one exists already
                startIndex = line.IndexOf('\"', startIndex);

                //but we must find the index of the second double quote, so we can read value between the two
                if (startIndex + 1 < line.Length)
                {
                    startIndex += 1;
                    endIndex = line.IndexOf('\"', startIndex);

                    if (endIndex > -1)
                    {
                        string valueString = line.Substring(startIndex, endIndex - startIndex);

                        if (!valueString.Contains(":"))
                        {
                            string pathFileFound = null;

                            if (File.Exists(valueString))
                            {
                                pathFileFound = valueString;
                            }
                            else
                            {
                                string tempPath = Path.Combine("images", valueString);
                                if (File.Exists(tempPath))
                                {
                                    pathFileFound = tempPath;
                                }
                            }

                            if (pathFileFound != null) {
                                //If the file exists locally, specify File protocol in the line
                                string uri = Path.GetFullPath(pathFileFound);
                                uri = $"file:///{uri}";

                                //modify the line with this
                                string prefix = line.Substring(0, startIndex);
                                string suffix = line.Substring(endIndex, line.Length - endIndex);
                                string modifiedLine = prefix + uri + suffix;

                                return modifiedLine;
                            }
                        }
                    }
                }
            }

            //Return the original line if a protocol is explicitly specified or if the file doesn't exist
            return line;
        }
    }
}
