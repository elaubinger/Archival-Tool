using log4net;
using log4net.Config;
using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace ArchivalTool
{
    static class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            const string appName = "Archival Tool";
            bool createdNew;

            var mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew) return;
            
            XmlConfigurator.Configure();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ArchiveMetadata.Initialize();

            Application.Run(new Bootstrap());
        }
    }
}
