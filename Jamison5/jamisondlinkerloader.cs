/********************************************************************
*** NAME       : Daniel Jamison								      ***
*** CLASS      : CSc 354										  ***
*** ASSIGNMENT : 5											      ***
*** DUE DATE   : 12-05-12										  ***
*** INSTRUCTOR : GAMRADT										  ***
*********************************************************************
*** DESCRIPTION : This namespace loads .obj files and links them  ***
***       as well as creating and displaying the coresponding     ***
***       memory map.                                              ***
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CustomIO;
using System.Text.RegularExpressions;
using BinarySearchTree;

namespace LinkerLoaderStuff
{
    /// <summary>
    /// Holds all the information about the memory map
    /// </summary>
    public static class MemoryMap
    {
        public static int startingAddress = 0;
        public static int controlSectionAddress = 0;
        public static int executionAddress = 0;
        public static int addressOffset = 0;
        public static StringBuilder memory = new StringBuilder("");


        /// <summary>
        /// Initializes all memory bits to ?
        /// </summary>
        public static void InitializeMemory()
        {
            memory = new StringBuilder("");
            for (int i = controlSectionAddress - startingAddress; i > 0; i--)
            {
                memory.Append("??");
            }
        }

        /// <summary>
        /// Assigns data to the memory block at the specified locaton for the next number of bytes specified by length
        /// </summary>
        /// <param name="start">int</param>
        /// <param name="data">string</param>
        /// <param name="length">int</param>
        public static void AssignMemory(int start, string data, int length)
        {
            int startPos = start - startingAddress;
            for (int i = 0; i < length * 2; i++)
            {
                memory[startPos * 2 + i] = data[i];
            }
        }

        /// <summary>
        /// Updates the value at the specified location by perfoming a calculation
        /// </summary>
        /// <param name="start">int</param>
        /// <param name="data">string</param>
        /// <param name="length">int</param>
        /// <param name="operation">char</param>
        public static void ModifyMemory(int start, string data, int length, char operation)
        {
            if (Driver.EnvironmentVars.DEBUGMODE)
            {
                DisplayMemoryMap();
            }
            
            int startPos = start - startingAddress;
            int lengthOffset = 6 - length;
            string curValueStr = "";

            for (int i = 0; i < length; i++)
            {
                curValueStr += memory[startPos * 2 + i + lengthOffset];
            }

            int finalVal = 0;
            if (operation == '+')
            {
                finalVal = Convert.ToInt32(curValueStr, 16) + Convert.ToInt32(data, 16);
            }
            else // if (operation == '-')
            {
                finalVal = Convert.ToInt32(curValueStr, 16) - Convert.ToInt32(data, 16);
            }
            string finalValStr = finalVal.ToString("X" + length);

            finalValStr = finalValStr.Remove(0, finalValStr.Length - length);

            for (int i = 0; i < length; i++)
            {
                memory[startPos * 2 + i + lengthOffset] = finalValStr[i];
            }

            IO.Output("M-record Orig: " + curValueStr + " Updated with: " + data + " Final: " + finalValStr, true);

            if (Driver.EnvironmentVars.DEBUGMODE)
            {
                DisplayMemoryMap();
                Console.ReadKey(true);
                IO.lineNum = 1;
                Console.Clear();
            }
        }
        
        /// <summary>
        /// Displays the entire memory map to the screen with a header
        /// </summary>
        public static void DisplayMemoryMap()
        {
            string header = String.Format("{0,-8}{1,3}{2,3}{3,3}{4,3}{5,3}{6,3}{7,3}{8,3}{9,3}{10,3}{11,3}{12,3}{13,3}{14,3}{15,3}{16,3}", "", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F");
            IO.Output(header);
            IO.writeFile[0].stream.WriteLine(header);

            int tempAddress = startingAddress;
            string tempMem = memory.ToString();
            bool end = false;

            // Breaks the memory string into its corresponding memory location
            while (!end)
            {
                string[] parts = new string[16];
                for (int i = 0; i < 32; i++)
                {
                    if (tempMem.Length > 0)
                    {
                        parts[i / 2] += tempMem[0];
                        tempMem = tempMem.Remove(0, 1);
                    }
                    else
                    {
                        parts[i / 2] += " ";
                        end = true;
                    }
                }

                string line = String.Format("{0,-8}{1,3}{2,3}{3,3}{4,3}{5,3}{6,3}{7,3}{8,3}{9,3}{10,3}{11,3}{12,3}{13,3}{14,3}{15,3}{16,3}", tempAddress.ToString("X5"), parts[0], parts[1], parts[2], parts[3], parts[4], parts[5], parts[6], parts[7], parts[8], parts[9], parts[10], parts[11], parts[12], parts[13], parts[14], parts[15]);
                IO.Output(line);
                IO.writeFile[0].stream.WriteLine(line);
                tempAddress = tempAddress + 16;
            }
        }
    }

    public class LinkerLoader
    {
        public BST symbolTable= new BST();

        public string fileName = "";
        public string controlSection = "";
        public string controlSectionLength = "";
        public string controlSectionStartingAddress = "";
        public string controlSectionFirstExecutable = "";
        public bool firstLine = true;
        public bool hFlag = false;
        public bool eFlag = false;

        /// <summary>
        /// Default constructor loads up the files to begin the linking loading process
        /// </summary>
        /// <param name="args">string[]</param>
        public LinkerLoader(string[] args)
        {
            // Set the starting address for the program
            MemoryMap.startingAddress = Convert.ToInt32("03590", 16);
            //MemoryMap.startingAddress = Convert.ToInt32("01000", 16);

            // sets up the objcode files that will be read 
            IO.OpenFilesForReading(args, "objCode");
            IO.OpenFilesForWriting(new string[] { "..\\..\\LAYOUT.DAT" }, "layout");

            // Make sure that files were loaded
            if (IO.readFile == null)
            {
                IO.Output("ERROR: No files have been specified. Program will now exit.");
                return;
            }
            else
            {
                // Begin processing code from the first file entered
                Pass1();

                MemoryMap.InitializeMemory();

                Pass2();
                
                IO.Output("\n", true);
                symbolTable.DisplayByCreationOrder();
                IO.Output("\n");
                MemoryMap.DisplayMemoryMap();

                IO.Output("\nExecution begins at address: " + MemoryMap.executionAddress.ToString("X5"));
                IO.writeFile[0].stream.WriteLine("\nExecution begins at address: " + MemoryMap.executionAddress.ToString("X5"));
            }
        }
        
        /// <summary>
        /// Processes the .obj files starting with the first one declared
        /// </summary>
        public void Pass1()
        {
            MemoryMap.controlSectionAddress = MemoryMap.startingAddress;

            using (IO.readFile[0].stream)
            {
                foreach(ReadFile ioFile in IO.readFile)
                {
                    fileName = "";
                    fileName = ioFile.fileName;
                    controlSection = "";
                    controlSectionLength = "";
                    controlSectionStartingAddress = "";
                    controlSectionFirstExecutable = "";
                    firstLine = true;
                    hFlag = false;
                    eFlag = false;

                    string line;

                    while ((line = ioFile.stream.ReadLine()) != null)
                    {
                        // Displays all lines to the screen
                        IO.Output(line, true);
                        line = line.Trim();

                        if (line.Length > 0)
                        {
                            switch (line[0])
                            {
                                case 'H':
                                case 'h':
                                    ProcessHeader(line);
                                    break;

                                case 'D':
                                case 'd':
                                    ProcessDefinition(line);
                                    break;

                                case 'E':
                                case 'e':
                                    ProcessEnd(line);
                                    break;
                            }
                        }
                    }

                    if (!hFlag)
                    {
                        IO.Output("ERROR: No H-record processed in \"" + fileName + "\"");
                    }
                    else if (!eFlag)
                    {
                        IO.Output("ERROR: No E-record processed in \"" + fileName + "\"");
                    }
                }
            }
        }

        /// <summary>
        /// Processes the .obj files starting with the first one declared
        /// </summary>
        public void Pass2()
        {
            MemoryMap.controlSectionAddress = MemoryMap.startingAddress;
            MemoryMap.executionAddress = MemoryMap.startingAddress;
            bool initialFile = true;

            using (IO.readFile[0].stream)
            {
                foreach (ReadFile ioFile in IO.readFile)
                {
                    ioFile.stream.Close();
                    ioFile.stream = new StreamReader(ioFile.fileName);
                }

                foreach (ReadFile ioFile in IO.readFile)
                {
                    fileName = "";
                    fileName = ioFile.fileName;
                    controlSection = "";
                    controlSectionLength = "";
                    controlSectionStartingAddress = "";
                    controlSectionFirstExecutable = "";
                    firstLine = true;
                    hFlag = false;
                    eFlag = false;

                    string line;

                    while ((line = ioFile.stream.ReadLine()) != null)
                    {
                        // Displays all lines to the screen
                        IO.Output(line, true);
                        line = line.Trim();

                        if (line.Length > 0)
                        {
                            switch (line[0])
                            {
                                case 'H':
                                case 'h':
                                    ProcessHeader(line, false);
                                    break;

                                case 'R':
                                case 'r':
                                    ProcessReference(line);
                                    break;

                                case 'T':
                                case 't':
                                    ProcessText(line);
                                    break;

                                case 'M':
                                case 'm':
                                    ProcessModification(line);
                                    break;

                                case 'E':
                                case 'e':
                                    ProcessEnd(line);
                                    break;
                            }
                        }
                    }

                    if (!hFlag)
                    {
                        IO.Output("ERROR: No H-record processed in \"" + fileName + "\"");
                    }
                    else if (!eFlag)
                    {
                        IO.Output("ERROR: No E-record processed in \"" + fileName + "\"");
                    }

                    if (initialFile)
                    {
                        initialFile = false;
                        MemoryMap.executionAddress += Convert.ToInt32(controlSectionFirstExecutable, 16);
                    }
                }
            }
        }

        /// <summary>
        /// Performs operations related to the header record
        /// </summary>
        /// <param name="line">string</param>
        public void ProcessHeader(string line, bool insert = true)
        {
            // Format H^Section ^StAddr^Length
            if (hFlag)
            {
                IO.Output("ERROR: More than one H-record found in \"" + fileName + "\"");
            }
            else if (!firstLine)
            {
                IO.Output("ERROR: H-record was not the first line in \"" + fileName + "\"");
            }
            else if (eFlag)
            {
                IO.Output("ERROR: E-Record defined before H-record in \"" + fileName + "\"");
            }
            else if (line.Length != 21)
            {
                IO.Output("ERROR: Header record length is invalid in \"" + fileName + "\"");
            }
            else
            {
                for (int i = 1; i <= 8; i++)
                {
                    controlSection += line[i];
                }
                for (int i = 9; i <= 14; i++)
                {
                    controlSectionStartingAddress += line[i];
                }
                for (int i = 15; i <= 20; i++)
                {
                    controlSectionLength += line[i];
                }
                hFlag = true;
                firstLine = false;
                if (insert)
                {
                    symbolTable.Insert(new BinarySearchTree.NodeData(controlSection, controlSection, 0, MemoryMap.controlSectionAddress, Convert.ToInt32(controlSectionLength, 16)));
                }
            }
        }

        /// <summary>
        /// Performs operations related to a definition record
        /// </summary>
        /// <param name="line">string</param>
        public void ProcessDefinition(string line)
        {
            // Format D^Symbol  ^Addres^REPEAT
            if (!hFlag)
            {
                IO.Output("ERROR: H-record not defined before D-record in \"" + fileName + "\"");
            }
            else if (eFlag)
            {
                IO.Output("ERROR: E-Record defined before D-record in \"" + fileName + "\"");
            }
            else if (line.Length < 15 || line.Length > 71 || ((line.Length - 1) % 14) != 0)
            {
                IO.Output("ERROR: D-record length is invalid in \"" + fileName + "\"");
            }
            else
            {
                line = line.Remove(0,1);
                while(line != null && line.Length != 0)
                {
                    string symbol = "";
                    string address = "";
                    for (int i = 0; i < 8; i++)
                    {
                        symbol += line[i];
                    }
                    for (int i = 9; i < 14; i++)
                    {
                        address += line[i];
                    }

                    symbolTable.Insert(new BinarySearchTree.NodeData(controlSection, symbol, Convert.ToInt32(address, 16), Convert.ToInt32(address, 16) + MemoryMap.controlSectionAddress, 0));
                    line = line.Remove(0,14);
                }
                firstLine = false;
            }
        }

        /// <summary>
        /// Performs operations related to a reference record
        /// </summary>
        /// <param name="line">string</param>
        public void ProcessReference(string line)
        {
            // Format R^Symbol  ^REPEAT
            if (!hFlag)
            {
                IO.Output("ERROR: H-record not defined before R-record in \"" + fileName + "\"");
            }
            else if (eFlag)
            {
                IO.Output("ERROR: E-Record defined before R-record in \"" + fileName + "\"");
            }
            else if (line.Length < 2 || line.Length > 72)
            {
                IO.Output("ERROR: R-record length is invalid in \"" + fileName + "\"");
            }
            else
            {
                line = line.Remove(0, 1);
                while (line != null && line.Length != 0)
                {
                    int endNum = 0;
                    string symbol = "";
                    if (line.Length >= 8)
                    {
                        endNum = 8;
                    }
                    else
                    {
                        endNum = line.Length;
                    }
                    for (int i = 0; i < endNum; i++)
                    {
                        symbol += line[i];
                    }

                    if (symbolTable.Search(symbol) == null)
                    {
                        IO.Output("ERROR: R-record bad reference in \"" + fileName + "\"");
                    }
                    else
                    {
                        firstLine = false;
                    }
                    line = line.Remove(0, endNum);
                }
            }
        }

        /// <summary>
        /// Performs operations related to a text record
        /// </summary>
        /// <param name="line">string</param>
        public void ProcessText(string line)
        {
            // Format T^StAddr^Ln^object code
            if (!hFlag)
            {
                IO.Output("ERROR: H-record not defined before T-record in \"" + fileName + "\"");
            }
            else if (eFlag)
            {
                IO.Output("ERROR: E-Record defined before T-record in \"" + fileName + "\"");
            }
            else if (line.Length < 9 || line.Length > 69)
            {
                IO.Output("ERROR: T-record length is invalid in \"" + fileName + "\"");
            }
            else
            {
                string tStartAddress = "";
                string tLength = "";

                for (int i = 1; i <= 6; i++)
                {
                    tStartAddress += line[i];
                }
                for (int i = 7; i <= 8; i++)
                {
                    tLength += line[i];
                }

                line = line.Remove(0, 9);
                if (line.Length == Convert.ToInt32(tLength, 16) * 2)
                {
                    //tCode = line;
                    MemoryMap.AssignMemory(Convert.ToInt32(tStartAddress, 16) + MemoryMap.controlSectionAddress, line, Convert.ToInt32(tLength, 16));

                    firstLine = false;
                }
                else
                {
                    IO.Output("ERROR: Bad T-record checksum in \"" + fileName + "\"");
                    //MemoryMap.AssignMemory(Convert.ToInt32(tStartAddress, 16)  + MemoryMap.controlSectionAddress, line, Convert.ToInt32(tLength, 16));
                }
            }
        }

        /// <summary>
        /// Performs operations related to a modification record
        /// </summary>
        /// <param name="line">string</param>
        public void ProcessModification(string line)
        {
            // Format M^StAddr^Ln^+/-^Symbol
            if (!hFlag)
            {
                IO.Output("ERROR: H-record not defined before M-record in \"" + fileName + "\"");
            }
            else if (eFlag)
            {
                IO.Output("ERROR: E-Record defined before M-record in \"" + fileName + "\"");
            }
            else if (line.Length < 11 || line.Length > 18)
            {
                IO.Output("ERROR: M-record length is invalid in \"" + fileName + "\"");
            }
            else
            {
                string mStartAddress = "";
                string mLength = "";
                char mFlag = '+';

                for (int i = 1; i <= 6; i++)
                {
                    mStartAddress += line[i];
                }
                for (int i = 7; i <= 8; i++)
                {
                    mLength += line[i];
                }
                mFlag = line[9];
                line = line.Remove(0, 10);
                //symbol = line;
                if (string.Compare(line, controlSection.Trim(), System.StringComparison.Ordinal) == 0)
                {
                    MemoryMap.ModifyMemory(Convert.ToInt32(mStartAddress, 16) + MemoryMap.controlSectionAddress, MemoryMap.controlSectionAddress.ToString("X"), Convert.ToInt32(mLength, 16), mFlag);
                }
                else
                {
                    BinarySearchTree.NodeData tempData = symbolTable.Search(line);
                    if (tempData != null)
                    {
                        MemoryMap.ModifyMemory(Convert.ToInt32(mStartAddress, 16) + MemoryMap.controlSectionAddress, tempData.address.ToString("X"), Convert.ToInt32(mLength, 16), mFlag);
                    }
                    else
                    {
                        IO.Output("ERROR: M-record references undefined symbol in \"" + fileName + "\"");
                    }
                }
                
                firstLine = false;
            }
        }

        /// <summary>
        /// Performs operations related to the end record
        /// </summary>
        /// <param name="line">string</param>
        public void ProcessEnd(string line)
        {
            // Format E^Addres
            if (!hFlag)
            {
                IO.Output("ERROR: E-record defined before H-record in \"" + fileName + "\"");
            }
            else if (eFlag)
            {
                IO.Output("ERROR: Multiple E-records defined in \"" + fileName + "\"");
            }
            else if (line.Length == 7 || line.Length == 1)
            {
                if (line.Length == 1)
                {
                    controlSectionFirstExecutable = "0";
                }
                else
                {
                    controlSectionFirstExecutable = line.Remove(0, 1);
                }
                
                MemoryMap.controlSectionAddress += Convert.ToInt32(controlSectionLength, 16);
                eFlag = true;
                firstLine = false;
            }
            else
            {
                IO.Output("ERROR: E-record length is invalid in \"" + fileName + "\"");
            }
        }
    }
}
