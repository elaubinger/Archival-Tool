﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ArchivalTool.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.6.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("F:\\WiFi Storage")]
        public string BaseDirectory {
            get {
                return ((string)(this["BaseDirectory"]));
            }
            set {
                this["BaseDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Archive")]
        public string ArchiveFolderName {
            get {
                return ((string)(this["ArchiveFolderName"]));
            }
            set {
                this["ArchiveFolderName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Unsorted")]
        public string UnsortedFolderName {
            get {
                return ((string)(this["UnsortedFolderName"]));
            }
            set {
                this["UnsortedFolderName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8")]
        public ushort SortThreads {
            get {
                return ((ushort)(this["SortThreads"]));
            }
            set {
                this["SortThreads"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50")]
        public int ThreadWaitTime {
            get {
                return ((int)(this["ThreadWaitTime"]));
            }
            set {
                this["ThreadWaitTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5000")]
        public int ArchiveCycleTime {
            get {
                return ((int)(this["ArchiveCycleTime"]));
            }
            set {
                this["ArchiveCycleTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AllowPrompts {
            get {
                return ((bool)(this["AllowPrompts"]));
            }
            set {
                this["AllowPrompts"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>A|^[aA]</string>
  <string>B|^[bB]</string>
  <string>C|^[cC]</string>
  <string>D|^[dD]</string>
  <string>E|^[eE]</string>
  <string>F|^[fF]</string>
  <string>G|^[gG]</string>
  <string>H|^[hH]</string>
  <string>I|^[iI]</string>
  <string>J|^[jJ]</string>
  <string>K|^[kK]</string>
  <string>L|^[lL]</string>
  <string>M|^[mM]</string>
  <string>N|^[nN]</string>
  <string>O|^[oO]</string>
  <string>P|^[pP]</string>
  <string>Q|^[qQ]</string>
  <string>R|^[rR]</string>
  <string>S|^[sS]</string>
  <string>T|^[tT]</string>
  <string>U|^[uU]</string>
  <string>V|^[vV]</string>
  <string>W|^[wW]</string>
  <string>X|^[xX]</string>
  <string>Y|^[yY]</string>
  <string>Z|^[zZ]</string>
  <string>Numeric|^[0-9]</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection DirectoryNameRegexRulesPairs {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["DirectoryNameRegexRulesPairs"]));
            }
            set {
                this["DirectoryNameRegexRulesPairs"] = value;
            }
        }
    }
}
