using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace yahb
{
    struct DestinationFile
    {
        public string destDrive;
        public string timestamp;
        public string fileName;
        public string driveTimeFilename;
    }

    class CopyModule
    {
        const int ERROR_SHARING_VIOLATION = 32;
        const int ERROR_LOCK_VIOLATION = 33;

        private Config cfg;
        private string now;
        List<String> sourceFileList;
        List<String> sourceDirList;
        List<DestinationFile> destFileList;
        List<String> destDirList;

        public CopyModule(Config cfg)
        {
            this.cfg = cfg;
            this.now = DateTime.Now.ToString("yyyyMMddHHmm");
            this.sourceFileList = new List<String>();
            this.sourceDirList = new List<String>();
            this.destFileList = new List<DestinationFile>();
            this.destDirList = new List<String>();
        }

        public List<String> createDirectoryList() {

            cfg.addToLog("creating list of directories ... ");
            
            List<String> dirs = new List<String>();
            // first check provided source directory and all subdirectories, if required
            if (cfg.copySubDirectories)
            {
                Stack<string> dir_stack = new Stack<string>(20);
                dir_stack.Push(cfg.sourceDirectory);

                List<string> subdirs;
                while (dir_stack.Count > 0)
                {
                    string currentDir = dir_stack.Pop();
                    try
                    {
                        subdirs = new List<string>(Directory.EnumerateDirectories(currentDir));
                        string currentDirName = new DirectoryInfo(currentDir).Name;
                        bool addDir = true;
                        if (!cfg.copyAll)
                        {
                            foreach (string commonDir in cfg.commonDirsToIgnore)
                            {
                                if (currentDir.Contains(commonDir))
                                {
                                    addDir = false;
                                    break;
                                }
                            }
                        }
                        if (addDir)
                        {
                            dirs.Add(currentDir);
                        } else
                        {
                            if(cfg.verboseMode)
                            {
                                cfg.addToLog(currentDir + ": skipping");
                            }
                        }
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        if (cfg.verboseMode)
                        {
                            cfg.addToLog(currentDir + ":" + e.Message);
                        }
                        continue;
                    }
                    catch (DirectoryNotFoundException e)
                    {
                        if (cfg.verboseMode)
                        {
                            cfg.addToLog(currentDir + ":" + e.Message);
                        }
                        continue;
                    }
                    catch (PathTooLongException e)
                    {
                        if (cfg.verboseMode)
                        {
                            cfg.addToLog(currentDir + ":" + e.Message);
                        }
                        continue;
                    }
                    foreach (string str in subdirs)
                    {
                        dir_stack.Push(str);
                    }
                }
            } else
            {
                dirs.Add(cfg.sourceDirectory);
            }
            // get directories from provided input file
            dirs.AddRange(cfg.inputDirectories);

            // filter directories according to input given
            List<String> dirs_filtered = new List<String>();
            foreach (string dir_i in dirs)
            {
                bool addDir = true;
                foreach (string pattern_i in cfg.directoriesToIgnore)
                {
                    if (dir_i.Contains(pattern_i))
                    {
                        addDir = false;
                        break;
                    }
                }
                if (addDir)
                {
                    dirs_filtered.Add(dir_i);
                } else
                {
                    if (cfg.verboseMode)
                    {
                        cfg.addToLog(dir_i + ": skipping");
                    }
                }
            }
            cfg.addToLog("creating list of directories ... DONE");
            return dirs_filtered;
        }

        public void createFileList(List<String> inputDirList) {

            cfg.addToLog("create list of files ... ");

            foreach (string dir_i in inputDirList)
            {
                try
                {
                    this.destDirList.Add(this.createDirDestPath(dir_i, cfg.destinationDirectory));
                    sourceDirList.Add(dir_i);
                }
                catch (System.IO.PathTooLongException e)
                {
                    if (cfg.verboseMode)
                    {
                        this.cfg.addToLog(dir_i + ": " + e.Message);
                    }
                    continue;
                }

                string[] files_dir_i = System.IO.Directory.GetFiles(dir_i);
                foreach (string file_i in files_dir_i)
                {
                    bool addFile = false;
                    // only add those files that are in our extension list
                    if (cfg.fileEndings.Count() > 0)
                    {
                        foreach (string pattern_i in cfg.fileEndings)
                        {
                            if (this.Like(file_i, pattern_i))
                            {
                                addFile = true;
                                break;
                            }
                        }
                    } else
                    {
                        addFile = true;
                    }
                    
                    if (cfg.filePatternsToIgnore.Count > 0 && addFile == true)
                    {
                        foreach (string pattern_i in cfg.filePatternsToIgnore)
                        {
                            if (this.Like(file_i, pattern_i))
                            {
                                addFile = false;
                                break;
                            }
                        }
                    }

                    if(!cfg.copyAll)
                    {
                        foreach (string pattern_i in cfg.commonFilePatternsToIgnore)
                        {
                            if (this.Like(file_i, pattern_i))
                            {
                                addFile = false;
                                break;
                            }
                        }
                    }

                    if (addFile)
                    {

                        try
                        {
                            this.destFileList.Add(this.createFileDestPath(file_i, cfg.destinationDirectory));
                            this.sourceFileList.Add(file_i);
                        } catch (System.IO.PathTooLongException e)
                        {
                            if (cfg.verboseMode)
                            {
                                this.cfg.addToLog(file_i + ": " + e.Message);
                            }
                        }
                    } else
                    {
                        if (cfg.verboseMode)
                        {
                            cfg.addToLog(file_i + ": skipping");
                        }
                    }
                }
            }

            /*
            Console.WriteLine("input directories: ");
            Console.WriteLine(String.Join("\n", sourceDirList));
            Console.WriteLine("destination directories: ");
            Console.WriteLine(String.Join("\n", destDirList));
            Console.WriteLine("input file list: ");
            Console.WriteLine(String.Join("\n", sourceFileList));
            Console.WriteLine("destination file list: ");
            foreach(DestinationFile df in destFileList)
            {
                Console.WriteLine(df.driveTimeFilename);
            }*/
            cfg.addToLog("create list of files ... DONE");
        }

        public string getLastDir(string destDirectory)
        {
            try
            {
                string[] dirs = System.IO.Directory.GetDirectories(destDirectory);
                List<string> numberDirs = new List<string>();
                foreach (string dir_i in dirs)
                {
                    DirectoryInfo f = new DirectoryInfo(dir_i);
                    string src_drive = Path.GetPathRoot(f.FullName);
                    //string dir_i_wo_drive = dir_i.Substring(src_drive.Length-1, dir_i.Length - src_drive.Length+1);
                    string dir_i_wo_drive = f.Name;

                    //Console.WriteLine("checking dir_i " + dir_i_wo_drive);

                    if(this.IsDigitsOnly(dir_i_wo_drive))
                    {
                        numberDirs.Add(dir_i);
                    }
                }
                numberDirs.Sort();
                numberDirs.Reverse();
                if(numberDirs.Count > 0)
                {
                    return numberDirs[0];
                } else
                {
                    throw new ArgumentException();
                }

            }
            catch (UnauthorizedAccessException e)
            {
                if (cfg.verboseMode)
                {
                    this.cfg.addToLog(destDirectory + ": " + e.Message);
                }
                throw new ArgumentException();
            }
            catch (DirectoryNotFoundException e)
            {
                if (cfg.verboseMode)
                {
                    this.cfg.addToLog(destDirectory + ": " + e.Message);
                }
                throw new ArgumentException();
            }
            catch (PathTooLongException e)
            {
                if (cfg.verboseMode)
                {
                    this.cfg.addToLog(destDirectory + ": " + e.Message);
                }
                throw new ArgumentException();
            }
        }

        public void doCopy()
        {
            List<Tuple<String, DestinationFile>> tryWithVSS = new List<Tuple<String, DestinationFile>>();

            this.cfg.addToLog("-----------------------------------------------------------------");

            bool foundLastDir = false;
            string lastBackupDirectory = "";
            try
            {
                lastBackupDirectory = this.getLastDir(cfg.destinationDirectory);
                this.cfg.addToLog("identified last backup directory: " + lastBackupDirectory);
                foundLastDir = true;
            } catch (ArgumentException e)
            {
                this.cfg.addToLog("unable to identify a previous backup location, copying all");
            }

            // first create all target directories
            foreach (string destDir in this.destDirList)
            {
                try
                {
                    if (!cfg.dryRun)
                    {
                        System.IO.Directory.CreateDirectory(destDir);
                    }
                    cfg.addToLog(destDir + ": created");
                    
                }
                catch (DirectoryNotFoundException ex)
                {
                    if (cfg.verboseMode)
                    {
                        this.cfg.addToLog(destDir + ": " + ex.Message);
                    }
                    continue;
                }
                catch (NotSupportedException ex)
                {
                    if (cfg.verboseMode)
                    {
                        this.cfg.addToLog(destDir + ": " + ex.Message);
                    }
                    continue;
                }
                catch (PathTooLongException ex)
                {
                    if (cfg.verboseMode)
                    {
                        this.cfg.addToLog(destDir + ": " + ex.Message);
                    }
                    continue;
                }
                catch (UnauthorizedAccessException ex)
                {
                    if (cfg.verboseMode)
                    {
                        this.cfg.addToLog(destDir + ": " + ex.Message);
                    }
                    continue;
                }
                catch (ArgumentException ex)
                {
                    if (cfg.verboseMode)
                    {
                        this.cfg.addToLog(destDir + ": " + ex.Message);
                    }
                    continue;
                }
                catch (IOException ex)
                {
                    if (cfg.verboseMode)
                    {
                        this.cfg.addToLog(destDir + ": " + ex.Message);
                    }
                    continue;
                }
            }

            // copy all files
            var sourceDestFiles = this.sourceFileList.Zip(this.destFileList, (a, b) => new { sourceFile = a, destFile = b });
            foreach (var x in sourceDestFiles)
            {
                try
                {
                    FileInfo fi_source = new FileInfo(x.sourceFile);
                    bool makeCopy = true;
                    if (foundLastDir) {
                        // first check if file exists in old backup
                        string oldFn = Path.Combine(x.destFile.destDrive, lastBackupDirectory, x.destFile.fileName);
                        //Console.WriteLine("checking if: " + oldFn + " exists...");
                        FileInfo fi_backup = new FileInfo(oldFn);
                        if (fi_backup.Exists)
                        {
                            if(fi_source.CreationTimeUtc == fi_backup.CreationTimeUtc &&
                                fi_source.LastWriteTimeUtc == fi_backup.LastWriteTimeUtc &&
                                fi_source.Length == fi_backup.Length)
                            {
                                if (!cfg.dryRun)
                                {

                                    //Console.WriteLine("no need to copy " + x.sourceFile + ", already exists in backup");
                                    if (CreateHardLink(x.destFile.driveTimeFilename, oldFn, IntPtr.Zero))
                                    {
                                        makeCopy = false;
                                        cfg.addToLog(x.sourceFile + ": creating hardlink to " + oldFn);
                                    }
                                    else
                                    {
                                        cfg.addToLog(x.sourceFile + ": unable to create hardlink, copying instead");
                                    }
                                } else
                                {
                                    cfg.addToLog(x.sourceFile + ": creating hardlink to " + oldFn);
                                }
                            }
                        }
                    }
                    if (!cfg.dryRun)
                    {
                        if (makeCopy)
                        {
                            File.Copy(x.sourceFile, x.destFile.driveTimeFilename);
                            FileInfo fi_dest = new FileInfo(x.destFile.driveTimeFilename);
                            fi_dest.CreationTime = fi_source.CreationTime;
                            fi_dest.LastWriteTime = fi_source.LastWriteTime;
                            fi_dest.LastAccessTime = fi_source.LastAccessTime;
                            cfg.addToLog(x.sourceFile + ": file copied");
                        }
                    } else
                    {
                        cfg.addToLog(x.sourceFile + ": file copied");
                    }
                }
                catch (IOException copyError)
                {
                    if(cfg.useVss)
                    {
                        int errorCode = Marshal.GetHRForException(copyError) & ((1 << 16) - 1);
                        if (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION)
                        {
                            if (cfg.verboseMode)
                            {
                                cfg.addToLog(x.sourceFile + ": sharing or lock violation, trying later w/ shadow copy");
                            }
                            Tuple<String, DestinationFile> tpl = new Tuple<String, DestinationFile>(x.sourceFile, x.destFile);
                            tryWithVSS.Add(tpl);
                        } else
                        {
                            if (cfg.verboseMode)
                            {
                                cfg.addToLog(x.sourceFile + ": " + copyError.Message);
                            }
                        }

                    } else
                    {
                        if (cfg.verboseMode)
                        {
                            cfg.addToLog(x.sourceFile + ": " + copyError.Message);
                        }
                    }
                }
            }

            if (this.cfg.useVss && tryWithVSS.Count > 0)
            {
                // todo catch all typical exceptions
                using (VssBackup vss = new VssBackup())
                {
                    vss.Setup(Path.GetPathRoot(tryWithVSS[0].Item1));
                    
                    foreach(Tuple<String, DestinationFile> x in tryWithVSS)
                    {
                        string snap_path = vss.GetSnapshotPath(x.Item1);
                        Alphaleonis.Win32.Filesystem.File.Copy(snap_path, x.Item2.driveTimeFilename);
                        cfg.addToLog(x.Item1 + ": copied snapshot via VSS");
                    }
                }
            }


            /*

// Initialize the shadow copy subsystem.
using (VssBackup vss = new VssBackup())
{
vss.Setup(Path.GetPathRoot(x.sourceFile));
string snap_path = vss.GetSnapshotPath(x.sourceFile);

cfg.addToLog(snap_path);

//string destpath = "F:\\201912232238\\c__\\Users\\user\\MyFiles\\workspace\\filelock\\foo.txt";
string a = "F:\\" + x.destFile.timestamp + "\\" + x.destFile.fileName;
cfg.addToLog(a);

Alphaleonis.Win32.Filesystem.File.Copy(snap_path, a);

//File.Copy(snap_path, x.destFile.driveTimeFilename);
cfg.addToLog(x.sourceFile + ": copied snapshot via VSS");

}*/

            this.cfg.addToLog("-----------------------------------------------------------------");
        }

        public DestinationFile createFileDestPath(string fileSourcePath, string destDrive)
        {
            if(destDrive.EndsWith(":"))
            {
                destDrive += "\\";
            }
            FileInfo f = new FileInfo(fileSourcePath);
            string src_drive = Path.GetPathRoot(f.FullName);
            string dest_file = fileSourcePath.Substring(src_drive.Length, fileSourcePath.Length - src_drive.Length);
            string src_drive_clean = src_drive.Replace(':', '_').Replace('\\', '_');
            DestinationFile df = new DestinationFile();
            df.destDrive = destDrive;
            df.timestamp = now;
            df.fileName = Path.Combine(src_drive_clean, dest_file);
            df.driveTimeFilename = Path.Combine(destDrive, now, src_drive_clean, dest_file);
            return df;
        }

        public string createDirDestPath(string dirSourcePath, string destDrive)
        {
            if (destDrive.EndsWith(":"))
            {
                destDrive += "\\";
            }
            DirectoryInfo f = new DirectoryInfo(dirSourcePath);
            string src_drive = Path.GetPathRoot(f.FullName);
            string des_dir = dirSourcePath.Substring(src_drive.Length, dirSourcePath.Length - src_drive.Length);
            string src_drive_clean = src_drive.Replace(':', '_').Replace('\\', '_');
            return Path.Combine(destDrive, now, src_drive_clean, des_dir);
        }


        public bool Like(string str, string pattern)
        {
            return new Regex(
                "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            ).IsMatch(str);
        }

        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool CreateHardLink(
        string lpFileName,
        string lpExistingFileName,
        IntPtr lpSecurityAttributes
        );


        /*
         * better: use File.Copy, check for IOException and then for ERROR_SHARING_VIOLATION
         * 
        private bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        } */

        /*
         * 
         * 
         * 

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode )]
        static extern bool CreateHardLink(
        string lpFileName,
        string lpExistingFileName,
        IntPtr lpSecurityAttributes
        );

        Usage:

        CreateHardLink(newLinkPath,sourcePath, IntPtr.Zero);
        */

    }
}
