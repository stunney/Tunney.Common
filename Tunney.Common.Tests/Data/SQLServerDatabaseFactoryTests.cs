using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using System.Data.SqlClient;
using Tunney.Common.Data;

namespace Tunney.InMemoryDatabase.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SQLServerDatabaseFactoryTests
    {
        public SQLServerDatabaseFactoryTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestObjectSerialization()
        {
            SQLServerDatabaseFactory factory = new SQLServerDatabaseFactory(TEST_SERVER_NAME, new MockDDLRunner());

            object c2 = Deserialize(Serialize(factory));

            object c3 = Deserialize(Serialize(c2));

            Assert.IsNotNull(c3);
        }

        protected const string TEST_SERVER_NAME = @"W2K8-SPS2-15";

        //[TestMethod]
        //public void TestConnectionDisposed()
        //{
        //    SQLServerDatabaseFactory factory = new SQLServerDatabaseFactory(TEST_SERVER_NAME, new MockDDLRunner());
        //    IDbConnection conn = factory.Create();
        //    conn.Dispose();
        //}

        private byte[] Serialize(object _obj)
        {
            //using (DeflateStream ms = new DeflateStream(new MemoryStream(), CompressionMode.Compress))
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, _obj);
                return ((MemoryStream)ms).ToArray();
            }
        }

        private object Deserialize(byte[] _bytes)
        {
            using (MemoryStream ms = new MemoryStream(_bytes))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                ms.Position = 0;
                return formatter.Deserialize(ms);
            }
        }

        [Serializable]
        private class MockDDLRunner : IDDLRunner
        {
            public MockDDLRunner()
            {
            }

            #region ISerializable Members

            protected MockDDLRunner(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {
            }

            public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {                
            }

            #endregion

            #region IDDLRunner Members

            public virtual void CreateEntities(IDbConnection _openConnection)
            {                
            }

            public virtual void DropEntities(IDbConnection _openConnection)
            {                
            }

            public virtual IDictionary<string, string> TableNames
            {
                get { return new Dictionary<string, string>(); }
            }

            #endregion
        }
    }
}