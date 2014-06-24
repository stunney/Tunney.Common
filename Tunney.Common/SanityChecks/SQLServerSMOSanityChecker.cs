using System;

using Microsoft.SqlServer.Management.Smo;

namespace Tunney.Common.SanityChecks
{
    public class SQLServerSMOSanityChecker : ISanityChecker
    {
        public SQLServerSMOSanityChecker()
        {
        }

        public void Check()
        {
            //Should throw an exception from here I believe, or even when the class is invoked from IoC.
            Table table = new Table();
        }
    }
}