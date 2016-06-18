using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyArms;

namespace UnitTestValTree
{
    [TestClass]
    public class MainTests
    {
        [TestMethod]
        public void TestParse()
        {
            var v = new ValTree();
            string path = Directory.GetCurrentDirectory();
            var worked = v.Parse(path + "/TestData.txt");

            Assert.IsTrue(worked, "ValTree failed to parse script");
        }

        [TestMethod]
        public void TestGetValidChild()
        {
            var v = new ValTree();
            string path = Directory.GetCurrentDirectory();
            var worked = v.Parse(path + "/TestData.txt");

            Assert.IsTrue(worked, "ValTree failed to parse script");
            var h = v.GetChild("g-is-long");
            Assert.IsNotNull(h, "Failed to get child");
        }

        [TestMethod]
        public void TestGetValidViaQueryChild()
        {
            var v = new ValTree();
            string path = Directory.GetCurrentDirectory();
            var worked = v.Parse(path + "/TestData.txt");

            Assert.IsTrue(worked, "ValTree failed to parse script");
            var h = v.Query("key1.key2.key3");
            Assert.IsNotNull(h, "Failed to get child");
        }

        [TestMethod]
        public void TestGetInValidChild()
        {
            var v = new ValTree();
            string path = Directory.GetCurrentDirectory();
            var worked = v.Parse(path + "/TestData.txt");

            Assert.IsTrue(worked, "ValTree failed to parse script");
            var h = v.GetChild("asfdasdasd");
            Assert.IsNull(h, "Got valid child when should not have");
        }

        [TestMethod]
        public void TestAddViaValTreeChild()
        {
            var v = new ValTree();
            string path = Directory.GetCurrentDirectory();
            var worked = v.Parse(path + "/TestData.txt");

            Assert.IsTrue(worked, "ValTree failed to parse script");
            v.AddChild(new ValTree("l", "90,90"));

            var h = v.GetChild("l");
            Assert.IsNotNull(h, "Failed to add child");
        }

        [TestMethod]
        public void TestAddViaQueryChild()
        {
            var v = new ValTree();
            string path = Directory.GetCurrentDirectory();
            var worked = v.Parse(path + "/TestData.txt");

            Assert.IsTrue(worked, "ValTree failed to parse script");
            v.AddChild("l.m.n.o.p", "q");

            var h = v.GetChild("l");
            Assert.IsNotNull(h, "Failed to add child");
        }


        [TestMethod]
        public void TestGetValueString()
        {
            var v = new ValTree();
            string path = Directory.GetCurrentDirectory();
            var worked = v.Parse(path + "/TestData.txt");

            Assert.IsTrue(worked, "ValTree failed to parse script");

            var h = v.GetChild("key1");
            Assert.IsNotNull(h, "Failed to add child");

            Assert.AreEqual(h.GetValue(), "val1 ");
        }

        [TestMethod]
        public void TestGetValueInt()
        {
            var v = new ValTree();
            string path = Directory.GetCurrentDirectory();
            var worked = v.Parse(path + "/TestData.txt");

            Assert.IsTrue(worked, "ValTree failed to parse script");

            var h = v.Query("key1.key2.key3");
            Assert.IsNotNull(h, "Failed to add child");

            Assert.AreEqual(h.GetInt(), 9);
        }

        [TestMethod]
        public void TestGetValueFloat()
        {
            var v = new ValTree();
            string path = Directory.GetCurrentDirectory();
            var worked = v.Parse(path + "/TestData.txt");

            Assert.IsTrue(worked, "ValTree failed to parse script");

            var h = v.Query("key1.key2.key3.key4");
            Assert.IsNotNull(h, "Failed to add child");

            Assert.AreEqual(h.GetFloat(), 17.8);
        }
    }
}
