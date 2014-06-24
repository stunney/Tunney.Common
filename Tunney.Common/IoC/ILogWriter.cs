using System;

namespace Tunney.Common.IoC
{
    public interface ILogWriter
    {
        ILogger Logger { get; set; }
    }
}