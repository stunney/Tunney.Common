using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Random r = new Random();
            int listSize = r.Next(5000000);
            
            List<double> doubles = new List<double>(listSize);
            for (int idx = 0; idx < listSize; idx++)
            {
                doubles.Add(3.0d);
            }

            Thread.Sleep(3000);

            throw new ApplicationException(@"BigFloppyDonkey!");
        }
    }
}
