/********************************************************************
*** NAME       : Daniel Jamison								      ***
*** CLASS      : CSc 354										  ***
*** ASSIGNMENT : 1											      ***
*** DUE DATE   : 09-19-12										  ***
*** INSTRUCTOR : GAMRADT										  ***
*********************************************************************
*** DESCRIPTION : This class controls the program's IO features   ***
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CustomIO
{
    /// <summary>
    /// Holds information about the the files being read from
    /// </summary>
    public class ReadFile
    {
        public StreamReader stream = null;
        public string fileName = "";
        public string tag = "";

        public ReadFile()
        {
            // Nothing
        }
        
        public ReadFile(string fileName, StreamReader stream, string tag = "")
        {
            this.fileName = fileName;
            this.stream = stream;
            this.tag = tag;
        }
    }

    /// <summary>
    /// Holds information about the the files being written to
    /// </summary>
    public class WriteFile
    {
        public StreamWriter stream = null;
        public string fileName = "";
        public string tag = "";

        public WriteFile()
        {
            // Nothing
        }

        public WriteFile(string fileName, StreamWriter stream, string tag = "")
        {
            this.fileName = fileName;
            this.stream = stream;
            this.tag = tag;
        }
    }

    /// <summary>
    /// Controls most IO features in the program, utilizes screen control error logging, and debug features
    /// </summary>
    class IO
    {
        public static readonly int windowWidth = Console.WindowWidth; // Set the number of characters on a line, max is typically 80, Console.WindowWidth for auto
        public static readonly int windowHeight = Console.WindowHeight;//Console.WindowHeight; // Set the number of lines on the screen, typically 20, Console.WindowHeight for auto

        public static int lineNum = 1; // Placeholder for the current line on the screen
        public static int errorCount = 0; // Placeholder for how many errors were encounterd in running the program

        public static StreamWriter logFile = null;
        public static string LOGFILE = "LOG.txt";

        public static ReadFile[] readFile;
        public static WriteFile[] writeFile;

        /// <summary>
        /// Read a file name from the user, test if it exist or stay in the loop
        /// </summary>
        /// <returns>string</returns>
        public static string PromptUserForFileName(bool isReadFile = true, bool createOrErase = true)
        {
            string fileName = "";
            Output("Please enter the name of the file to open, type QUIT when finished.");
            while (true)
            {
                fileName = Input();
                if (fileName.Equals("QUIT"))
                {
                    return fileName;
                }
                else if (isReadFile)
                {
                    if (CheckForFile(ref fileName))
                    {
                        break;
                    }
                    else
                    {
                        Output("That file does not exist. Please try again.");
                    }
                }
                else
                {
                    if (CheckForFile(ref fileName, createOrErase))
                    {
                        break;
                    }
                    else
                    {
                        Output("That file does not exist. Please try again.");
                    }
                }
            }
            return fileName;
        }

        /// <summary>
        /// Creates a file, the requested name is passed in but can be modified by ref if the name is unavailable
        /// </summary>
        /// <param name="fileName">ref string</param>
        /// <returns>StreamWriter</returns>
        public static StreamWriter CreateFile(ref string fileName, bool createOrErase = false)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                    return new StreamWriter(fileName, !createOrErase);
                }
                catch
                {
                    Random num = new Random();
                    fileName = fileName + num.Next() + ".txt";
                    return new StreamWriter(fileName, !createOrErase);
                }
            }
            else
            {
                //File.Create(fileName);
                return new StreamWriter(fileName, !createOrErase);
            }
        }
 
        /// <summary>
        /// Searches for the specified filename within the current directory as well as the two directories above it. If the flag is set if the file is found it is erased, and if it is not found it is created
        /// </summary>
        /// <param name="fileName">ref string</param>
        /// <param name="createOrErase">bool</param>
        /// <returns>bool</returns>
        public static bool CheckForFile(ref string fileName, bool createOrErase = false)
        {
            StreamWriter sw = null;
            if (File.Exists(fileName))
            {
                if (createOrErase)
                {
                    sw = CreateFile(ref fileName);
                }
            }
            else if (File.Exists("..\\" + fileName))
            {
                fileName = "..\\" + fileName;
                if (createOrErase)
                {
                    sw = CreateFile(ref fileName);
                }
            }
            else if (File.Exists("..\\..\\" + fileName))
            {
                fileName = "..\\..\\" + fileName;
                if (createOrErase)
                {
                    sw = CreateFile(ref fileName);
                }
            }
            else if (createOrErase)
            {
                sw = CreateFile(ref fileName);
            }
            else
            {
                return false;
            }

            if (sw != null)
            {
                sw.Close();
            }
            return true;
        }

        /// <summary>
        /// Opens files either from an array of file names or prompting the user repeadtedly
        /// </summary>
        /// <param name="args">string[]</param>
        /// <param name="tag">string</param>
        /// <param name="createOrErase">bool</param>
        public static void OpenFilesForReading(string[] args = null, string tag = "", bool createOrErase = false)
        {
            if (args.Length == 0)
            {
                // Get file names from user one at a time and open streams
                while (true)
                {
                    string input = PromptUserForFileName();
                    if (input.Equals("QUIT"))
                    {
                        break;
                    }
                    else
                    {
                        int position = 0;
                        if (readFile != null)
                        {
                            position = readFile.Length;
                        }

                        Array.Resize(ref readFile, position + 1);
                        readFile[position] = new ReadFile(input, new StreamReader(input), tag);
                    }
                }
            }
            else
            {
                foreach (string str in args)
                {
                    string input = str;

                    if (CheckForFile(ref input))
                    {
                        int position = 0;
                        if (readFile != null)
                        {
                            position = readFile.Length;
                        }

                        Array.Resize(ref readFile, position + 1);
                        readFile[position] = new ReadFile(input, new StreamReader(input), tag);
                    }
                    else
                    {
                        Output("Error: Could not open file \"" + str + "\"");
                    }
                }
            }
        }

        /// <summary>
        /// Opens files either from an array of file names or prompting the user repeadtedly
        /// </summary>
        /// <param name="args">string[]</param>
        /// <param name="tag">string</param>
        /// <param name="createOrErase">bool</param>
        public static void OpenFilesForWriting(string[] args = null, string tag = "", bool createOrErase = true)
        {
            if (args.Length == 0)
            {
                // Get file names from user one at a time and open streams
                while (true)
                {
                    string input = PromptUserForFileName(false, createOrErase);
                    if (input.Equals("QUIT"))
                    {
                        break;
                    }
                    else
                    {
                        int position = 0;
                        if (writeFile != null)
                        {
                            position = writeFile.Length;
                        }

                        Array.Resize(ref writeFile, position + 1);
                        writeFile[position] = new WriteFile(input, CreateFile(ref input, createOrErase), tag);
                    }
                }
            }
            else
            {
                foreach (string str in args)
                {
                    string input = str;
                    if (CheckForFile(ref input, createOrErase))
                    {
                        int position = 0;
                        if (writeFile != null)
                        {
                            position = writeFile.Length;
                        }

                        Array.Resize(ref writeFile, position + 1);
                        writeFile[position] = new WriteFile(input, CreateFile(ref input, createOrErase), tag);
                    }
                    else
                    {
                        Output("Error: Could not open file \"" + str + "\"");
                    }
                }
            }
        }

        /// <summary>
        /// Steps through all the open input files streams counting the occurence of each tag
        /// </summary>
        /// <param name="tag">string</param>
        /// <returns>int</returns>
        public static int CountInputTagOccurences(string tag)
        {
            int count = 0;
            foreach (ReadFile ioFile in readFile)
            {
                if (ioFile.tag.Equals(tag))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Steps through all the open output files streams counting the occurence of each tag
        /// </summary>
        /// <param name="tag">string</param>
        /// <returns>int</returns>
        public static int CountOutputTagOccurences(string tag)
        {
            int count = 0;
            foreach (WriteFile ioFile in writeFile)
            {
                if (ioFile.tag.Equals(tag))
                {
                    count++;
                }
            }
            return count;
        }
 
        /// <summary>
        /// handles all input in the program to keep track of the lines for screen control
        /// </summary>
        /// <returns>string</returns>
        public static string Input()
        {
            ScreenControl();

            string inputStr = Console.ReadLine();

            if (Driver.EnvironmentVars.DEBUGMODE)
            {
                logFile.WriteLine(inputStr);
            }

            lineNum = Console.CursorTop + 1;

            ScreenControl();

            return inputStr;
        }

        /// <summary>
        /// Controlled output for the program to utilize screen control
        /// </summary>
        /// <param name="input">object</param>
        /// <param name="isDebugOnly">bool</param>
        /// <param name="isError">bool</param>
        public static void Output(object passedMsg, bool isDebugOnly = false, bool isError = false)
        {
            // Allows for the function to accept any datatype to be ouputed as a string
            string msg = passedMsg.ToString();

            // If the string begins with the error title it is automatically flaged as an error
            if (msg.StartsWith("ERROR:"))
            {
                isError = true;
            }

            int end = msg.Length;
            int current = 0;
            while (current != end)
            {
                bool fullLine = false;
                int remaining = end - current;
                if (remaining >= windowWidth)
                {
                    remaining = windowWidth;
                    fullLine = true;
                }

                string line = "";
                for (int i = 0; i < remaining; i++)
                {
                    if (msg[current] == '\n')
                    {
                        current++;
                        break;
                    }
                    line += msg[current];
                    current++;
                }

                DisplayOneLine(line, isDebugOnly, isError);
                if (fullLine)
                {
                    if (!Driver.EnvironmentVars.SCRIPTMODE)
                    {
                        if (current == end)
                        {
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                        }
                    }
                }
            }

            Console.ResetColor();
        }

        /// <summary>
        /// Prints up to one full line to the screen as well as to the log file if the debug flag was set
        /// </summary>
        /// <param name="msg">string</param>
        /// <param name="isDebugOnly">bool</param>
        /// <param name="isError">bool</param>
        private static void DisplayOneLine(string msg, bool isDebugOnly, bool isError)
        {
            if (!Driver.EnvironmentVars.SCRIPTMODE)
            {
                ScreenControl();
            }

            if (isError)
            {
                errorCount++;
                Console.ForegroundColor = ConsoleColor.Red;
            }

            if (!isDebugOnly)
            {
                if (!Driver.EnvironmentVars.SCRIPTMODE)
                {
                    Console.WriteLine(msg);
                }
                lineNum++;
                if (Driver.EnvironmentVars.DEBUGMODE || Driver.EnvironmentVars.SCRIPTMODE)
                {
                    logFile.WriteLine(msg);
                }
            }
            else if (Driver.EnvironmentVars.DEBUGMODE)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                if (!Driver.EnvironmentVars.SCRIPTMODE)
                {
                    Console.WriteLine(msg);
                }
                lineNum++;
                logFile.WriteLine(msg);
            }

            Console.ResetColor();
        }

        /// <summary>
        /// Ensures that screen control is maintained
        /// </summary>
        private static void ScreenControl()
        {
            if (lineNum >= windowHeight)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("## More: Press any Key to continue ##");
                Console.ReadKey(true);
                Console.ResetColor();
                Console.Clear();
                lineNum = 1;
            }
        }

        /// <summary>
        /// Closes all open streams used in the class
        /// </summary>
        public static void Exit()
        {
            if (logFile != null)
            {
                logFile.Close();
            }

            if (readFile != null)
            {
                foreach (ReadFile ioFile in readFile)
                {
                    if (ioFile.stream != null)
                    {
                        ioFile.stream.Close();
                    }
                }
            }

            if (writeFile != null)
            {
                foreach (WriteFile ioFile in writeFile)
                {
                    if (ioFile.stream != null)
                    {
                        ioFile.stream.Close();
                    }
                }
            }
        }
    }
}
