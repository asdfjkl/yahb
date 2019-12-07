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
            if (Process.GetCurrentProcess().ProcessName.Trim().Length > 0)
            {
                Console.WriteLine(Process.GetCurrentProcess().ProcessName);
            }
            else
            {
                Console.WriteLine("Product Name: " + version.ProductName);
            }

            Console.WriteLine("Version " + version.FileVersion);
            Console.WriteLine("Copyright " + version.LegalCopyright);
            Console.WriteLine("TradeMarks " + version.LegalTrademarks);

            DisplayHelp();
        }
        public virtual void DisplayHelp()
        {
            Console.WriteLine("See help for command-line usage.");
        }
    }
}
