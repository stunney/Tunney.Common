using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Tunney.Common.Data
{
    [SQLiteFunction(Name = @"HASHBYTES", Arguments = 3, FuncType = FunctionType.Scalar)]
    [Serializable]
    public class SQLiteCustomFunctionHashBytes : SQLiteFunction
    {
        protected readonly IHashGenerator m_hashGenerator = new SHA1HashGenerator();

        public SQLiteCustomFunctionHashBytes()
            : base()
        {
        }

        
        public override object Invoke(object[] args)
        {
            List<object> objects = new List<object>(args.Length);
            for (int idx = 0; idx < args.Length; idx++)
            {
                if (args[idx] == null || args[idx] is DBNull)
                {
                    continue;
                }

                if (!(args[idx] is string))
                {
                    throw new ArgumentException(@"All args are expected to be strings for the CUSTOM function Tunney.Common.Data.SQLiteCustomFunctionHashBytes [HASHBYTES].", @"args");
                }

                objects.Add(args[idx]);
            }

            return m_hashGenerator.Generate(objects.ToArray());
        }
    }

    [SQLiteFunction(Name = @"CHARINDEX", Arguments = 2, FuncType = FunctionType.Scalar)]
    [Serializable]
    public class SQLiteCustomFunctionCharIndex : SQLiteFunction
    {
        public SQLiteCustomFunctionCharIndex() : base() { }

        /// <summary>
        /// Will find the index of the string within the string.
        /// 
        /// </summary>
        /// <param name="args">
        /// First argument is the string to search, the second argument is the string to search for within the first.
        /// </param>
        /// <returns>
        /// An <see cref="System.Int32"/> indicating the position within the first argument that the second argument resides.  -1 if not found.
        /// </returns>
        public override object Invoke(object[] args)
        {
            string searchee = (string)args[0];
            string criteria = (string)args[1];
            return searchee.IndexOf(criteria, 0);            
        }
    }

    [SQLiteFunction(Name = @"SUBSTR", Arguments = 3, FuncType = FunctionType.Scalar)]
    [Serializable]
    public class SQLiteCustomFunctionSubString : SQLiteFunction
    {
        public SQLiteCustomFunctionSubString() : base() { }

        /// <summary>
        /// Will find the string within the string.
        /// </summary>
        /// <param name="args">
        /// First element is the string to search.
        /// Second element is the start index.
        /// Third element is the number of character to return (length).
        /// </param>
        /// <returns>
        /// The <see cref="System.String"/> found within the first element of <paramref name="args"/>.
        /// </returns>
        public override object Invoke(object[] args)
        {
            string searchee = (string)args[0];
            int startIndex = (int)args[1];
            int length = (int)args[2];
            return searchee.Substring(startIndex, length);
        }
    }
}