using log4net;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ArchivalTool.Properties;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Linq;

namespace ArchivalTool
{
    /// <summary>
    /// Background worker who manages the functionality of the archival process
    /// </summary>
    public class ArchiveWorker : BackgroundWorker
    {
        #region Public Enums
        /// <summary>
        /// Enum defining modes of operation for the archival process
        /// </summary>
        public enum ArchiveWorkerMode
        {
            Continuous,
            Once
        }
        #endregion

        #region Variable Declarations
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Settable value to cancel archival after current iteration
        /// </summary>
        public bool? CancelRequested;
        #endregion

        #region Constructors
        /// <summary>
        /// Master worker which dispatches subworkers to perform archival
        /// </summary>
        /// <param name="toSortDirectory">Directory which provides the files to be sorted</param>
        /// <param name="mode">The mode of operating for the archival process</param>
        public ArchiveWorker(DirectoryInfo toSortDirectory, ArchiveWorkerMode mode = ArchiveWorkerMode.Continuous) : base()
        {
            DoWork += async delegate
            {
                bool _cancelRequested = false;
                while (!_cancelRequested)
                {
                    var start = DateTime.Now;

                    #region Sort Unsorted Files
                    var files = new ConcurrentQueue<FileInfo>(
                        toSortDirectory.EnumerateFiles("*", SearchOption.AllDirectories)
                        );

                    long counter = 0;

                    var sortWorkers = new List<SortWorker>();
                    for (int i = 0; i < Settings.Default.SortThreads; i++)
                    {
                        var worker = new SortWorker(files);
                        worker.RunWorkerCompleted += delegate { Interlocked.Increment(ref counter); };
                        sortWorkers.Add(worker);
                    }
                    foreach (var worker in sortWorkers) worker.RunWorkerAsync();
                    while (Interlocked.Read(ref counter) < Settings.Default.SortThreads) await Task.Delay(Settings.Default.ThreadWaitTime);
                    #endregion

                    #region Prune Empty Directories
                    foreach (var directory in ArchiveMetadata.ArchiveDirectory.GetDirectories())
                    {
                        if (directory.Exists && !directory.EnumerateFiles("*", System.IO.SearchOption.AllDirectories).Any())
                        {
                            log.Info($"Pruning Directory: {directory.FullName}");
                            directory.Delete(true);
                        }
                    }
                    #endregion

                    lock (this) _cancelRequested = CancelRequested.HasValue ? CancelRequested.Value : false;
                    if (mode == ArchiveWorkerMode.Once) _cancelRequested = true;
                    
                    if (!_cancelRequested)
                    {
                        #region Compute Timespan and Wait
                        var timespan = DateTime.Now - start;
                        var waitTime = Math.Max(Settings.Default.ArchiveCycleTime - timespan.Milliseconds, 0);
                        await Task.Delay(waitTime);
                        #endregion
                    }
                }
            };
        }
        #endregion
        
        #region Private Classes
        /// <summary>
        /// Subworker for archival process which are of a configurable number and remove files to sort from the concurrent queue until all have been processed
        /// </summary>
        private class SortWorker : BackgroundWorker
        {
            private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            public SortWorker(ConcurrentQueue<FileInfo> queue) : base()
            {
                DoWork += async delegate
                {
                    while(!queue.IsEmpty)
                    {
                        try
                        {
                            if (queue.TryDequeue(out var file))
                            {
                                if (file.IsReadOnly) file.Attributes = FileAttributes.Normal;

                                var folder = ArchiveMetadata.GetArchiveDirectory(file);

                                for (int i = 0; i < Settings.Default.FileMoveAttempts; i++)
                                {
                                    try
                                    {
                                        string
                                            newFullName = $"{folder.FullName}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(ArchiveMetadata.RemoveAddendums(file.Name))}";
                                        if (File.Exists($"{newFullName}{file.Extension}"))
                                        {
                                            int iterator = 0;
                                            while (File.Exists($"{newFullName} ({iterator}){file.Extension}")) iterator++;
                                            newFullName = $"{newFullName} ({iterator})";
                                        }
                                        file.MoveTo($"{newFullName}{file.Extension}");
                                    }
                                    catch (IOException)
                                    {
                                        await Task.Delay(Settings.Default.ThreadWaitTime);
                                        continue;
                                    }
                                    break;
                                }
                                log.Info($"Sorted File: {file.FullName}");
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
