using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarkdownViewer
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            InitializeComponent();
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            string applicationPath = Context.Parameters["assemblypath"];
            string workingDir = Path.GetDirectoryName(applicationPath);

            string netFrameworkRedistPath = Path.Combine(Path.Combine(workingDir, "redist"), "ndp48-web.exe");
            string webview2RedistPath = Path.Combine(Path.Combine(workingDir, "redist"), "MicrosoftEdgeWebview2Setup.exe");

            Process.Start(netFrameworkRedistPath);
            Process.Start(webview2RedistPath);

            base.OnAfterInstall(savedState);
        }
    }
}
