using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunney.Common.IoC
{
    public interface IIoCContainer
    {
        /// <summary>
        /// Gets the object that is configured in your IoC 
        /// configuration block with the given <paramref name="_id"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to return, can be generalized to something like
        /// 'object', or an interface or base class, or as specific as the
        /// actual concrete type of the object.
        /// </typeparam>
        /// <param name="_id">
        /// The string identifier used to find the configuration block with the desired
        /// object to retrieve and instantiate.
        /// </param>
        /// <returns>
        /// The configured object, or an exception with details.  You might want to put a breakpoint in the constructor
        /// of your target object if you run into problems.
        /// </returns>
        T Resolve<T>(string _id);
    }
}