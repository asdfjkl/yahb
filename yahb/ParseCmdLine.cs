using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yahb
{
    class ParseCmdLine
    {
        // All args are delimited by tab or space.
        // All double-quotes are removed except when escaped '\"'.
        // All single-quotes are left untouched.

        public ParseCmdLine() { }

        public virtual string ParseSwitch(string arg)
        {
            arg = arg.TrimStart(new char[] { '/' });

            if (arg.IndexOf(':') >= 0)
            {
                throw (new ArgumentException("Command-Line parameter error: switch " +
                      arg + " must not be followed by one or more arguments.", arg));
            }
            return (arg);
        }

        public virtual void ParseSwitchColonArg(string arg, out string outSwitch,
                                                out string outArgument)
        {
            outSwitch = "";
            outArgument = "";

            try
            {
                // This is a switch or switch/argument pair.
                arg = arg.TrimStart(new char[] { '/' });

                if (arg.IndexOf(':') >= 0)
                {
                    outSwitch = arg.Substring(0, arg.IndexOf(':'));
                    outArgument = arg.Substring(arg.IndexOf(':') + 1);

                    if (outArgument.Trim().Length <= 0)
                    {
                        throw (new ArgumentException(
                           "Command-Line parameter error: switch " +
                           arg +
                           " must be followed by one or more arguments.", arg));
                    }
                }
                else
                {
                    throw (new ArgumentException(
                            "Command-Line parameter error: argument " +
                            arg +
                            " must be in the form of a 'switch:argument}' pair.",
                            arg));
                }
            }
            catch (ArgumentException ae)
            {
                // Re-throw the exception to be handled in the calling method.
                throw;
            }
            catch (Exception e)
            {
                // Wrap an ArgumentException around the exception thrown.
                throw (new ArgumentException("General command-Line parameter error",
                                             arg, e));
            }
        }

        public virtual void ParseSwitchColonArgs(string arg, out string outSwitch,
                                                 out string[] outArguments)
        {
            outSwitch = "";
            outArguments = null;

            try
            {
                // This is a switch or switch/argument pair.
                arg = arg.TrimStart(new char[] { '/' });

                if (arg.IndexOf(':') >= 0)

                {
                    outSwitch = arg.Substring(0, arg.IndexOf(':'));
                    string Arguments = arg.Substring(arg.IndexOf(':') + 1);

                    if (Arguments.Trim().Length <= 0)
                    {
                        throw (new ArgumentException(
                                "Command-Line parameter error: switch " +
                                arg +
                                " must be followed by one or more arguments.", arg));
                    }

                    outArguments = Arguments.Split(new char[1] { ';' });
                }
                else
                {
                    throw (new ArgumentException(
                       "Command-Line parameter error: argument " +
                       arg +
                       " must be in the form of a 'switch:argument{;argument}' pair.",
                       arg));
                }
            }
            catch (Exception e)
            {
                // Wrap an ArgumentException around the exception thrown.
                throw;
            }
        }

        public virtual void DisplayErrorMsg()
        {
            DisplayErrorMsg("");
        }

        public virtual void DisplayErrorMsg(string msg)
        {
            Console.WriteLine
                ("An error occurred while processing the command-line arguments:");
            Console.WriteLine(msg);
            Console.WriteLine();

            FileVersionInfo version =
                       Process.GetCurrentProcess().MainModule.FileVersionInfo;
            //if (Process.GetCurrentProcess().ProcessName.Trim().Length > 0)
            //{
            //    Console.WriteLine(Process.GetCurrentProcess().ProcessName);
            //}
            //else
            //{
            Console.WriteLine("YAHB: " + version.ProductName);
            //}

            Console.WriteLine("Version " + version.FileVersion);
            Console.WriteLine("Copyright " + version.LegalCopyright);

            DisplayHelp();
        }
        public virtual void DisplayHelp()
        {
            Console.WriteLine("See help (/? oder /help) for command-line usage.");
        }

        public void DisplayVerboseHelp()
        {
            Console.WriteLine("YAHB (Yet Another Hardlink-based Backup-Tool)");
            FileVersionInfo version =
                       Process.GetCurrentProcess().MainModule.FileVersionInfo;
            Console.WriteLine("Version " + version.FileVersion);
            Console.WriteLine("Copyright (c) 2019 - 2021 Dominik Klein");
            Console.WriteLine("");
            Console.WriteLine("     Syntax:: yahb.exe <source-dir> <target-dir> [<options>]");
            Console.WriteLine("");
            Console.WriteLine(" source-dir:: source directory (i.e. C:\\MyFiles)");
            Console.WriteLine(" target-dir:: target directory (i.e. D:\\Backups)");
            Console.WriteLine("");
            Console.WriteLine("TYPICAL EXAMPLE:");
            Console.WriteLine("");
            Console.WriteLine(" yahb c:\\MyFiles d:\\Backup /s /xf:*.tmp");
            Console.WriteLine("");
            Console.WriteLine("will copy all files and the directory structure from c:\\MyFiles");
            Console.WriteLine("to d:\\Backup\\YYYYMMDDHHMM, including all subdirectories. Yahb will");
            Console.WriteLine("also look for previous backups of c:\\MyFiles in d:\\Backup, and if");
            Console.WriteLine("a file has not changed, it will create a hardlink to that location.");
            Console.WriteLine("Moreover, all files with ending .tmp will be skipped.");
            Console.WriteLine("");
            Console.WriteLine("OPTIONS");
            Console.WriteLine("");
            //Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("  /copyall                 :: copy ALL files. Otherwise the following directory");
            Console.WriteLine("                              patterns and file types are excluded:");
            Console.WriteLine("");
            Console.WriteLine("                              DIRECTORIES:");
            Console.WriteLine("                              - 'System Volume Information'");
            Console.WriteLine("                              - 'AppData\\Local\\Temp'");
            Console.WriteLine("                              - 'AppData\\Local\\Microsoft\\Windows\\INetCache'");
            Console.WriteLine("                              - 'C:\\Windows'");
            Console.WriteLine("                              - '$Recycle.Bin'");
            Console.WriteLine("");
            Console.WriteLine("                              FILES AND PLACEHOLDERS:");
            Console.WriteLine("                              - hiberfil.sys");
            Console.WriteLine("                              - pagefile.sys");
            Console.WriteLine("                              - swapfile.sys");
            Console.WriteLine("                              - *.~");
            Console.WriteLine("                              - *.temp");
            Console.WriteLine("");
            Console.WriteLine("  /files:PAT1;PAT2;...     :: copy only files that match the supplied");
            Console.WriteLine("                              file patterns (like *.exe)");
            Console.WriteLine("");
            Console.WriteLine("  /help                    :: display this help screen");
            Console.WriteLine("");
            Console.WriteLine("  /id:FILENAME             :: supply a list of Input Directories to copy which");
            Console.WriteLine("                              are stored line by line in a textfile FILENAME.");
            Console.WriteLine("                              If this options is used, <source-dir> can be");
            Console.WriteLine("                              omitted. If both <source-dir> and /id:FILENAME");
            Console.WriteLine("                              are present, all directories will be copied.");
            Console.WriteLine("                              NOTE that if /s is provided, it will be ");
            Console.WriteLine("                              applied to the list of input directories, and");
            Console.WriteLine("                              will also be applied to <source-dir>.");
            Console.WriteLine("");
            Console.WriteLine("  /list                    :: do not copy anything, just list all files");
            Console.WriteLine("");
            Console.WriteLine("  /log:FILENAME            :: write all output (log) to a textfile FILNAME.");
            Console.WriteLine("                              If FILENAME exists, it will be overwritten");
            Console.WriteLine("");
            Console.WriteLine("  /+log:FILENAME           :: same as /log:FILENAME, but always append, i.e.");
            Console.WriteLine("                              do not not overwrite FILENAME if it exists.");
            Console.WriteLine("");
            Console.WriteLine("  /pause                   :: after finishing, wait for the user to press");
            Console.WriteLine("                              ENTER before closing the program. This");
            Console.WriteLine("                              prevents a command - prompt from vanishing");
            Console.WriteLine("                              after finishing if run e.g. by Windows' RUNAS");
            Console.WriteLine("                              command");
            Console.WriteLine("");
            Console.WriteLine("  /s                       :: also copy all SUBDIRECTORIES of <source-dir>");
            Console.WriteLine("");
            Console.WriteLine("  /tee                     :: even if /log:FILENAME or /+log:FILENAME is");
            Console.WriteLine("                              chosen, still write everything additionally");
            Console.WriteLine("                              to console output.");
            Console.WriteLine("");
            Console.WriteLine("  /verbose                 :: by default, only the progress and errors ");
            Console.WriteLine("                              are output to the console/log. In verbose");
            Console.WriteLine("                              mode, all created files and directories");
            Console.WriteLine("                              are listed - note that for large copy");
            Console.WriteLine("                              operations, this frequent output to console");
            Console.WriteLine("                              will slow down the overal operation");
            Console.WriteLine("");
            Console.WriteLine("  /vss                     :: If a file is currently in use, and cannot be");
            Console.WriteLine("                              accessed, try to still copy that file by using");
            Console.WriteLine("                              Windows' Volume Shadow Copy Service.");
            Console.WriteLine("                              YOU NEED TO RUN YAHB WITH ELEVATED (ADMIN)");
            Console.WriteLine("                              RIGHTS FOR THIS TO WORK.");
            Console.WriteLine("");
            Console.WriteLine("  /xd:DIR1;DIR2;...        :: eXclude directories dir1, dir2, and so forth.");
            Console.WriteLine("                              I.e. if DIR is provided here, any (full)" );
            Console.WriteLine("                              directory path that contains DIR is skipped");
            Console.WriteLine("");
            Console.WriteLine("  /xf:PAT1;PAT2;...        :: eXclude files with filename PAT1, PAT2 and so");
            Console.WriteLine("                              forth. PAT can also be a file pattern like *.tmp");
            Console.WriteLine("");
            Console.WriteLine("  /?                       :: display this help screen");
        }
    }
}
