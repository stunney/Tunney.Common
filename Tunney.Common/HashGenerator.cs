using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Tunney.Common
{
    public interface IHashGenerator
    {
        byte[] Generate(DateTimeOffset _saltStamp, string _value);
        string GenerateStringFromHash(byte[] _hashBytes);
        byte[] GenerateHashFromHashString(string _stringOfHash);
        byte[] Generate(params object[] _values);
    }

    /// <summary>
    /// A non thread-safe class that created hashes in both string and byte[] form from strings.
    /// </summary>
    /// <remarks>
    /// This class is NOT thread-safe.  Use with caution!!!!
    /// </remarks>
    public class SHA1HashGenerator : IHashGenerator
    {
        /// <summary>
        /// Our hash generator.
        /// </summary>
        /// <remarks>
        /// Make sure this matches up with the method used in SQLServer for our computed columns.
        /// </remarks>
        //protected static readonly SHA1 s_hasher = SHA1.Create();

        /// <summary>
        /// Default == UTF8, which is NOT what SQLServer uses.
        /// Use Unicode (UTF-16) to encode your string bytes for accurate matchups with SQLServer calculated columns!  S.T.
        /// </summary>
        protected static readonly Encoding s_unicodeEncoding = UnicodeEncoding.Unicode;
                
        protected readonly SHA1 m_encryptor = SHA1.Create();

        /// <summary>
        /// Generates a hash of a single non-empty string value, possibly combined with a salt based on the passed in <paramref name="_saltStamp"/>.
        /// </summary>
        /// <param name="_saltStamp">
        /// The <see cref="DateTimeOffset"/> that can possibly be used to affect the resulting hash.
        /// </param>
        /// <param name="_value">
        /// The value to hash.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// You may not get the same hash every time you pass in the same <paramref name="_value"/>.  Ensure that you are consistent with the use of the <paramref name="_saltStamp"/> in
        /// relation to your data!
        /// </remarks>
        public virtual byte[] Generate(DateTimeOffset _saltStamp, string _value)
        {
            if (string.IsNullOrEmpty(_value)) throw new ArgumentNullException(@"_value");

            //Proposed Salt usage.
            //string concat = string.Format(@"{0}-{1}-{2}", _saltStamp.Month, _saltStamp.Day, _value);

            byte[] stringBytes = s_unicodeEncoding.GetBytes(_value.Trim().ToLower());
            byte[] hashData = m_encryptor.ComputeHash(stringBytes);

            return hashData;
        }

        public virtual string GenerateStringFromHash(byte[] _hashBytes)
        {
            string hashValue = string.Format(@"0x{0}", BitConverter.ToString(_hashBytes).Replace("-", string.Empty).ToUpper());
            return hashValue;
        }

        public virtual byte[] GenerateHashFromHashString(string _stringOfHash)
        {
            _stringOfHash = _stringOfHash.Substring(2, _stringOfHash.Length - 2).ToLower();

            int numberChars = _stringOfHash.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2) bytes[i / 2] = Convert.ToByte(_stringOfHash.Substring(i, 2), 16);
            return bytes;
        }

        protected internal const string GENERATE_APPEND_FORMAT = @"{0}|";

        /// <summary>
        /// Generates a hash of a set of strings values that matches the computed column values found in SQLServer columns using the CAKHashCreator Scalar-valued UDF.
        /// </summary>
        /// <param name="_values"></param>
        /// <returns></returns>
        public virtual byte[] Generate(params object[] _values)
        {
            if (null == _values) throw new ArgumentNullException(@"_values");

            StringBuilder retval = new StringBuilder();

            if (0 == _values.Length) return null;            

            foreach (object o in _values)
            {
                if (o == null) break;

                retval.AppendFormat(GENERATE_APPEND_FORMAT, o.ToString().Trim());
            }

            if (0 == retval.Length)
            {
                return null;
            }
            else
            {
                retval = retval.Remove(retval.Length - 1, 1);
            }

            try
            {
                byte[] stringBytes = s_unicodeEncoding.GetBytes(retval.ToString().ToLower());
                byte[] hashData = m_encryptor.ComputeHash(stringBytes);

//#if DEBUG
//                string hashValue = BitConverter.ToString(hashData).Replace("-", string.Empty); //For comparing to SQLServer's displayed value in SSMS.
//#endif

                return hashData;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public interface ICachingHashGenerator : IHashGenerator
    {
        byte[] Generate(Dictionary<string, byte[]> _cache, params object[] _values);
    }

    public class SHA1CachingHashGenerator : SHA1HashGenerator, ICachingHashGenerator
    {
        /// <summary>
        /// Generates a hash of a set of strings values that matches the computed column values found in SQLServer columns using the CAKHashCreator Scalar-valued UDF.
        /// </summary>
        /// <param name="_values"></param>
        /// <returns></returns>
        public virtual byte[] Generate(Dictionary<string, byte[]> _cache, params object[] _values)
        {
            if (null == _cache) throw new ArgumentNullException(@"_cache");
            if (null == _values) throw new ArgumentNullException(@"_values");

            if (0 == _values.Length) return null;

            StringBuilder key = new StringBuilder();

            foreach (object o in _values)
            {
                if (o == null) break;

                key.AppendFormat(GENERATE_APPEND_FORMAT, o.ToString().Trim());
            }

            if (0 == key.Length)
            {
                return null;
            }
            else
            {
                key.Remove(key.Length - 1, 1);
            }

            string retval = key.ToString();

            foreach (string k in _cache.Keys)
            {
                if (0 == string.Compare(k, retval, true))
                {
                    return _cache[k];
                }
            }

            try
            {
                byte[] stringBytes = s_unicodeEncoding.GetBytes(retval);
                byte[] hashData = m_encryptor.ComputeHash(stringBytes);

                _cache.Add(retval, hashData);

                return hashData;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}