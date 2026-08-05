using System;
using System.Collections.Generic;

namespace CastleWindsorPlayground
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var data = new Dictionary<int, string[]>
            {
                { 1, new[] { "example" } }
            };

            MyObject o = new MyObject(data);
            Console.WriteLine("MyObject.Count = {0}", o.Count);
            Console.ReadLine();
        }
    }
}