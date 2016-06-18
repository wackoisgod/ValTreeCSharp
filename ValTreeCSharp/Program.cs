using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using TinyArms;

namespace ValTreeCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var testValTree = new ValTree();
            string path = Directory.GetCurrentDirectory();
            var worked = testValTree.Parse(path + "/TestData/PuzzleExample.txt");

            var parent = testValTree.GetChild("properties");


            testValTree.Log();

            foreach (ValTree x in testValTree)
            {
                var y = x;
                y.GetFirstChild();
            }


        }
    }
}
