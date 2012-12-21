/********************************************************************
*** NAME       : Daniel Jamison								      ***
*** CLASS      : CSc 354										  ***
*** ASSIGNMENT : 1											      ***
*** DUE DATE   : 09-19-12										  ***
*** INSTRUCTOR : GAMRADT										  ***
*********************************************************************
*** DESCRIPTION : This class maintains the binary search tree     ***
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomIO;

namespace BinarySearchTree
{
    /// <summary>
    /// Holds the values for each node in the tree, and has display built in
    /// </summary>
    public class NodeData
    {
        // NodeData variables
        public string controlSection;
        public string symbol;
        public int offset;
        public int address;
        public int length;
        public bool symbolErrorFlag;
        public bool sectionErrorFlag;

        /// <summary>
        /// Constructor must be fully defined to create a NodeData class
        /// </summary>
        /// <param name="longSymbol">string</param>
        /// <param name="value">int</param>
        /// <param name="rflag">bool</param>
        /// <param name="iflag">bool</param>
        /// <param name="mflag">bool</param>
        public NodeData (string controlSection, string symbol, int offset, int address, int length, bool symbolErrorFlag = false, bool sectionErrorFlag = false)
        {
            this.controlSection = controlSection;
            this.symbol = symbol;
            this.offset = offset;
            this.address = address;
            this.length = length;
            this.symbolErrorFlag = symbolErrorFlag;
            this.sectionErrorFlag = sectionErrorFlag;
        }

        /// <summary>
        /// Display a formated string that shows the column names of the data
        /// </summary>
        public static void DisplayHeader()
        {
            string header = String.Format("{0,-10}{1,10}{2,8}{3,8}{4,8}{5,7}{6,8}", "CSection", "Symbol", "Address", "MAddr", "Length", "SError", "CSError");
            IO.Output(header);
        }

        /// <summary>
        /// Display the passed in set of data in formated columns
        /// </summary>
        /// <param name="data">NodeData</param>
        public static void DisplayData(NodeData data)
        {
            string output = String.Format("{0,-10}{1,10}{2,8}{3,8}{4,8}{5,7}{6,8}", data.controlSection, data.symbol, data.offset.ToString("X5"), data.address.ToString("X5"), data.length.ToString("X"), data.symbolErrorFlag, data.sectionErrorFlag);
            IO.Output(output);
        }
    }

    /// <summary>
    /// Holds the structure of each node
    /// </summary>
    public class Node
    {
        // Node variables
        public NodeData data;
        public Node left, right;
        public static int currentItem = 0;
        public int creationNum;

        /// <summary>
        /// Constructor must have a defined NodeData
        /// </summary>
        /// <param name="data">NodeData</param>
        public Node (NodeData data)
        {
            this.data = data;
            left = null;
            right = null;
            creationNum = ++currentItem;
        }
    }

    /// <summary>
    /// BST class maintains a binary search tree
    /// </summary>
    public class BST
    {
        public bool duplicateControlSection = false;

        // Define the root of the tree
        private Node root;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BST()
        {
            root = null;
        }

        /// <summary>
        /// Constructor to initialize the first node
        /// </summary>
        /// <param name="firstNodeData">NodeData</param>
        public BST(NodeData firstNodeData)
        {
            root = null;
            Insert(firstNodeData);
        }

        /// <summary>
        /// Search for the first 8 characters from the passed in string, flag controls wether to mark the mflag or not
        /// </summary>
        /// <param name="longSymbol">string</param>
        /// <param name="markIfDuplicate">bool</param>
        /// <returns>NodeData</returns>
        public NodeData Search(string longSymbol, bool markIfDuplicate = false)
        {
            // ensure that only the first 8 chars are looked at in the search
            string symbol = "";
            if (longSymbol.Length <= 8)
            {
                symbol = longSymbol;
            }
            else
            {
                for (int i = 0; i < 8 && i < longSymbol.Length; i++)
                {
                    symbol += longSymbol[i];
                }
            }
            Node curNode = root;
            // Make sure the tree is not empty
            if (curNode != null)
            {
                while (curNode != null)
                {
                    // The node has been found
                    if (string.Compare(symbol.Trim(), curNode.data.symbol.Trim(), System.StringComparison.Ordinal) == 0)
                    {
                        if (markIfDuplicate)
                        {
                            curNode.data.symbolErrorFlag = true;
                        }
                        return curNode.data;
                    }
                    // The node your looking for is less than this nodes key move left
                    else if (string.Compare(symbol, curNode.data.symbol, System.StringComparison.Ordinal) < 0)
                    {
                        curNode = curNode.left;
                    }
                    // The node your looking for is greater than this nodes key move left
                    else //if (string.Compare(symbol, node.data.symbol) > 0)
                    {
                        curNode = curNode.right;
                    }
                }
            }
            
            // Return null if the node was not found
            return null;
        }

        /// <summary>
        /// Inserts a new node into the tree in alphabetical order according to the ascii chart
        /// </summary>
        /// <param name="data">bool</param>
        /// <returns>NodeData</returns>
        public bool Insert(NodeData data)
        {
            // Check to ensure that the node does not already exist
            if (Search(data.symbol, true) == null)
            {
                Node newNode = new Node(data);
                // If the tree is empty make it the root node
                if (root == null)
                {
                    root = newNode;
                }
                else
                {
                    Node curNode = root;
                    Node lastNode = null;
                    bool isLeft = false;
                    while (curNode != null)
                    {
                        // The node your looking for is less than this nodes key move left
                        if (string.Compare(data.symbol, curNode.data.symbol, System.StringComparison.Ordinal) < 0)
                        {
                            lastNode = curNode;
                            isLeft = true;
                            curNode = curNode.left;
                        }
                        // The node your looking for is greater than this nodes key move left
                        else //if (string.Compare(symbol, node.data.symbol) > 0)
                        {
                            lastNode = curNode;
                            isLeft = false;
                            curNode = curNode.right;
                        }
                    }
                    // place the node in the correct location
                    if (isLeft)
                    {
                        lastNode.left = newNode;
                    }
                    else
                    {
                        lastNode.right = newNode;
                    }
                }
            }
            else
            {
                IO.Output("ERROR: Cannot insert symbol \"" + data.symbol + "\" previously defined ");
                //return false;
            }
            // Return null if the node was not found
            return true;
        }

        /// <summary>
        /// Begins the display of all nodes in the tree by passing the recursive function the root
        /// </summary>
        public void InorderTraversal(bool output = true, string section = null, bool markDuplicate = false, int creationNum = 0)
        {
            duplicateControlSection = false;
            if (output && creationNum == 0)
            {
                NodeData.DisplayHeader();
            }
            InorderTraverse(root, output, section, markDuplicate, creationNum); 
        }

        /// <summary>
        /// Recursivly calls itself to navigate through an inorder traversal of the tree
        /// </summary>
        /// <param name="curNode">Node</param>
        private void InorderTraverse(Node curNode, bool output = true, string section = null, bool markDuplicate = false, int creationNum = 0)
        {
            if (curNode == null)
            {
                return;
            }
            if (curNode.left != null)
            {
                InorderTraverse(curNode.left, output, section, markDuplicate, creationNum);
            }
            if (section != null)
            {
                if (string.Compare(section, curNode.data.symbol, System.StringComparison.Ordinal) == 0)
                {
                    duplicateControlSection = true;
                    if (markDuplicate)
                    {
                        curNode.data.sectionErrorFlag = true;
                    }
                }
            }
            if (output)
            {
                if (creationNum == 0)
                {
                    NodeData.DisplayData(curNode.data);
                }
                else if (curNode.creationNum == creationNum)
                {
                    NodeData.DisplayData(curNode.data);
                }
            }
            if (curNode.right != null)
            {
                InorderTraverse(curNode.right, output, section, markDuplicate, creationNum);
            }
            return;
        }

        /// <summary>
        /// Displays all the entries in the order they were created
        /// </summary>
        public void DisplayByCreationOrder()
        {
            NodeData.DisplayHeader();

            for (int i = 1; i <= Node.currentItem; i++)
            {
                InorderTraversal(true, null, false, i);
            }
        }
    }
}
