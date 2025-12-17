using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

        // Import the SetThreadExecutionState function from kernel32.dll
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(uint esFlags);

        // Flags for SetThreadExecutionState
        const uint ES_CONTINUOUS = 0x80000000;
        const uint ES_SYSTEM_REQUIRED = 0x00000001;

        static void Main(string[] args)
        {
            // prevent windows from sleeping
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED);

            ParseCmdLine parse = new ParseCmdLine();

            Config cfg = new Config();

            try
            {
                // Create an array of all possible command-line parameters
                // and how to parse them.
                object[,] mySwitches = new object[2, 17] {
                 {"src", "dest", "id", "r", "copyall" , "pause",
                        "xf", "xd", "list", "verbose",
                        "log", "+log", "tee", "?",
                        "files", "vss", "help"},
                 {ArgType.Complex, ArgType.Compound, ArgType.Compound, ArgType.SimpleSwitch, ArgType.SimpleSwitch, ArgType.SimpleSwitch,
                        ArgType.Complex, ArgType.Complex, ArgType.SimpleSwitch, ArgType.SimpleSwitch,
                        ArgType.Compound, ArgType.Compound, ArgType.SimpleSwitch, ArgType.SimpleSwitch,
                        ArgType.Complex, ArgType.SimpleSwitch, ArgType.SimpleSwitch}};

                for (int counter = 0; counter < args.Length; counter++)
                {

                    // if user requested help, show help immediately and exit
                    if (args[counter].Equals("/help") || args[counter].Equals("/?"))
                    {
                        // make sure that Windows can go to standby after program ends
                        SetThreadExecutionState(ES_CONTINUOUS);
                        // display help 
                        parse.DisplayVerboseHelp();                        
                        // and exit
                        System.Environment.Exit(0);
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
                                    case "r":
                                        cfg.copySubDirectories = true;
                                        break;

                                    case "lev":
                                        cfg.maxLvel = System.Int32.Parse(theArgument);
                                        break;

                                    case "vss":
                                        cfg.useVss = true;
                                        break;

                                    case "src":
                                        foreach (string inputDir in theArguments)
                                        {
                                            cfg.inputDirectories.Add(inputDir);
                                        }
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

                                    case "verbose":
                                        cfg.verboseMode = true;
                                        break;

                                    case "log":
                                        cfg.fnLogFile = theArgument;
                                        cfg.overwriteLogFile = true;
                                        break;

                                    case "dest":
                                        cfg.destinationDirectory = theArgument;
                                        break;

                                    case "+log":
                                        cfg.fnLogFile = theArgument;
                                        cfg.overwriteLogFile = false;
                                        break;

                                    case "pause":
                                        cfg.pauseAtEnd = true;
                                        break;

                                    case "tee":
                                        cfg.writeToLogAndConsole = true;
                                        break;

                                    case "copyall":
                                        cfg.copyAll = true;
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
                // make sure that Windows can go to standby after program ends
                SetThreadExecutionState(ES_CONTINUOUS);
                return;
            }            

            // start copy operations
            if(cfg.dryRun)
            {
                cfg.addToLog("SIMULATION RUN: LIST FILES ONLY, DON'T COPY ANYTHING");
            }

            //Console.WriteLine(cfg.ToString());
            CopyModule cm = new CopyModule(cfg);
            List<String> dirs = cm.createDirectoryList();
            cm.createFileList(dirs);
            cm.doCopy();
            if(cfg.pauseAtEnd)
            {
                Console.WriteLine("Press ENTER to exit.");
                Console.ReadLine();
            }

            // make sure that Windows can go to standby after program ends
            SetThreadExecutionState(ES_CONTINUOUS);


        }
    }
}
