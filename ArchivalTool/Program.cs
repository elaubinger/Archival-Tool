using ArchivalTool.Properties;
using log4net;
using log4net.Config;
using System;
using System.Drawing;
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
            using (new TrayIcon())
            {
                Application.Run();
            }
        }
    }

    public class TrayIcon : IDisposable
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private NotifyIcon trayIcon;

        public TrayIcon()
        {
            var archiveWorker = new ArchiveWorker(ArchiveMetadata.UnsortedDirectory);

            var resort = new ToolStripMenuItem(
                "Re-Sort Existing",
                image: Resources.loop_circular_2x);

            resort.Click += delegate
            {
                resort.Enabled = false;

                var worker = new ArchiveWorker(ArchiveMetadata.ArchiveDirectory, ArchiveWorker.ArchiveWorkerMode.Once);
                worker.RunWorkerCompleted += delegate { resort.Enabled = true; };
                worker.RunWorkerAsync();
            };

            // Initialize Tray Icon
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.AddRange(new ToolStripItem[] {
                    resort,
                    new ToolStripSeparator(),
                    new ToolStripMenuItem(
                        "Exit",
                        image: Resources.account_logout_2x,
                        onClick: delegate
                    {
                        archiveWorker.CancelRequested = true;
                        trayIcon.Visible = false;
                        Application.Exit();
                    })
            });

            trayIcon = new NotifyIcon
            {
                Icon = Icon.FromHandle(Resources.bolt_2x.GetHicon()),
                ContextMenuStrip = contextMenu,
                Visible = true
            };

            archiveWorker.RunWorkerAsync();
        }

        public void Dispose()
        {
            trayIcon.Dispose();
        }
    }
}
