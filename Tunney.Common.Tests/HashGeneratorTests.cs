using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace Tunney.Common.Tests
{
    [TestClass]
    public class HashGeneratorTests
    {
        [TestMethod]
        public void TestGeneratedValues()
        {
            IHashGenerator hashMaker = new SHA1HashGenerator();

            List<string> testValues = new List<string>(10000);
            for (int idx = 0; idx < testValues.Capacity; idx++)
            {
                testValues.Add(string.Format(@"Mon{2}key-{0}-{1}-{0}", idx, DateTime.UtcNow, idx*6+2/2));
            }

            for (int idx = 0; idx < testValues.Count; idx++)
            {
                string testValue = testValues[idx];

                byte[] hash = hashMaker.Generate(DateTimeOffset.Now, testValue);

                string hashString = hashMaker.GenerateStringFromHash(hash);

                byte[] hash2 = hashMaker.GenerateHashFromHashString(hashString);

                Assert.AreEqual(hash.Length, hash2.Length);

                for(int idx2 = 0; idx2 < hash.Length; idx2++)
                    Assert.AreEqual(hash[idx2], hash2[idx2], string.Format(@"Hashes don't match for {0} with a hash string of {1} at index {2}-{3}", testValue, hashString, idx, idx2));
            }
        }

        [TestMethod]
        public void TestWoodStovesGeneration()
        {
            IHashGenerator hashMaker = new SHA1HashGenerator();

            byte[] hash = hashMaker.Generate(DateTimeOffset.Now, @"wood stoves");

            string hashString = hashMaker.GenerateStringFromHash(hash);

            Console.WriteLine(hashString);
        }

        [TestMethod]
        [Ignore]
        public void TestInsertGeneratedValuesIntoDatabase()
        {
            IHashGenerator hashMaker = new SHA1HashGenerator();

            List<string> testValues = new List<string>(10000);
            for (int idx = 0; idx < testValues.Capacity; idx++)
            {
                testValues.Add(string.Format(@"Mon{2}key-{0}-{1}-{0}", idx, DateTime.UtcNow, idx*6+2/2));
            }

            string connectionString = @"Data Source=localhost;Initial Catalog=dummydb;Integrated Security=True";

            const string INSERT = @"INSERT INTO [Test_ClickTermHashWithHexStringResult] ([ClickTerm], [CodeHexString], [ClickTermCodeSignature]) VALUES (@ClickTerm, @CodeHexString, @ClickTermCodeSignature)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach(string test in testValues)
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = INSERT;
                        cmd.CommandType = System.Data.CommandType.Text;

                        cmd.Parameters.Add(new SqlParameter(@"ClickTerm", test));

                        byte[] hash = hashMaker.Generate(DateTimeOffset.Now, test);

                        string hashString = hashMaker.GenerateStringFromHash(hash);

                        cmd.Parameters.Add(new SqlParameter(@"CodeHexString", hashString));

                        cmd.Parameters.Add(new SqlParameter(@"ClickTermCodeSignature", hash));

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}