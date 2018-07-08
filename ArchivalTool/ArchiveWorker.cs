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

namespace ArchivalTool
{
    public class ArchiveWorker : BackgroundWorker
    {
        public enum ArchiveWorkerMode
        {
            Continuous,
            Once
        }

        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool? CancelRequested;

        public ArchiveWorker(DirectoryInfo toSortDirectory, ArchiveWorkerMode mode = ArchiveWorkerMode.Continuous) : base()
        {
            DoWork += async delegate
            {
                CancelRequested = false;
                while (CancelRequested.HasValue && !CancelRequested.Value)
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
                    
                    if (mode == ArchiveWorkerMode.Once) CancelRequested = true;
                    else
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
                                string
                                    newFullName = $"{folder.FullName}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(file.Name)}";
                                if (File.Exists($"{newFullName}{file.Extension}"))
                                {
                                    int iterator = 0;
                                    while (File.Exists($"{newFullName} ({iterator}){file.Extension}")) iterator++;
                                    newFullName = $"{newFullName} ({iterator})";
                                }
                                file.MoveTo($"{newFullName}{file.Extension}");
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
    }
}
