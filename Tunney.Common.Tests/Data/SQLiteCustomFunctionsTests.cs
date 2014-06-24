using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Tunney.Common.Data;

namespace Tunney.Common.Tests.Data
{
    [TestClass]
    public class SQLiteCustomFunctionsTests
    {
        protected const string col2Format = @"Test{0}";
        protected const int maxSize = 1000;

        [Ignore]
        [TestMethod]
        public void TestMethod1()
        {
            IDataStore ds = new SQLiteDataStore();
            
            ds.ExecuteNonQuery(@"CREATE TABLE [MyTable] (col1 GLOB NOT NULL, col2 TEXT NOT NULL);", false);

            Task t1 = new Task(new Action<object>(InsertRowsThread1), ds);
            Task t2 = new Task(new Action<object>(InsertRowsThread2), ds);
            Task t3 = new Task(new Action<object>(InsertRowsThread3), ds);
            Task t4 = new Task(new Action<object>(InsertRowsThread4), ds);
            Task t5 = new Task(new Action<object>(InsertRowsThread5), ds);
            Task t6 = new Task(new Action<object>(InsertRowsThread6), ds);

            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();
            t5.Start();
            t6.Start();

            Task.WaitAll(new [] {t1, t2, t3, t4, t5, t6});

            DataSet dataSet = new DataSet();
            IDataAdapter da = ds.CreateDataAdapter(@"SELECT * FROM MyTable");
            da.Fill(dataSet);

            Assert.AreEqual(maxSize, dataSet.Tables[0].Rows.Count);

            for (int idx = 0; idx < maxSize - 1; idx++)
            {
                DataRow dr = dataSet.Tables[0].Rows[idx];
                //Assert.AreEqual(string.Format(col2Format, idx), (string)dr[1]);

                object o = dr[0];
                Assert.IsTrue(o is byte[]);
                byte[] b = (byte[]) o;
                Assert.AreEqual(20, b.Length);
                for (int idx2 = idx + 1; idx2 < maxSize; idx2++)
                {
                    DataRow dr2 = dataSet.Tables[0].Rows[idx2];
                    byte[] b2 = (byte[])dr2[0];
                    int matchCount = 0;
                    for (int i = 0; i < 20; i++)
                    {
                        if (b[i] == b2[i]) matchCount++;
                    }
                    Assert.AreNotEqual(20, matchCount);
                }
            }
        }

        protected virtual void InsertRowsThread1(object state)
        {
            IDataStore ds = (IDataStore) state;
            for (int idx = 0; idx < 200; idx++)
            {
                InsertRow(ds, string.Format(col2Format, idx));
                //Thread.Sleep(0);
            }
        }

        protected virtual void InsertRowsThread2(object state)
        {
            IDataStore ds = (IDataStore)state;
            for (int idx = 200; idx < 400; idx++)
            {
                InsertRow(ds, string.Format(col2Format, idx));
                //Thread.Sleep(0);
            }
        }
        protected virtual void InsertRowsThread3(object state)
        {
            IDataStore ds = (IDataStore)state;
            for (int idx = 400; idx < 600; idx++)
            {
                InsertRow(ds, string.Format(col2Format, idx));
                //Thread.Sleep(0);
            }
        }
        protected virtual void InsertRowsThread4(object state)
        {
            IDataStore ds = (IDataStore)state;
            for (int idx = 600; idx < 700; idx++)
            {
                InsertRow(ds, string.Format(col2Format, idx));
                //Thread.Sleep(0);
            }
        }
        protected virtual void InsertRowsThread5(object state)
        {
            IDataStore ds = (IDataStore)state;
            for (int idx = 700; idx < 850; idx++)
            {
                InsertRow(ds, string.Format(col2Format, idx));
                //Thread.Sleep(0);
            }
        }
        protected virtual void InsertRowsThread6(object state)
        {
            IDataStore ds = (IDataStore)state;
            for (int idx = 850; idx < 1000; idx++)
            {
                InsertRow(ds, string.Format(col2Format, idx));
                //Thread.Sleep(0);
            }
        }

        protected virtual void InsertRow(IDataStore _dataStore, string _value)
        {
            IDbCommand cmd = _dataStore.CreateCommand();
            cmd.CommandText = @"INSERT INTO MyTable (col1, col2) VALUES (HASHBYTES(@col2, NULL, NULL), @col2)";
            IDbDataParameter param1 = cmd.CreateParameter();
            param1.DbType = DbType.String;
            param1.Value = _value;
            param1.ParameterName = @"@col2";
            cmd.Parameters.Add(param1);
            int ret = cmd.ExecuteNonQuery();

            Assert.AreEqual(1, ret);
        }
    }
}