using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yahb { 

    enum ArgType
    {
        SimpleSwitch = 1,   // switch starting with "/"
        Compound = 2,       // 'switch:argument' pair starting with '/'
        Complex = 3         // 'switch:argument{;argument}' pair with multiple args starting with '/'
    }
    class Program
    {
        static void Main(string[] args)
        {
            ParseCmdLine parse = new ParseCmdLine();

            Config cfg = new Config();

            try
            {
                // Create an array of all possible command-line parameters
                // and how to parse them.
                object[,] mySwitches = new object[2, 14] {
                 {"s", "lev", "if", 
                    "id", "novss", "xf", "xd", 
                      "list", "v", "log", "+log", 
                        "tee", "?", "files"},
                 {ArgType.SimpleSwitch, ArgType.Compound, ArgType.Compound,
                   ArgType.Compound, ArgType.SimpleSwitch, ArgType.Compound, ArgType.Compound,
                     ArgType.SimpleSwitch, ArgType.SimpleSwitch, ArgType.Compound, ArgType.Compound,
                       ArgType.SimpleSwitch, ArgType.SimpleSwitch, ArgType.Complex}};

                for (int counter = 0; counter < args.Length; counter++)
                {
                    if (counter == 0)
                    {
                        if (!args[counter].StartsWith("/"))
                        {
                            // should be the input directory. Check validity later
                            cfg.sourceDirectory = args[counter];
                        } else
                        {
                            // cannot be - even when omitting the source directory
                            // we need at least the destination directory in arg[0]
                            throw (new ArgumentException(
                                      "Cmd-Line parameter error: " + args[counter] +
                                      "is a parameter, but a directory is expected.")
                                );
                        }
                    }

                    if (counter == 1)
                    {
                        if (!args[counter].StartsWith("/"))
                        {
                            cfg.destinationDirectory = args[counter];
                        } else
                        {
                            // this is a parameter, i.e. /foo. If so, there is only the chance,
                            // that the first passed argument was the destination
                            // directory, and we have the /if or /id option
                            // then change first arg to destination dir
                            cfg.destinationDirectory = args[0];
                            cfg.sourceDirectory = "";
                        }
                    }

                    if (args[counter].StartsWith("/"))
                    {
                        args[counter] = args[counter].TrimStart(new char[1] { '/' });

                        // Search for the correct ArgType and parse argument according to
                        // this ArgType.
                        for (int index = 0; index <= mySwitches.GetUpperBound(1); index++)
                        {
                            string theSwitch = "";
                            string theArgument = "";
                            string[] theArguments = new string[0];

                            if (args[counter].StartsWith((string)mySwitches[0, index]))
                            {
                                // Parse each argument into switch:arg1;arg2…
                                switch ((ArgType)mySwitches[1, index])
                                {
                                    case ArgType.SimpleSwitch:
                                        theSwitch = parse.ParseSwitch(args[counter]);
                                        break;

                                    case ArgType.Compound:
                                        parse.ParseSwitchColonArg(args[counter], out theSwitch,
                                                                  out theArgument);
                                        break;

                                    case ArgType.Complex:
                                        parse.ParseSwitchColonArgs(args[counter], out theSwitch,
                                                                   out theArguments);
                                        break;

                                    default:
                                        throw (new ArgumentException(
                                          "Cmd-Line parameter error: ArgType enumeration " +
                                          mySwitches[1, index].ToString() +
                                          " not recognized."));
                                }

                                // Implement functionality to handle each parsed
                                // command-line parameter.
                                switch ((string)mySwitches[0, index])
                                {
                                    case "s":
                                        cfg.copySubDirectories = true;
                                        break;

                                    case "lev":
                                        cfg.maxLvel = System.Int32.Parse(theArgument);
                                        break;

                                    case "id":
                                        cfg.fnInputDirectories = theArgument;
                                        break;

                                    case "novss":
                                        cfg.useVss = false;
                                        break;

                                    case "xf":
                                        foreach (string excludePattern in theArguments)
                                        {
                                            cfg.filePatternsToIgnore.Add(excludePattern);
                                        }
                                        break;

                                    case "xd":
                                        foreach (string excludeDir in theArguments)
                                        {
                                            cfg.directoriesToIgnore.Add(excludeDir);
                                        }
                                        break;

                                    case "files":
                                        foreach (string fileEnding in theArguments)
                                        {
                                            cfg.fileEndings.Add(fileEnding);
                                        }
                                        break;

                                    case "list":
                                        cfg.dryRun = true;
                                        break;

                                    case "v":
                                        cfg.verboseMode = true;
                                        break;

                                    case "log":
                                        cfg.fnLogFile = theArgument;
                                        cfg.overwriteLogFile = true;
                                        break;

                                    case "+log":
                                        cfg.fnLogFile = theArgument;
                                        cfg.overwriteLogFile = false;
                                        break;

                                    case "tee":
                                        cfg.writeToLogAndConsole = true;
                                        break;

                                    case "?":
                                        cfg.showHelp = true;
                                        break;

                                    default:
                                        throw (new ArgumentException(
                                           "Cmd-Line parameter error: Switch " +
                                           mySwitches[0, index].ToString() +
                                           " not recognized."));
                                }
                            }
                        }
                    }
                }
                // check sanity of parsed configuration
                // throws ArgumentException on inconsistencies
                cfg.checkConsistency();
            }
            catch (ArgumentException ae)
            {
                parse.DisplayErrorMsg(ae.Message);
                return;
            }
            catch (Exception e)
            {
                // Handle other exceptions here…
            }

            // start copy operations
            Console.WriteLine(cfg.ToString());
            CopyModule cm = new CopyModule(cfg);
            List<String> dirs = cm.createDirectoryList();
            cm.createFileList(dirs);
            cm.doCopy();

        }
    }
}
