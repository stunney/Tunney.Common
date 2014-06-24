using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Tunney.Common.IoC;
using Castle.Core.Resource;
using Tunney.Common.IoC.Resource;

namespace Tunney.IoCContainer.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class CastleWindsorContainerTests
    {
        public CastleWindsorContainerTests()
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
            Tunney.Common.IoC.ILogger logger = new Log4NetLogger("Tunney.DefaultLogger");
            IResource resource = new CastleWindsorEmbeddedResourceResource("Tunney.Common.Tests", "Tunney.Common.Tests.IoC.IoCConfig.xml");

            CastleWindsorContainer container = new CastleWindsorContainer(resource, logger);

            IIoCContainer resolvedContainer = container.Resolve<IIoCContainer>(@"IoCContainer");

            object c2 = Deserialize(Serialize(container));

            Assert.IsNotNull(c2);

            IIoCContainer c3 = ((IIoCContainer)c2).Resolve<IIoCContainer>(@"IoCContainer");

            Assert.IsNotNull(c3);
        }

        [TestMethod]
        public void TestObjectSerializationMultipleTimes()
        {
            Tunney.Common.IoC.ILogger logger = new Log4NetLogger("Tunney.DefaultLogger");
            IResource resource = new CastleWindsorEmbeddedResourceResource("Tunney.Common.Tests", "Tunney.Common.Tests.IoC.IoCConfig.xml");

            CastleWindsorContainer container = new CastleWindsorContainer(resource, logger);

            IIoCContainer resolvedContainer = container.Resolve<IIoCContainer>(@"IoCContainer");

            object c2 = Deserialize(Serialize(container));

            Assert.IsNotNull(c2);

            IIoCContainer c3 = ((IIoCContainer)c2).Resolve<IIoCContainer>(@"IoCContainer");

            Assert.IsNotNull(c3);

            object c4 = Deserialize(Serialize(c2));

            Assert.IsNotNull(c4);

            IIoCContainer c5 = ((IIoCContainer)c4).Resolve<IIoCContainer>(@"IoCContainer");

            Assert.IsNotNull(c5);
        }

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
    }
}
