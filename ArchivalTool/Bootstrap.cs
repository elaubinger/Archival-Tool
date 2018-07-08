using ArchivalTool.Properties;
using log4net;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ArchivalTool
{
    /// <summary>
    /// Empty form which configures and launches the tray icon, used so that any potential future forms can be more easily integrated
    /// </summary>
    public partial class Bootstrap : Form
    {
        #region Variable Declarations
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private NotifyIcon trayIcon;
        #endregion

        #region Constructors
        public Bootstrap()
        {
            InitializeComponent();
            Hide();

            var archiveWorker = new ArchiveWorker(ArchiveMetadata.UnsortedDirectory);

            MenuItem 
                prune = new MenuItem("Prune Empty"),
                resort = new MenuItem("Re-Sort Existing");

            prune.Click += delegate
            {
                prune.Enabled = false;

                var worker = new BackgroundWorker();
                worker.DoWork += delegate
                {
                    foreach (var directory in ArchiveMetadata.ArchiveDirectory.GetDirectories())
                    {
                        if (directory.Exists && !directory.EnumerateFiles().Any())
                        {
                            log.Info($"Pruning Directory: {directory.FullName}");
                            directory.Delete();
                        }
                    }
                };
                worker.RunWorkerCompleted += delegate { prune.Enabled = true; };
                worker.RunWorkerAsync();
            };

            resort.Click += delegate
            {
                resort.Enabled = false;

                var worker = new ArchiveWorker(ArchiveMetadata.ArchiveDirectory, ArchiveWorker.ArchiveWorkerMode.Once);
                worker.RunWorkerCompleted += delegate { resort.Enabled = true; };
                worker.RunWorkerAsync();
            };

            // Initialize Tray Icon
            trayIcon = new NotifyIcon
            {
                
                Icon = Icon.FromHandle(Resources.bolt_2x.GetHicon()),
                ContextMenu = new ContextMenu(new MenuItem[] {
                    prune,
                    resort,
                    new MenuItem("Exit", delegate
                    {
                        archiveWorker.CancelRequested = true;
                        trayIcon.Visible = false;
                        Application.Exit();
                    })
                }),
                Visible = true
            };

            archiveWorker.RunWorkerAsync();
        }
        #endregion
    }
}
