using System;

namespace Tunney.Common.IoC
{
    public interface IContainerUser
    {
        IIoCContainer Container { get; set; }
    }
}