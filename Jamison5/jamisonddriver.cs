/********************************************************************
*** NAME       : Daniel Jamison								      ***
*** CLASS      : CSc 354										  ***
*** ASSIGNMENT : 1											      ***
*** DUE DATE   : 09-19-12										  ***
*** INSTRUCTOR : GAMRADT										  ***
*********************************************************************
*** DESCRIPTION : This program creates, searches and displays a   ***
***     symbols table. This is the main class.                    ***
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CustomIO;
using LinkerLoaderStuff;


namespace Driver
{
    /// <summary>
    /// Holds various options for the program runtime environment
    /// </summary>
    public struct EnvironmentVars
    {
        public static bool DEBUGMODE = false;
        public static bool SCRIPTMODE = false;
    }

    /// <summary>
    /// Runs the program contains main
    /// </summary>
    class DriverMain
    {
        public static LinkerLoader linkerLoader = null;

        static void Main(string[] args)
        {
            //Console.SetWindowSize(80, 20);

            IO.logFile = IO.CreateFile(ref IO.LOGFILE);

            string[] executionArgs = ProcessCommandLineArgs(args);

            linkerLoader = new LinkerLoader(executionArgs);

            Exit();
        }

        /// <summary>
        /// Sets environment and options for the program and creates a new list of input parameters for the rest of the code
        /// </summary>
        /// <param name="args">string[]</param>
        /// <returns>string[]</returns>
        public static string[] ProcessCommandLineArgs(string[] args)
        {
            IO.Output("Command line args: " + args, true);
            string executionArgs = "";
            string originalArgs = "";
            foreach (string str in args)
            {
                switch (str.ToLower())
                {
                    case "-debug":
                        EnvironmentVars.DEBUGMODE = true;
                        IO.Output("Debug mode active", true);
                        
                        break;
                    case "-script":
                        EnvironmentVars.SCRIPTMODE = true;
                        IO.Output("Script mode active", true);
                        break;
                    
                    default:
                        executionArgs += str + " ";
                        break;
                }
                originalArgs += str + " ";
            }

            IO.Output("Command line args: " + originalArgs, true);
            IO.Output("Remaining execution args: " + executionArgs, true);
            IO.Output("\n\n", true);
            return executionArgs.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Called to exit the program. Displays additional output depending on the situation, and preforms cleanup.
        /// </summary>
        /// <param name="forcedExit">bool</param>
        public static void Exit(bool forcedExit = false)
        {
            if (forcedExit)
            {
                IO.Output("Press any key to exit...", false, true);
            }
            else
            {
                if (IO.errorCount == 0)
                {
                    IO.Output("\nProgram compleated sucessfully. Press any key to exit...");
                }
                else if (IO.errorCount == 1)
                {
                    IO.Output("\nProgram compleated. Error logged in \"" + IO.LOGFILE + "\". Press any key to exit...");
                }
                else //if (errorCount > 1)
                {
                    IO.Output("\nProgram compleated. Errors logged in \"" + IO.LOGFILE + "\". Press any key to exit...");
                }
            }

            // Closes the streams
            IO.Exit();

            if (!Driver.EnvironmentVars.SCRIPTMODE )
            {
                Console.ReadKey(true); // Allows for press any key to exit
            }

            Environment.Exit(0);
        }
    }
}
