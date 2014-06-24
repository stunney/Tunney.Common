using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Castle.Core.Resource;
using Tunney.Common.IoC;
using Tunney.Common.IoC.Resource;

namespace Tunney.Common.Tests.IoC
{
    [TestClass]
    public class CustomDictionaryTypeConverterTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Tunney.Common.IoC.ILogger logger = new Log4NetLogger("Tunney.DefaultLogger");
            IResource resource = new CastleWindsorEmbeddedResourceResource("Tunney.Common.Tests", "Tunney.Common.Tests.IoC.IoCConfig.xml");

            CastleWindsorContainer container = new CastleWindsorContainer(resource, logger);

            Tunney.Common.DictionaryWithInt32KeyAndListValue<string> o = container.Resolve<Tunney.Common.DictionaryWithInt32KeyAndListValue<string>>("myObjectID");

            Assert.AreNotEqual(0, o.Count);
        }
    }
}