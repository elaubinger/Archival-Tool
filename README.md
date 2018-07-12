# ArchivalTool

--------
Overview
--------
A simple tool for archiving in the windows directory system.

------------------------------
Installation and Configuration
------------------------------
Files from the Release folder can be moved to any directory and run.  The exe must have rights to write to the directory for logging to work correctly unless the log location is changed in the .config to a different, writable location.  All options are available in the .config file.  Most important options are the directory locations and the sorting rules.

----------------------
Defining Sorting Rules
----------------------
Sorting rules are defined as follows

Directory Name|Regex to Match Against Filenames

The first pipe character is used to delimit the two segments.  If no pipe exists the entire string will be treated as regex and will be used as the directory name to sort to, this does lead to many instances where illegal characters are in the proposed directory name and sorting will fail so a name is recommended.  The regex rules are applied in the order they appear in the XML starting with the top.  The default rules sort by first alphabetical character of file name into separate folders, all files with numeric characters first into a single folder, and then all remaining into an unsorted folder.
