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
            var v = new ValTree();
            string path = Directory.GetCurrentDirectory();
            var worked = v.Parse(path + "/TestData.txt");

            var h = v.GetChild("g-is-long").GetChild("h");
            Console.WriteLine("The value of 'g-is-long.h' is '" + h.GetValue() + "'");

            var key41 = v.Query("key1.key2.key3.key4-1");
            Console.WriteLine("The value of 'key1.key2.key3.key4-1' is '" + key41.GetValue() + "'");

            v.AddChild(new ValTree("l", "90,90"));
            Console.WriteLine("After adding child 'l', new ValTree looks like this:");
            v.Log();

            v.AddChild("l.m.n.o.p", "q");
            Console.WriteLine("After adding tree 'l.m.n.o.p', new ValTree looks like this:");
            v.Log();

            v.Save(path + "/TestData-modified.txt");

        }
    }
}
