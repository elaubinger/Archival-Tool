using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ArchivalTool
{
    static class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DllImport("kernel32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.I1)]
        static extern bool CreateSymbolicLinkA(string lpSymlinkFileName, string lpTargetFileName, UInt32 dwFlags);


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string 
                symF = Path.GetFullPath(@"F:\WiFi Storage\test\link\test.txt"),
                targetF = Path.GetFullPath(@"F:\WiFi Storage\test\test.txt");

            CreateSymbolicLinkA(
                lpSymlinkFileName: symF,
                lpTargetFileName: targetF,
                dwFlags: 0
                );
            return;

            #region Check for Existing Running Instances
            const string appName = "Archival Tool";
            bool createdNew;

            var mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew) return;
            #endregion

            // Configure logging
            XmlConfigurator.Configure();
            
            // Configure application settings
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Configure archival metadata
            ArchiveMetadata.Initialize();

            // Begin running core form app
            Application.Run(new Bootstrap());
        }
    }
}
