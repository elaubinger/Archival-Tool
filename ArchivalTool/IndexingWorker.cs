using ArchivalTool.Properties;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ArchivalTool
{
    class IndexingWorker : BackgroundWorker
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        #region Variable Declarations
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Settable value to cancel archival after current iteration
        /// </summary>
        public bool? CancelRequested;
        #endregion

        #region Constructors
        /// <summary>
        /// Generates a new IndexWorker
        /// </summary>
        /// <param name="toIndexDirectory">Directory to index files of</param>
        public IndexingWorker(DirectoryInfo toIndexDirectory)
        {
            if (!Settings.Default.UseIndexing) throw new InvalidOperationException($"{nameof(Settings.Default.UseIndexing)} must be configured and enabled");
            DoWork += async delegate
            {
                bool _cancelRequested = false;

                lock (this) _cancelRequested = CancelRequested.HasValue ? CancelRequested.Value : false;

                var files = new ConcurrentQueue<FileInfo>(
                        toIndexDirectory.EnumerateFiles("*", SearchOption.AllDirectories)
                        );

                long counter = 0;

                var indexWorker = new List<IndexWorker>();
                for (int i = 0; i < Settings.Default.SortThreads; i++)
                {
                    var worker = new IndexWorker(files);
                    worker.RunWorkerCompleted += delegate { Interlocked.Increment(ref counter); };
                    indexWorker.Add(worker);
                }
                foreach (var worker in indexWorker) worker.RunWorkerAsync();
                while (Interlocked.Read(ref counter) < Settings.Default.SortThreads) await Task.Delay(Settings.Default.ThreadWaitTime);


            };
        }
        #endregion

        #region Private Classes
        private class IndexWorker : BackgroundWorker
        {
            private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            // TODO This needs to be rethought because worker needs access to all too, maybe dictionary and queue?
            public IndexWorker(ConcurrentQueue<FileInfo> queue) : base()
            {
                DoWork += async delegate
                {
                    while (!queue.IsEmpty)
                    {
                        try
                        {
                            if (queue.TryDequeue(out var file))
                            {
                                // TODO Implement indexing behavior
                            }
                            else await Task.Delay(Settings.Default.ThreadWaitTime);
                        }
                        catch (IOException ex) { log.Info("IOException moving file", ex); }
                        catch (Exception ex) { log.Error("Exception moving file", ex); }
                    }
                };
            }
        }
        #endregion
    }
}
