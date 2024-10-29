using System;
using System.IO;
using System.Windows.Forms;

namespace MarkdownViewer
{
    internal static class Program
    {
        public static string FilePath;

        [STAThread]
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                MessageBox.Show("Please run Markdown Viewer with a markdown (.md) file.", "Markdown Viewer");
                return;
            }
            else if (args != null && args.Length > 0)
            {
                string extension = GetFileExtension(args[0]);

                if(extension != null && (extension == ".md" || extension == ".txt"))
                {
                    FilePath = args[0];
                }
                else
                {
                    MessageBox.Show("Not a valid file. File must have .md or .txt extension.", "Markdown Viewer");
                    return;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());

            Directory.Delete(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "MarkdownViewer.exe.WebView2"), true);
        }

        static string GetFileExtension(string path)
        {
            int index = path.LastIndexOf('.');
            if (index < 0)
                return null;
            return path.Substring(index);
        }
    }
}
