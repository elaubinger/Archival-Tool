using ArchivalTool.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ArchivalTool
{
    /// <summary>
    /// Class containing static metadata such as directories for the archival process
    /// </summary>
    public static class ArchiveMetadata
    {
        #region Variable Declarations
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        /// <summary>
        /// Master working directory for application
        /// </summary>
        public static DirectoryInfo MasterDirectory
        {
            get => _masterDirectory ?? throw new ArchiveConfigurationException($"{nameof(MasterDirectory)} Not Found, Ensure Archive Directories are Properly Configured and Initialized");
            internal set => _masterDirectory = value;
        }

        /// <summary>
        /// Directory to create archive directories within and sort files into
        /// </summary>
        public static DirectoryInfo ArchiveDirectory
        {
            get => _archiveDirectory ?? throw new ArchiveConfigurationException($"{nameof(ArchiveDirectory)} Not Found, Ensure Archive Directories are Properly Configured and Initialized");
            internal set => _archiveDirectory = value;
        }

        /// <summary>
        /// Directory to monitor for new files for archival
        /// </summary>
        public static DirectoryInfo UnsortedDirectory
        {
            get => _unsortedDirectory ?? throw new ArchiveConfigurationException($"{nameof(UnsortedDirectory)} Not Found, Ensure Archive Directories are Properly Configured and Initialized");
            internal set => _unsortedDirectory = value;
        }

        /// <summary>
        /// Directory used to build indices
        /// </summary>
        public static DirectoryInfo IndexingDirectory
        {
            get => _indexDirectory ?? throw new ArchiveConfigurationException($"{nameof(IndexingDirectory)} Not Found, Ensure Archive Directories are Properly Configured and Initialized");
            internal set => _indexDirectory = value;
        }

        /// <summary>
        /// Get <see cref="IndexingDirectory"/> but return default value rather than throw error if indexing not initialized
        /// </summary>
        public static DirectoryInfo NullableIndexingDirectory
        {
            get
            {
                try { return IndexingDirectory; }
                catch(Exception) { return default(DirectoryInfo); }
            }
        }

        private static DirectoryInfo
            _masterDirectory,
            _archiveDirectory,
            _unsortedDirectory,
            _indexDirectory;

        private static List<Tuple<string, Regex>> SortingRules { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initiate static metadata values for archival processes
        /// </summary>
        public static void Initialize()
        {
            #region Initialize Major Directory Locations
            MasterDirectory = new DirectoryInfo(Environment.ExpandEnvironmentVariables(Settings.Default.BaseDirectory));
            if (!MasterDirectory.Exists) throw new ArchiveConfigurationException($"Configured Base Directory does not exist: {MasterDirectory.FullName}");

            Directory.SetCurrentDirectory(MasterDirectory.FullName);

            ArchiveDirectory = Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(Path.GetFullPath(Settings.Default.ArchiveFolderName)));
            UnsortedDirectory = Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(Path.GetFullPath(Settings.Default.UnsortedFolderName)));
            if(Settings.Default.UseIndexing)
                IndexingDirectory = Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(Path.GetFullPath(Settings.Default.IndexingFolderName)));

            log.Info(
                $"{Environment.NewLine}" +
                $"\t{nameof(MasterDirectory)}: {MasterDirectory.FullName}{Environment.NewLine}" +
                $"\t{nameof(ArchiveDirectory)}: {ArchiveDirectory.FullName}{Environment.NewLine}" +
                $"\t{nameof(UnsortedDirectory)}: {UnsortedDirectory.FullName}{Environment.NewLine}" +
                $"\t{nameof(IndexingDirectory)}: {IndexingDirectory?.FullName}{Environment.NewLine}"
                );
            #endregion

            #region Initialize Sorting Rules
            SortingRules = new List<Tuple<string, Regex>>();

            foreach(var entry in Settings.Default.DirectoryNameRegexRulesPairs)
            {
                var values = entry.Split(new char[] { '|' }, 
                    count: 2, 
                    options: StringSplitOptions.RemoveEmptyEntries);

                // These will be equivalent if no name is given
                string
                    regexStr = values[values.GetUpperBound(0)],
                    directoryName = values[0];

                try
                {
                    var regex = new Regex(regexStr, 
                        options:
                            RegexOptions.Singleline | 
                            RegexOptions.Compiled);
                    SortingRules.Add(new Tuple<string, Regex>(directoryName, regex));
                }
                catch(ArgumentException ex)
                {
                    log.Warn($"Regex compilation exception for proposed sorting rule (raw entry: {entry})", ex);
                    var result = DialogResult.No;
                    if(Settings.Default.AllowPrompts)
                    {
                        result = MessageBox.Show(
                            text:
                            $"An Exception Occurred Parsing the Following Proposed Rule: {entry}{Environment.NewLine}" +
                            $"\t{ex.Message}{Environment.NewLine}" +
                            $"Continue and Omit This Rule?",
                            caption: "Regex Parsing Exception",
                            icon: MessageBoxIcon.Error,
                            buttons: MessageBoxButtons.YesNo
                            );
                    }

                    if (result == DialogResult.No) throw ex;
                }
            }
            #endregion
        }
        
        public static DirectoryInfo GetArchiveDirectory(FileInfo file) => GetArchiveDirectory(file.Name);
        /// <summary>
        /// Get the directory which the given file name would be sorted into using the custom regex rules
        /// </summary>
        /// <param name="fileName">name of file to apply sorting rules to</param>
        /// <returns>directory where file would be sorted</returns>
        public static DirectoryInfo GetArchiveDirectory(string fileName)
        {
            Directory.SetCurrentDirectory(ArchiveDirectory.FullName);
            
            foreach (var rule in SortingRules) if (rule.Item2.IsMatch(fileName)) return Directory.CreateDirectory($"{Path.GetFullPath(rule.Item1)}");
            return Directory.CreateDirectory($"{Path.GetFullPath("__NoMatch")}");
        }

        public static string RemoveAddendums(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var withoutExt = Path.GetFileNameWithoutExtension(fileName);
            var result = Regex.Match(withoutExt, @"( \([0-9]?\))*$");
            return $"{withoutExt.Substring(0, result.Groups[0].Index)}{extension}";
        }
        #endregion

        #region Exception Types
        /// <summary>
        /// Occurs during exceptional behavior when either an archive directory has not been initialized or fails initialization
        /// </summary>
        private class ArchiveConfigurationException : Exception
        {
            public ArchiveConfigurationException() : base() { }
            public ArchiveConfigurationException(string message) : base(message) { }
            public ArchiveConfigurationException(string message, Exception innerException) : base(message, innerException) { }
        }

        /// <summary>
        /// Occurs on failure to parse the string encoding of a sorting rule (directory name|regex rule)
        /// </summary>
        private class SortingRuleConfigurationException : Exception
        {
            public SortingRuleConfigurationException() : base() { }
            public SortingRuleConfigurationException(string message) : base(message) { }
            public SortingRuleConfigurationException(string message, Exception innerException) : base(message, innerException) { }
        }
        #endregion
    }
}
