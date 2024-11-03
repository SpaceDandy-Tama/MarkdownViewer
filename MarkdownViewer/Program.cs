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
                MessageBox.Show("Please run Markdown Viewer with a suitable file. Suitable files include .md, .obmd, .pdf and .txt", Application.ProductName);
                return;
            }
            else if (args != null && args.Length > 0)
            {
                string arg = args[0];
                StringComparison comparison = StringComparison.OrdinalIgnoreCase;

                if(arg.EndsWith(".md", comparison) || 
                    arg.EndsWith(".obmd", comparison) || 
                    arg.EndsWith(".pdf", comparison) || 
                    arg.EndsWith(".txt", comparison))
                {
                    FilePath = args[0];
                }
                else
                {
                    MessageBox.Show("Not a valid file. File must have .md, .obmd, .pdf or .txt extension.", Application.ProductName);
                    return;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
