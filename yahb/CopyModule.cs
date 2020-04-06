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
                if (!String.IsNullOrEmpty(cfg.sourceDirectory))
                {
                    dir_stack.Push(cfg.sourceDirectory);
                }

                // add directories from file here to stack
                // /s is used, in order to get all
                // subdirs
                if(cfg.copySubDirectories)
                {
                    foreach( string dir_i in cfg.inputDirectories)
                    {
                        dir_stack.Push(dir_i);
                    }
                }

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
                                // like ".Contains" but case insensitive
                                if (currentDir.IndexOf(commonDir, StringComparison.OrdinalIgnoreCase) >= 0)
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
                        cfg.addToLog("ERR:" + currentDir + ":" + e.Message);                        
                        continue;
                    }
                    catch (DirectoryNotFoundException e)
                    {
                        cfg.addToLog("ERR:" + currentDir + ":" + e.Message);
                        continue;
                    }
                    catch (PathTooLongException e)
                    {
                        cfg.addToLog("ERR:" + currentDir + ":" + e.Message);
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
            // but only if /s was not provided (otherwise)
            // above stack operation added them incl. subdirs
            if (!cfg.copySubDirectories)
            {
                dirs.AddRange(cfg.inputDirectories);
            }

            // filter directories according to input given
            List<String> dirs_filtered = new List<String>();
            foreach (string dir_i in dirs)
            {
                bool addDir = true;
                foreach (string pattern_i in cfg.directoriesToIgnore)
                {
                    if (dir_i.IndexOf(pattern_i, StringComparison.OrdinalIgnoreCase) >= 0)
                    //if (dir_i.Contains(pattern_i))
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

            cfg.addToLog("creating list of files ... ");

            foreach (string dir_i in inputDirList)
            {
                try
                {
                    this.destDirList.Add(this.createDirDestPath(dir_i, cfg.destinationDirectory));
                    sourceDirList.Add(dir_i);
                }
                catch (System.IO.PathTooLongException e)
                {
                    this.cfg.addToLog("ERR:" + dir_i + ": " + e.Message);
                    continue;
                }

                string[] files_dir_i = System.IO.Directory.GetFiles(dir_i);
                foreach (string file_i in files_dir_i)
                {
                    bool addFile = false;
                    string file_i_pure = System.IO.Path.GetFileName(file_i);
                    // only add those files that are in our extension list
                    if (cfg.fileEndings.Count() > 0)
                    {
                        foreach (string pattern_i in cfg.fileEndings)
                        {
                            if (this.Like(file_i_pure, pattern_i))
                            {
                                addFile = true;
                                break;
                            }
                        }
                    } else // otherwise take file
                    {
                        addFile = true;
                    }
                    
                    if (cfg.filePatternsToIgnore.Count > 0 && addFile == true)
                    {
                        foreach (string pattern_i in cfg.filePatternsToIgnore)
                        {
                            if (this.Like(file_i_pure, pattern_i))
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
                            // in our common File patterns, we also have full file names like "hiberfil.sys"
                            // hence we must also check equality
                            if (this.Like(file_i_pure, pattern_i) || string.Equals(file_i_pure, pattern_i))
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
                            this.cfg.addToLog("ERR:" + file_i + ": " + e.Message);
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
            cfg.addToLog("creating list of files ... DONE");
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
                this.cfg.addToLog("ERR:" + destDirectory + ": " + e.Message);
                throw new ArgumentException();
            }
            catch (DirectoryNotFoundException e)
            {
                this.cfg.addToLog("ERR:" + destDirectory + ": " + e.Message);
                throw new ArgumentException();
            }
            catch (PathTooLongException e)
            {
                this.cfg.addToLog("ERR:" + destDirectory + ": " + e.Message);
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
                    if (cfg.verboseMode)
                    {
                        cfg.addToLog(destDir + ": created");
                    }
                    
                }
                catch (DirectoryNotFoundException ex)
                {
                    this.cfg.addToLog("ERR:" + destDir + ": " + ex.Message);
                    continue;
                }
                catch (NotSupportedException ex)
                {
                    this.cfg.addToLog("ERR:" + destDir + ": " + ex.Message);
                    continue;
                }
                catch (PathTooLongException ex)
                {
                    this.cfg.addToLog("ERR:" + destDir + ": " + ex.Message);
                    continue;
                }
                catch (UnauthorizedAccessException ex)
                {
                    this.cfg.addToLog("ERR:" + destDir + ": " + ex.Message);
                    continue;
                }
                catch (ArgumentException ex)
                {
                    this.cfg.addToLog("ERR:" + destDir + ": " + ex.Message);
                    continue;
                }
                catch (IOException ex)
                {
                    this.cfg.addToLog("ERR:" + destDir + ": " + ex.Message);
                    continue;
                }
            }

                      
            // copy all files
            var sourceDestFiles = this.sourceFileList.Zip(this.destFileList, (a, b) => new { sourceFile = a, destFile = b });
            int counter = 0;
            int cntAll = sourceDestFiles.Count();
            if(cntAll == 0)
            {
                cntAll = 1;
            }
            int onePercent  = (int) ((float) cntAll / 100.0);            
            var watch = System.Diagnostics.Stopwatch.StartNew();
            cfg.WriteProgressBar("copying files: ", "", 0);

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
                                        if (cfg.verboseMode)
                                        {
                                            cfg.addToLog(x.sourceFile + ": creating hardlink to " + oldFn);
                                        }
                                    }
                                    else
                                    {
                                        cfg.addToLog("ERR:" + x.sourceFile + ": unable to create hardlink, copying instead");
                                    }
                                } else
                                {
                                    if (cfg.verboseMode)
                                    {
                                        cfg.addToLog(x.sourceFile + ": creating hardlink to " + oldFn);
                                    }
                                }
                            }
                        }
                    }
                    if (!cfg.dryRun)
                    {
                        if (makeCopy)
                        {
                            File.Copy(x.sourceFile, x.destFile.driveTimeFilename);
                            try
                            {
                                FileInfo fi_dest = new FileInfo(x.destFile.driveTimeFilename);
                                bool wasReadOnly = false;
                                if (fi_dest.IsReadOnly)
                                {
                                    fi_dest.IsReadOnly = false;
                                    wasReadOnly = true;
                                }
                                fi_dest.CreationTime = fi_source.CreationTime;
                                fi_dest.LastWriteTime = fi_source.LastWriteTime;
                                fi_dest.LastAccessTime = fi_source.LastAccessTime;
                                if(wasReadOnly)
                                {
                                    fi_dest.IsReadOnly = true;
                                }
                            }
                            catch(UnauthorizedAccessException unauthEx)
                            {
                                cfg.addToLog("ERR: " + x.sourceFile + ": couldn't change access and creation time of destiation file");
                            }
                            if (cfg.verboseMode)
                            {
                                cfg.addToLog(x.sourceFile + ": file copied");
                            }
                        }
                    } else
                    {
                        if (cfg.verboseMode)
                        {
                            cfg.addToLog(x.sourceFile + ": file copied");
                        }
                    }
                }
                catch (IOException copyError)
                {
                    if(cfg.useVss)
                    {
                        int errorCode = Marshal.GetHRForException(copyError) & ((1 << 16) - 1);
                        if (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION)
                        {
                            cfg.addToLog("ERR:" + x.sourceFile + ": sharing or lock violation, trying later w/ shadow copy");
                            Tuple<String, DestinationFile> tpl = new Tuple<String, DestinationFile>(x.sourceFile, x.destFile);
                            tryWithVSS.Add(tpl);
                        } else
                        {
                            cfg.addToLog("ERR:" + x.sourceFile + ": " + copyError.Message);
                        }

                    } else
                    {
                        cfg.addToLog("ERR:" + x.sourceFile + ": " + copyError.Message);
                    }
                }
                counter += 1;
                if (!cfg.verboseMode)
                {
                    if(onePercent == 0)
                    {
                        onePercent += 1;
                    }
                    if (counter % onePercent == 0)
                    {
                        int percent = (int)((((double)counter / (double)cntAll)) * 100.0);
                        watch.Stop();
                        long passedSecs = watch.ElapsedMilliseconds / 1000;
                        double ratio = ((double) cntAll - (double)(counter + 1)) / (double) (counter + 1);
                        long remSecsTotal = (long)(passedSecs * ratio);
                        if (remSecsTotal > 10)
                        {
                            int remSeconds = (int) (remSecsTotal % 60);
                            int remMinutes = ((int) (remSecsTotal / 60)) % 60;
                            int remHours = (int) (remSecsTotal / (60 * 60));                                    
                            string strETR = String.Format(" ETR: {0:00}:{1:00}:{2:00}", remHours, remMinutes, remSeconds);
                            cfg.WriteProgressBar("copying files:", strETR, percent, true);
                        } else
                        {
                            cfg.WriteProgressBar("copying files:", "", percent, true);
                        }
                        watch.Start();
                    }
                }
            }
            if(!cfg.verboseMode)
            {
                cfg.WriteProgressBar("copying files:", "                         ", 100, true);
                cfg.addToLog("");
            }
            cfg.addToLog("copying files: finished.");
            watch.Stop();
            TimeSpan ts = watch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("Time Required: " + elapsedTime);

            if (this.cfg.useVss && tryWithVSS.Count > 0)
            {
                using (VssBackup vss = new VssBackup())
                {
                    vss.Setup(Path.GetPathRoot(tryWithVSS[0].Item1));
                    
                    foreach(Tuple<String, DestinationFile> x in tryWithVSS)
                    {
                        string snap_path = vss.GetSnapshotPath(x.Item1);
                        try
                        {
                            Alphaleonis.Win32.Filesystem.File.Copy(snap_path, x.Item2.driveTimeFilename);
                            cfg.addToLog(x.Item1 + ": copied snapshot via VSS");
                        } catch (ArgumentException e)
                        {
                            cfg.addToLog("ERR: " + x.Item1 + ": "+e.Message);
                        } catch (DirectoryNotFoundException e)
                        {
                            cfg.addToLog("ERR: " + x.Item1 + ": " + e.Message);
                        } catch (FileNotFoundException e)
                        {
                            cfg.addToLog("ERR: " + x.Item1 + ": " + e.Message);
                        } catch (IOException e)
                        {
                            cfg.addToLog("ERR: " + x.Item1 + ": " + e.Message);
                        } catch (NotSupportedException e)
                        {
                            cfg.addToLog("ERR: " + x.Item1 + ": " + e.Message);
                        } catch (UnauthorizedAccessException e)
                        {
                            cfg.addToLog("ERR: " + x.Item1 + ": " + e.Message);
                        }
                    }
                }
            }
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

    }
}
