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
    public static class ArchiveMetadata
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static DirectoryInfo MasterDirectory { get; internal set; }
        public static DirectoryInfo ArchiveDirectory { get; internal set; }
        public static DirectoryInfo UnsortedDirectory { get; internal set; }
        public static List<Tuple<string, Regex>> SortingRules { get; internal set; }

        public static void Initialize()
        {
            #region Initialize Major Directory Locations
            MasterDirectory = new DirectoryInfo(Environment.ExpandEnvironmentVariables(Settings.Default.BaseDirectory));
            if (!MasterDirectory.Exists) throw new ArchiveConfigurationException($"Configured Base Directory does not exist: {MasterDirectory.FullName}");

            Directory.SetCurrentDirectory(MasterDirectory.FullName);

            ArchiveDirectory = Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(Path.GetFullPath(Settings.Default.ArchiveFolderName)));
            UnsortedDirectory = Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(Path.GetFullPath(Settings.Default.UnsortedFolderName)));

            log.Info(
                $"{Environment.NewLine}" +
                $"\tMaster Directory: {MasterDirectory.FullName}{Environment.NewLine}" +
                $"\tArchive Directory: {ArchiveDirectory.FullName}{Environment.NewLine}" +
                $"\tUnsorted Directory: {UnsortedDirectory.FullName}{Environment.NewLine}"
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
        public static DirectoryInfo GetArchiveDirectory(string fileName)
        {
            Directory.SetCurrentDirectory(ArchiveDirectory.FullName);
            
            foreach (var rule in SortingRules) if (rule.Item2.IsMatch(fileName)) return Directory.CreateDirectory($"{Path.GetFullPath(rule.Item1)}");
            return Directory.CreateDirectory($"{Path.GetFullPath("__NoMatch")}");
        }

        private class ArchiveConfigurationException : Exception
        {
            public ArchiveConfigurationException() : base() { }
            public ArchiveConfigurationException(string message) : base(message) { }
            public ArchiveConfigurationException(string message, Exception innerException) : base(message, innerException) { }
        }

        private class SortingRuleConfigurationException : Exception
        {
            public SortingRuleConfigurationException() : base() { }
            public SortingRuleConfigurationException(string message) : base(message) { }
            public SortingRuleConfigurationException(string message, Exception innerException) : base(message, innerException) { }
        }
    }
}
