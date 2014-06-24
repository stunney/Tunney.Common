using Tunney.Common.Data;

namespace Tunney.Common.Data
{
    public interface ISemaphoreFactoryReader
    {
        ISemaphoreFactory SemaphoreFactory { get; set; }
    }
}