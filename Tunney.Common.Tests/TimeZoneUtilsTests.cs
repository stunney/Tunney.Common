using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tunney.Common.Tests
{
    [TestClass]
    public class TimeZoneUtilsTests
    {
        //[TestMethod]
        //public void TestConvertToDateTimeOffset()
        //{
        //    DateTime now = DateTime.Now;
        //    DateTime utcNow = now.ToUniversalTime();

        //    TimeSpan offset = now - utcNow;

        //    DateTimeOffset dto = TimeZoneUtils.ConvertToDateTimeOffset(now, TimeZoneInfo.Local); //Local is hardcoded so this test can work anywhere.  Do I really care about PST and EST here?  NO!

        //    Assert.AreEqual(offset, dto.Offset);
        //}

        [TestMethod]
        public void TestGetUtcOffset()
        {
            DateTime inDST = new DateTime(2010, 6, 6, 9, 0, 0, DateTimeKind.Local);
            DateTime outDST = new DateTime(2010, 1, 1, 9, 0, 0, DateTimeKind.Local);

            TimeZoneInfo EST = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneUtils.TIMEZONE_EST);

            TimeSpan inDSTOffset = EST.GetUtcOffset(inDST);
            TimeSpan outDSTOffset = EST.GetUtcOffset(outDST);

            Assert.AreNotEqual(inDSTOffset, outDSTOffset);
        }
    }
}