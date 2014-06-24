using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Tunney.Common.Data;

namespace Tunney.Common.Tests.Data
{
    [TestClass]
    public class SQLiteDDLGeneratorTests
    {
        private const string SQL_SELECT_RAWCLICKS = @"SELECT TOP 10000 [CLKid]
      ,[ADid]
      ,[BIDid]
      ,[BIDamount]
      ,CAST(SWITCHOFFSET([CLKtime], 0) AS DATETIME) AS [CLKtimeUTC]
      ,DATEPART(TZoffset, [CLKtime]) AS [CLKtimeOffset]
      ,[CLKreferrer]
      ,[CLKsessionid]
      ,[CLKrank]
      ,[CLKservername]
      ,[CLKdestinationUrl]
      ,[ClkSitid]
      ,[ClkGUID]
      ,[ClkBid]
      ,[ClkFeedType]
      ,[ClkServer]
      ,[ServerID]
      ,[ClkCurrency]
      ,[ClkTerm]
      ,[ClkTypeTag]
      ,[VisitorType]
      ,[PageID]
      ,[PageViewID]
      ,[CLKagent]
      ,[CLKredirectUrl]
      ,[ClkAdSourceID]
      ,[PartnerTagID]
      ,[ImpersonateSiteID]
      ,[ImpersonateDomainName]
    FROM [RawStaging].[dbo].[RawClicks_Naked] WITH (NOLOCK);";

        [Ignore]
        [TestMethod]
        public void TestBulkCopyFromSQLServerToSQLite()
        {
            DataSet ds = new DataSet();
            using (SqlConnection sqlConn = new SqlConnection(@"Data Source=PROD-SQLRAWDATA.CORP.TUNNEY.COM;database=RawStaging;user=RawDataUser;password=aH3@#2A!hal#33;"))
            {
                sqlConn.Open();

                using (SqlDataAdapter da = new SqlDataAdapter(SQL_SELECT_RAWCLICKS, sqlConn))
                {
                    da.Fill(ds);
                }
            }

            //NOTE:  The USER must name their tables before embarking on this trip!! :)
            ds.Tables[0].TableName = "RawClicks_Naked";

            SQLiteDataStore dataStore = new SQLiteDataStore();
            dataStore.StoreData(ds);            

            IDataAdapter da2 = dataStore.CreateDataAdapter(@"SELECT * FROM RawClicks_Naked");
            DataSet ds2 = new DataSet();
            da2.Fill(ds2);

            Assert.AreEqual(10000, ds.Tables[0].Rows.Count);
        }
    }
}