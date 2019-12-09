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
        public string fnInputFiles;
        public string fnInputDirectories;
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
            this.fnInputFiles = "";
            this.fnInputDirectories = "";
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
            // check if we have either an input directory, or supplied file names via /if or /id
            // with valid directory or file lists
            bool hasInput = false;
            if(!string.IsNullOrEmpty(this.sourceDirectory))
            {
                if(System.IO.Directory.Exists(this.sourceDirectory))
                {
                    hasInput = true;
                } else
                {
                    throw new ArgumentException("error: " + this.sourceDirectory + " is not a valid directory");
                }
            }

            // check if the supplied list of input filenames (if any) is valid
            if (!string.IsNullOrEmpty(this.fnInputFiles))
            {
                try
                {
                    string[] lines = System.IO.File.ReadAllLines(this.fnInputFiles);
                    foreach (string line in lines)
                    {
                        if(!System.IO.File.Exists(line))
                        {
                            throw new ArgumentException("error: " + line + " defined in " +
                                this.fnInputFiles + " does not exist");
                        }
                        this.inputFiles.Add(line);
                    }
                    hasInput = true;
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Error: could not load input directories from: " + this.fnInputDirectories);
                }
            }

            // check if the supplied list of input directories (if any) is valid
            if (!string.IsNullOrEmpty(this.fnInputDirectories))
            {
                try
                {
                    string[] lines = System.IO.File.ReadAllLines(this.fnInputDirectories);
                    foreach (string line in lines)
                    {
                        if(!System.IO.Directory.Exists(line))
                        {
                            throw new ArgumentException("error: " + line + " defined in " + 
                                this.fnInputDirectories + " is not a valid directory");
                        }
                        this.inputDirectories.Add(line);
                    }
                    hasInput = true;
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Error: could not load input directories from: " + this.fnInputDirectories);
                }
            }

            if(!hasInput)
            {
                throw new ArgumentException("error: no valid input directory defined, and no valid " +
                    "directory or file lists supplied via /id or /if");
            }

            if(string.IsNullOrEmpty(this.destinationDirectory) || !System.IO.File.Exists(this.destinationDirectory)  {
                throw new ArgumentException("error: no valid output directory defined or directory" +
                    "does not exist");
            }
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