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
