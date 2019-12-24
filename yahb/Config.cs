using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace yahb
{
    class Config
    {
        public string sourceDirectory;
        public string destinationDirectory;
        public bool copySubDirectories;
        public int maxLvel;
        public string fnInputDirectories;
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
        public List<String> fileEndings;
        public bool copyAll;
        public bool logToFile;

        public List<String> commonDirsToIgnore;
        public List<String> commonFilePatternsToIgnore;

        private int logsCached;

        public Config()
        {
            this.sourceDirectory = "";
            this.destinationDirectory = "";
            this.copySubDirectories = false;
            this.maxLvel = 2147483647; // max 32 int value, should be enough
            this.fnInputDirectories = "";
            this.inputDirectories = new List<String>();
            this.useVss = false;
            this.filePatternsToIgnore = new List<String>();
            this.directoriesToIgnore = new List<String>();
            this.dryRun = false;
            this.verboseMode = false;
            this.fnLogFile = "";
            this.overwriteLogFile = true;
            this.writeToLogAndConsole = false;
            this.showHelp = false;
            this.fileEndings = new List<String>();
            this.copyAll = false;
            this.logToFile = false;
            this.commonDirsToIgnore = new List<String>();
            commonDirsToIgnore.Add("System Volume Information");
            commonDirsToIgnore.Add("AppData\\Local\\Temp");
            commonDirsToIgnore.Add("AppData\\Local\\Microsoft\\Windows\\INetCache");
            commonDirsToIgnore.Add("C:\\Windows");
            commonDirsToIgnore.Add("$Recycle.Bin");
            this.commonFilePatternsToIgnore = new List<String>();
            commonFilePatternsToIgnore.Add("hiberfil.sys");
            commonFilePatternsToIgnore.Add("pagefile.sys");
            commonFilePatternsToIgnore.Add("swapfile.sys");
            commonFilePatternsToIgnore.Add("*.~");
            commonFilePatternsToIgnore.Add("*.tmp");

            this.logsCached = 0;
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

            /*
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
            } */

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

            // check if we have a valid destination directory
            if(string.IsNullOrEmpty(this.destinationDirectory) || !System.IO.Directory.Exists(this.destinationDirectory))  {
                throw new ArgumentException("error: no valid output directory defined or directory " +
                    "does not exist");
            }

            if (this.destinationDirectory.EndsWith(":"))
            {
                this.destinationDirectory += "\\";
            }

            if (this.useVss && !this.IsAdmin)
            {
                throw new ArgumentException("error: shadow copy /sc requested, but program is not run with admin rights!");
            }

            // check if we have a valid log-file path
            if(!string.IsNullOrEmpty(this.fnLogFile))
            {
                // try to write or append log file
                // write time-stamp in header
                string now = DateTime.Now.ToString("yyyy'_'MM'_'dd_HH'_'mm'_'ss'Z'");
                if(this.overwriteLogFile)
                {
                    try
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(this.fnLogFile))
                        {
                            file.WriteLine("########################################");
                            file.WriteLine("# Copying started@ " + now);
                            file.WriteLine("########################################");
                        }
                        this.logToFile = true;

                    }
                    catch (System.IO.IOException)
                    {
                        throw new ArgumentException("error: IO exception writing to log file " + this.fnLogFile);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException("error: general exception writing to log file " + this.fnLogFile);
                    }

                } else
                {
                    try
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(this.fnLogFile, append: true))
                        {
                            file.WriteLine("########################################");
                            file.WriteLine("# Copying started@ " + now);
                            file.WriteLine("########################################");
                        }
                        this.logToFile = true;
                    }
                    catch (System.IO.IOException)
                    {
                        throw new ArgumentException("error: IO exception appending to log file " + this.fnLogFile);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException("error: general exception appending to log file " + this.fnLogFile);
                    }

                }           
            }
        }


        public void addToLog(string message)
        {
            if(this.logToFile)
            {
                if(this.writeToLogAndConsole)
                {
                    Console.WriteLine(message);
                }
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(this.fnLogFile, append: true))
                {
                    file.WriteLine(message);
                }
            } else
            {
                Console.WriteLine(message);
            }

            this.logsCached += 1;
            if(this.logsCached > 10)
            {
                Console.Out.Flush();
                this.logsCached = 0;
            }

        }

        
        private bool IsAdmin
        {
            get
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                if (identity != null)
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    List<Claim> list = new List<Claim>(principal.UserClaims);
                    Claim c = list.Find(p => p.Value.Contains("S-1-5-32-544"));
                    if (c != null)
                        return true;
                }
                return false;
            }
        }

        public override string ToString()
        {
            string currentCfg = "";
            currentCfg += "source dir...........: " + this.sourceDirectory + "\n";
            currentCfg += "destination dir......: " + this.destinationDirectory + "\n";
            currentCfg += "file endings.........: " + String.Join(", ", this.fileEndings) + "\n";
            currentCfg += "copy sub dirs........: " + this.copySubDirectories + "\n";
            currentCfg += "max dir level........: " + this.maxLvel + "\n";
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