using ArchivalTool.Properties;
using log4net;
using System.Drawing;
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
        #endregion
    }
}
