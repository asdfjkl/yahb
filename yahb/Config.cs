using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yahb
{
    class Config
    {
        public string sourceDirectory;
        public string destinationDirectory;
        public List<String> fileToCopyTypes; // empty array means copy all
        public bool copySubDirectories;
        public bool includeEmptyDirectories;
        public int maxLvel;
        public List<String> inputFiles;
        public List<String> inputDirectories;
        public bool useVss;
        public List<String> filePatternsToIgnore;
        public List<String> directoriesToIgnore;
        public bool dryRun;
        public bool verboseMode;
        public string fnLogFile; // empty file name: don't log
        public bool overwriteLogFile;
        public bool writeToLogAndConsole;
        public bool showHelp;

        public Config()
        {
            this.sourceDirectory = "";
            this.destinationDirectory = "";
            this.fileToCopyTypes = new List<String>();
            this.copySubDirectories = false;
            this.includeEmptyDirectories = false;
            this.maxLvel = 2147483647; // max 32 int value, should be enough
            this.inputFiles = new List<String>();
            this.inputDirectories = new List<String>();
            this.useVss = true;
            this.filePatternsToIgnore = new List<String>();
            this.directoriesToIgnore = new List<String>();
            this.dryRun = false;
            this.verboseMode = false;
            this.fnLogFile = "";
            this.overwriteLogFile = true;
            this.writeToLogAndConsole = false;
            this.showHelp = false;
        }

        public void checkConsistency()
        {
            // check configuration for consistency/validity
        }

        public override string ToString()
        {
            string currentCfg = "";
            currentCfg += "source dir...........: " + this.sourceDirectory + "\n";
            currentCfg += "destination dir......: " + this.destinationDirectory + "\n";
            currentCfg += "copy file types......: " + String.Join(", ", this.fileToCopyTypes) + "\n";
            currentCfg += "copy sub dirs........: " + this.copySubDirectories + "\n";
            currentCfg += "include empty dirs...: " + this.includeEmptyDirectories + "\n";
            currentCfg += "max dir level........: " + this.maxLvel + "\n";
            currentCfg += "input file list......: " + String.Join(", ", this.inputFiles) + "\n";
            currentCfg += "input dirs list......: " + String.Join(", ", this.inputDirectories) + "\n";
            currentCfg += "use vss..............: " + this.useVss + "\n";
            currentCfg += "ignore patterns......: " + String.Join(", ", this.filePatternsToIgnore) + "\n";
            currentCfg += "ignore dirs..........: " + String.Join(", ", this.directoriesToIgnore) + "\n";
            currentCfg += "list only............: " + this.dryRun + "\n";
            currentCfg += "verbose mode.........: " + this.verboseMode + "\n";
            currentCfg += "log file name........: " + this.fnLogFile + "\n";
            currentCfg += "overwrite log........: " + this.overwriteLogFile + "\n";
            currentCfg += "write log and console: " + this.writeToLogAndConsole + "\n";
            currentCfg += "show help............: " + this.showHelp + "\n";
            return currentCfg;
        }
    }
}